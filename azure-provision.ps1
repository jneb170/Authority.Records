##############################################################################
# azure-provision.ps1
#
# One-time script to create all Azure resources for Authority.Records.
# Prerequisites:
#   1. Azure CLI installed: https://learn.microsoft.com/cli/azure/install-azure-cli
#   2. Log in first: az login
#
# Usage:
#   .\azure-provision.ps1 -SqlAdminPassword "YourStrongPassword123!"
#
# Re-run after a partial failure (reuse existing SQL server name):
#   .\azure-provision.ps1 -SqlAdminPassword "..." -ExistingSqlServerName "sql-authority-records-abc123"
##############################################################################

param(
    [string]$ResourceGroup          = "rg-authority-records",
    [string]$Location               = "eastus",
    # Optional override for the Azure SQL logical server region. If omitted, the script
    # starts with the main deployment region and falls back to alternate regions when Azure
    # temporarily blocks new SQL logical server creation there.
    [string]$SqlServerLocation      = "",
    [string]$SqlServerBase          = "sql-authority-records",
    [string]$SqlAdminUser           = "AuthorityAdmin",
    [Parameter(Mandatory)]
    [string]$SqlAdminPassword,
    [string]$DatabaseName           = "AuthorityRecords",
    [string]$AppServicePlan         = "asp-authority-records",
    [string]$UIAppName              = "authority-records-ui",
    [string]$WorkerAppName          = "authority-records-worker",
    # Start with a lower-cost dedicated plan and scale up only if load or features require it.
    [string]$AppServiceSku          = "B1",
    [double]$SqlMinCapacity         = 0.5,
    [int]$SqlMaxCapacity            = 1,
    [int]$SqlAutoPauseDelayMinutes  = 60,
    # Provide a name from a previous partial run to reuse an existing SQL server.
    # The script will verify the server exists before skipping creation.
    [string]$ExistingSqlServerName  = "",
    # Skip the dedicated worker app for the lower-cost baseline.
    # The Blazor UI host already runs the shared infrastructure hosted services.
    [switch]$SkipWorkerApp
)

$suffix        = -join ((48..57) + (97..122) | Get-Random -Count 6 | ForEach-Object { [char]$_ })
$SqlServerName = if ($ExistingSqlServerName) { $ExistingSqlServerName } else { "$SqlServerBase-$suffix" }

# 0. Verify az CLI is logged in
az account show --output none 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Not logged in. Run az login first."
    exit 1
}

# Resolve the effective location before creating any regional resources.
$resourceGroupExists = (az group exists --name $ResourceGroup --output tsv).Trim()
if ($LASTEXITCODE -ne 0) {
    Write-Error "Could not determine whether resource group $ResourceGroup exists."
    exit 1
}

if ($resourceGroupExists -eq "true") {
    $existingLocation = (az group show --name $ResourceGroup --query location --output tsv).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($existingLocation)) {
        Write-Error "Could not read the existing location for resource group $ResourceGroup."
        exit 1
    }

    if ($existingLocation -ne $Location) {
        Write-Host "Resource group $ResourceGroup already exists in $existingLocation. Reusing that location instead of requested $Location." -ForegroundColor Yellow
        $Location = $existingLocation
    }
}

if ([string]::IsNullOrWhiteSpace($SqlServerLocation)) {
    $SqlServerLocation = $Location
}

$sqlLocationCandidates = New-Object System.Collections.Generic.List[string]
foreach ($candidate in @($SqlServerLocation, $Location, "centralus", "southcentralus", "westus2", "westus3", "eastus2")) {
    if (-not [string]::IsNullOrWhiteSpace($candidate) -and -not $sqlLocationCandidates.Contains($candidate)) {
        $null = $sqlLocationCandidates.Add($candidate)
    }
}

function Get-AppServicePlanNameFromId {
    param([string]$ServerFarmId)

    if ([string]::IsNullOrWhiteSpace($ServerFarmId)) {
        return ""
    }

    return ($ServerFarmId.TrimEnd('/') -split '/')[-1]
}

