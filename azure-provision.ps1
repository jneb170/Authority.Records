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
    [string]$Location               = "eastus2",
    [string]$SqlServerBase          = "sql-authority-records",
    [string]$SqlAdminUser           = "AuthorityAdmin",
    [Parameter(Mandatory)]
    [string]$SqlAdminPassword,
    [string]$DatabaseName           = "AuthorityRecords",
    [string]$AppServicePlan         = "asp-authority-records",
    [string]$UIAppName              = "authority-records-ui",
    [string]$WorkerAppName          = "authority-records-worker",
    # P1V2 (Premium V2) uses a separate quota pool from Standard/Basic VMs.
    # On new Azure subscriptions, Standard VMs quota is 0. P1V2 avoids this.
    # Cost: ~$73/mo. If you also hit P1V2 quota, request an increase at:
    #   Azure Portal -> Subscriptions -> Your Subscription -> Usage + quotas
    [string]$AppServiceSku          = "P1V2",
    # Provide a name from a previous partial run to reuse an existing SQL server.
    # The script will verify the server exists before skipping creation.
    [string]$ExistingSqlServerName  = ""
)

$suffix        = -join ((48..57) + (97..122) | Get-Random -Count 6 | ForEach-Object { [char]$_ })
$SqlServerName = if ($ExistingSqlServerName) { $ExistingSqlServerName } else { "$SqlServerBase-$suffix" }

Write-Host ""
Write-Host "=== Authority.Records - Azure Provisioning ===" -ForegroundColor Cyan
Write-Host "Resource Group : $ResourceGroup"
Write-Host "Location       : $Location"
Write-Host "SQL Server     : $SqlServerName"
Write-Host "UI App         : $UIAppName"
Write-Host "Worker App     : $WorkerAppName"
Write-Host "App Svc SKU    : $AppServiceSku"
Write-Host ""

# 0. Verify az CLI is logged in
az account show --output none 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Not logged in. Run az login first."
    exit 1
}

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
    az sql server show `
        --name           $SqlServerName `
        --resource-group $ResourceGroup `
        --output         none 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "SQL Server $SqlServerName already exists, skipping creation."
        $needCreateServer = $false
    } else {
        Write-Host "SQL Server $SqlServerName not found - will create it."
    }
}

if ($needCreateServer) {
    Write-Host ""
    Write-Host "Creating Azure SQL Server (this takes ~2 min)..." -ForegroundColor Yellow
    az sql server create `
        --name           $SqlServerName `
        --resource-group $ResourceGroup `
        --location       $Location `
        --admin-user     $SqlAdminUser `
        --admin-password $SqlAdminPassword `
        --output         table
    if ($LASTEXITCODE -ne 0) {
        Write-Host "SQL Server creation failed. Fix the error then re-run with:" -ForegroundColor Red
        Write-Host "  -ExistingSqlServerName $SqlServerName" -ForegroundColor Yellow
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
    --capacity         2 `
    --compute-model    Serverless `
    --auto-pause-delay 60 `
    --output           table
if ($LASTEXITCODE -ne 0) {
    Write-Host "SQL Database creation failed." -ForegroundColor Red
    exit 1
}

# 5. App Service Plan
Write-Host ""
Write-Host "Creating App Service Plan ($AppServiceSku)..." -ForegroundColor Yellow
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
    Write-Host "  - P1V2 (default): Premium V2 quota. New subscriptions may need a quota increase."
    Write-Host "  - Request 1 core at: Azure Portal -> Subscriptions -> Usage + quotas"
    Write-Host "  - Filter for your region (eastus) and request Standard Dv2 Family vCPUs = 1"
    Write-Host "  - Quota increases for 1-2 cores are usually auto-approved in minutes."
    Write-Host "  - Then re-run: .\azure-provision.ps1 -SqlAdminPassword ... -ExistingSqlServerName $SqlServerName"
    exit 1
}

$connStr = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;User ID=$SqlAdminUser;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 6. UI App Service
Write-Host ""
Write-Host "Creating UI App Service..." -ForegroundColor Yellow
az webapp create `
    --name           $UIAppName `
    --resource-group $ResourceGroup `
    --plan           $AppServicePlan `
    --runtime        "DOTNETCORE:9.0" `
    --output         table
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

# 7. Worker App Service
Write-Host ""
Write-Host "Creating Worker App Service..." -ForegroundColor Yellow
az webapp create `
    --name           $WorkerAppName `
    --resource-group $ResourceGroup `
    --plan           $AppServicePlan `
    --runtime        "DOTNETCORE:9.0" `
    --output         table
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

# 8. Summary
Write-Host ""
Write-Host "[OK] Provisioning complete!" -ForegroundColor Green
Write-Host "-----------------------------------------------------------"
Write-Host "SQL Server  : $SqlServerName.database.windows.net"
Write-Host "UI URL      : https://$UIAppName.azurewebsites.net"
Write-Host "Worker URL  : https://$WorkerAppName.azurewebsites.net"
Write-Host ""
Write-Host "GitHub Secrets to add (Settings -> Secrets -> Actions):"
Write-Host "  AZURE_SQL_CONNECTION_STRING  =  $connStr"
Write-Host "  AZURE_WEBAPP_UI_NAME         =  $UIAppName"
Write-Host "  AZURE_WEBAPP_WORKER_NAME     =  $WorkerAppName"
Write-Host "  AZURE_CREDENTIALS            =  (see command below)"
Write-Host ""
Write-Host "Run this to create the deploy service principal:" -ForegroundColor Cyan
$subId = (az account show --query id -o tsv)
Write-Host "  az ad sp create-for-rbac --name authority-records-deploy --role contributor --scopes /subscriptions/$subId/resourceGroups/$ResourceGroup --json-auth"
Write-Host ""
Write-Host "Copy the JSON output and save it as the AZURE_CREDENTIALS GitHub secret."
Write-Host ""