function Resolve-WebAppPlanName {
    param($WebApp)

    if ($null -eq $WebApp) {
        return ""
    }

    foreach ($candidateId in @($WebApp.serverFarmId, $WebApp.appServicePlanId)) {
        $planName = Get-AppServicePlanNameFromId $candidateId
        if (-not [string]::IsNullOrWhiteSpace($planName)) {
            return $planName
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($WebApp.id)) {
        $resourcePlanId = az resource show `
            --ids    $WebApp.id `
            --query  "properties.serverFarmId" `
            --output tsv 2>$null
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($resourcePlanId)) {
            return Get-AppServicePlanNameFromId $resourcePlanId.Trim()
        }
    }

    return ""
}

Write-Host ""
Write-Host "=== Authority.Records - Azure Provisioning ===" -ForegroundColor Cyan
Write-Host "Resource Group : $ResourceGroup"
Write-Host "Location       : $Location"
Write-Host "SQL Region     : $SqlServerLocation"
Write-Host "SQL Server     : $SqlServerName"
Write-Host "UI App         : $UIAppName"
Write-Host "Worker App     : $(if ($SkipWorkerApp) { '(skipped)' } else { $WorkerAppName })"
Write-Host "App Svc SKU    : $AppServiceSku"
Write-Host "SQL Max vCores : $SqlMaxCapacity"
Write-Host "SQL Min vCores : $SqlMinCapacity"
Write-Host "SQL AutoPause  : $SqlAutoPauseDelayMinutes min"
Write-Host ""

# 1. Register required resource providers (needed on first use of a subscription)
Write-Host "Registering Azure resource providers (takes ~1-2 min on first run)..." -ForegroundColor Yellow
az provider register --namespace Microsoft.Sql --wait --output none
Write-Host "  Microsoft.Sql - registered"
az provider register --namespace Microsoft.Web --wait --output none
Write-Host "  Microsoft.Web - registered"
Write-Host ""

# 2. Resource Group
Write-Host "Creating resource group..." -ForegroundColor Yellow
az group create `
    --name     $ResourceGroup `
    --location $Location `
    --output   table

# 3. Azure SQL Server
$needCreateServer = $true
if ($ExistingSqlServerName) {
    Write-Host ""
    Write-Host "Checking if SQL Server $SqlServerName exists..." -ForegroundColor Yellow
    $existingSqlServerLocation = az sql server show `
        --name           $SqlServerName `
        --resource-group $ResourceGroup `
        --query          "location" `
        --output         tsv 2>$null
    if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($existingSqlServerLocation)) {
        $SqlServerLocation = $existingSqlServerLocation.Trim()
        Write-Host "SQL Server $SqlServerName already exists in $SqlServerLocation; skipping creation."
        $needCreateServer = $false
    } else {
        Write-Host "SQL Server $SqlServerName not found - will create it."
    }
}

if ($needCreateServer) {
    Write-Host ""
    $createdSqlServer = $false
    foreach ($candidateLocation in $sqlLocationCandidates) {
        Write-Host "Creating Azure SQL Server in $candidateLocation (this takes ~2 min)..." -ForegroundColor Yellow
        $sqlCreateOutput = az sql server create `
            --name           $SqlServerName `
            --resource-group $ResourceGroup `
            --location       $candidateLocation `
            --admin-user     $SqlAdminUser `
            --admin-password $SqlAdminPassword `
            --output         table 2>&1

        if ($LASTEXITCODE -eq 0) {
            $SqlServerLocation = $candidateLocation
            if (-not [string]::IsNullOrWhiteSpace($sqlCreateOutput)) {
                Write-Host $sqlCreateOutput
            }
            $createdSqlServer = $true
            break
        }

        if ($sqlCreateOutput -match "RegionDoesNotAllowProvisioning") {
            Write-Host "Azure is not currently accepting new SQL logical servers in $candidateLocation. Trying the next fallback region..." -ForegroundColor Yellow
            continue
        }

        Write-Host $sqlCreateOutput -ForegroundColor Red
        Write-Host "SQL Server creation failed. Fix the error then re-run with:" -ForegroundColor Red
        Write-Host "  -ExistingSqlServerName $SqlServerName" -ForegroundColor Yellow
        exit 1
    }

    if (-not $createdSqlServer) {
        Write-Host "SQL Server creation failed in all candidate regions: $($sqlLocationCandidates -join ', ')." -ForegroundColor Red
        Write-Host "Re-run with an explicit SQL region if you know a region that is currently accepting new Azure SQL logical servers:" -ForegroundColor Yellow
        Write-Host "  .\azure-provision.ps1 -SqlAdminPassword ... -SqlServerLocation centralus" -ForegroundColor Yellow
        exit 1
    }
}

az sql server firewall-rule create `
    --server           $SqlServerName `
    --resource-group   $ResourceGroup `
    --name             AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address   0.0.0.0 `
    --output           none
Write-Host "Firewall rule set for Azure services."

# 4. Azure SQL Database
Write-Host ""
Write-Host "Creating SQL database..." -ForegroundColor Yellow
az sql db create `
    --name             $DatabaseName `
    --server           $SqlServerName `
    --resource-group   $ResourceGroup `
    --edition          GeneralPurpose `
    --family           Gen5 `
    --capacity         $SqlMaxCapacity `
    --min-capacity     $SqlMinCapacity `
    --compute-model    Serverless `
    --auto-pause-delay $SqlAutoPauseDelayMinutes `
    --output           table
if ($LASTEXITCODE -ne 0) {
    Write-Host "SQL Database creation failed." -ForegroundColor Red
    exit 1
}

# 5. Discover existing app topology before picking the effective App Service Plan.
$uiAppJson = az webapp show `
    --name           $UIAppName `
    --resource-group $ResourceGroup `
    --output         json 2>$null
$uiAppExists = $LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($uiAppJson)
$uiExistingPlanName = ""
if ($uiAppExists) {
    $uiApp = $uiAppJson | ConvertFrom-Json
    $uiExistingPlanName = Resolve-WebAppPlanName $uiApp
    if ([string]::IsNullOrWhiteSpace($uiExistingPlanName)) {
        Write-Host "UI App $UIAppName exists, but its current App Service Plan could not be determined." -ForegroundColor Red
        exit 1
    }
}

$workerAppExists = $false
$workerAppJson = ""
$workerExistingPlanName = ""
if (-not $SkipWorkerApp) {
    $workerAppJson = az webapp show `
        --name           $WorkerAppName `
        --resource-group $ResourceGroup `
        --output         json 2>$null
    $workerAppExists = $LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($workerAppJson)
    if ($workerAppExists) {
        $workerApp = $workerAppJson | ConvertFrom-Json
        $workerExistingPlanName = Resolve-WebAppPlanName $workerApp
        if ([string]::IsNullOrWhiteSpace($workerExistingPlanName)) {
            Write-Host "Worker App $WorkerAppName exists, but its current App Service Plan could not be determined." -ForegroundColor Red
            exit 1
        }
    }
}

$adoptedExistingPlanName = ""
if (-not [string]::IsNullOrWhiteSpace($uiExistingPlanName)) {
    $adoptedExistingPlanName = $uiExistingPlanName
    if ($uiExistingPlanName -ne $AppServicePlan) {
        Write-Host "UI App $UIAppName already exists on App Service Plan $uiExistingPlanName. Reusing that plan instead of requested $AppServicePlan." -ForegroundColor Yellow
        $AppServicePlan = $uiExistingPlanName
    }
}

if (-not [string]::IsNullOrWhiteSpace($workerExistingPlanName)) {
    if (-not [string]::IsNullOrWhiteSpace($adoptedExistingPlanName) -and $workerExistingPlanName -ne $adoptedExistingPlanName) {
        Write-Host "Existing UI and Worker apps are attached to different App Service Plans ($adoptedExistingPlanName vs. $workerExistingPlanName)." -ForegroundColor Red
        Write-Host "Choose consistent app names or manually consolidate the apps onto one plan before rerunning." -ForegroundColor Yellow
        exit 1
    }

    $adoptedExistingPlanName = $workerExistingPlanName
    if ($workerExistingPlanName -ne $AppServicePlan) {
        Write-Host "Worker App $WorkerAppName already exists on App Service Plan $workerExistingPlanName. Reusing that plan instead of requested $AppServicePlan." -ForegroundColor Yellow
        $AppServicePlan = $workerExistingPlanName
    }
}

# 6. App Service Plan
Write-Host ""
Write-Host "Ensuring App Service Plan ($AppServicePlan)..." -ForegroundColor Yellow
$existingPlanJson = az appservice plan show `
    --name           $AppServicePlan `
    --resource-group $ResourceGroup `
    --query          "{location:location,sku:sku.name,reserved:reserved}" `
    --output         json 2>$null

if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($existingPlanJson)) {
    $existingPlan = $existingPlanJson | ConvertFrom-Json
    Write-Host "App Service Plan $AppServicePlan already exists; reusing it."
    if ($existingPlan.location -ne $Location) {
        Write-Host "  Existing plan location: $($existingPlan.location)" -ForegroundColor Yellow
    }
    if ($existingPlan.sku -ne $AppServiceSku) {
        Write-Host "  Existing plan SKU is $($existingPlan.sku); requested SKU was $AppServiceSku." -ForegroundColor Yellow
    }
} else {
    az appservice plan create `
        --name           $AppServicePlan `
        --resource-group $ResourceGroup `
        --location       $Location `
        --sku            $AppServiceSku `
        --is-linux `
        --output         table
    if ($LASTEXITCODE -ne 0) {
        Write-Host "App Service Plan creation failed." -ForegroundColor Red
        Write-Host ""
        Write-Host "Quota error troubleshooting:" -ForegroundColor Yellow
        Write-Host "  - Basic/Standard/Premium App Service plans require dedicated VM quota in the selected region."
        Write-Host "  - Request a small core increase at: Azure Portal -> Subscriptions -> Usage + quotas"
        Write-Host "  - Start with B1 for the lowest dedicated-cost baseline, then scale up if needed."
        Write-Host "  - Then re-run: .\azure-provision.ps1 -SqlAdminPassword ... -ExistingSqlServerName $SqlServerName"
        exit 1
    }
}

$connStr = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;User ID=$SqlAdminUser;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 7. UI App Service
Write-Host ""
Write-Host "Ensuring UI App Service..." -ForegroundColor Yellow
if ($uiAppExists) {
    Write-Host "UI App Service $UIAppName already exists; reusing it."
} else {
    az webapp create `
        --name           $UIAppName `
        --resource-group $ResourceGroup `
        --plan           $AppServicePlan `
        --runtime        "DOTNETCORE:9.0" `
        --output         table
    if ($LASTEXITCODE -ne 0) {
        Write-Host "UI App Service creation failed." -ForegroundColor Red
        exit 1
    }
}
az webapp config set `
    --name                $UIAppName `
    --resource-group      $ResourceGroup `
    --web-sockets-enabled true `
    --output              none
az webapp update `
    --name                    $UIAppName `
    --resource-group          $ResourceGroup `
    --client-affinity-enabled true `
    --output                  none
az webapp config appsettings set `
    --name           $UIAppName `
    --resource-group $ResourceGroup `
    --settings       ASPNETCORE_ENVIRONMENT=Production `
    --output         none
az webapp config connection-string set `
    --name                   $UIAppName `
    --resource-group         $ResourceGroup `
    --settings               DefaultConnection="$connStr" `
    --connection-string-type SQLServer `
    --output                 none
Write-Host "UI App configured."

if (-not $SkipWorkerApp) {
    # 8. Worker App Service
    Write-Host ""
    Write-Host "Ensuring Worker App Service..." -ForegroundColor Yellow
    if ($workerAppExists) {
        Write-Host "Worker App Service $WorkerAppName already exists; reusing it."
    } else {
        az webapp create `
            --name           $WorkerAppName `
            --resource-group $ResourceGroup `
            --plan           $AppServicePlan `
            --runtime        "DOTNETCORE:9.0" `
            --output         table
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Worker App Service creation failed." -ForegroundColor Red
            exit 1
        }
    }
    az webapp config appsettings set `
        --name           $WorkerAppName `
        --resource-group $ResourceGroup `
        --settings       ASPNETCORE_ENVIRONMENT=Production `
        --output         none
    az webapp config connection-string set `
        --name                   $WorkerAppName `
        --resource-group         $ResourceGroup `
        --settings               DefaultConnection="$connStr" `
        --connection-string-type SQLServer `
        --output                 none
    Write-Host "Worker App configured."
}

# 8. Summary
Write-Host ""
Write-Host "[OK] Provisioning complete!" -ForegroundColor Green
Write-Host "-----------------------------------------------------------"
Write-Host "SQL Server  : $SqlServerName.database.windows.net"
Write-Host "UI URL      : https://$UIAppName.azurewebsites.net"
if (-not $SkipWorkerApp) {
    Write-Host "Worker URL  : https://$WorkerAppName.azurewebsites.net"
} else {
    Write-Host "Worker URL  : (not provisioned)"
}
Write-Host ""
Write-Host "GitHub Secrets to add (Settings -> Secrets -> Actions):"
Write-Host "  AZURE_SQL_CONNECTION_STRING  =  $connStr"
Write-Host "  AZURE_WEBAPP_UI_NAME         =  $UIAppName"
if (-not $SkipWorkerApp) {
    Write-Host "  AZURE_WEBAPP_WORKER_NAME     =  $WorkerAppName"
    Write-Host "  DEPLOY_WORKER_APP (repo var) =  true"
} else {
    Write-Host "  AZURE_WEBAPP_WORKER_NAME     =  (skip for low-cost baseline)"
    Write-Host "  DEPLOY_WORKER_APP (repo var) =  leave unset"
}
Write-Host "  AZURE_CREDENTIALS            =  (see command below)"
Write-Host ""
Write-Host "Run this to create the deploy service principal:" -ForegroundColor Cyan
$subId = (az account show --query id -o tsv)
Write-Host "  az ad sp create-for-rbac --name authority-records-deploy --role contributor --scopes /subscriptions/$subId/resourceGroups/$ResourceGroup --json-auth"
Write-Host ""
Write-Host "Copy the JSON output and save it as the AZURE_CREDENTIALS GitHub secret."
Write-Host ""
