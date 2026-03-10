##############################################################################
# azure-provision.ps1
#
# One-time script to create all Azure resources for Authority.Records.
# Prerequisites:
#   1. Azure CLI installed: https://learn.microsoft.com/cli/azure/install-azure-cli
#   2. Log in first: az login
#
# Usage:
#   .\azure-provision.ps1 -SqlAdminPassword "***REMOVED***"
#
# You can override any default by passing additional parameters:
#   .\azure-provision.ps1 -Location "westus2" -UIAppName "my-rms-ui" -SqlAdminPassword "..."
##############################################################################

param(
    [string]$ResourceGroup   = "rg-authority-records",
    [string]$Location        = "eastus",
    # SQL Server name must be globally unique — a random suffix is appended automatically
    [string]$SqlServerBase   = "sql-authority-records",
    [string]$SqlAdminUser    = "AuthorityAdmin",
    [Parameter(Mandatory)]
    [string]$SqlAdminPassword,
    [string]$DatabaseName    = "AuthorityRecords",
    [string]$AppServicePlan  = "asp-authority-records",
    [string]$UIAppName       = "authority-records-ui",
    [string]$WorkerAppName   = "authority-records-worker"
)

# Generate a unique suffix so the SQL server name doesn't collide with other Azure customers
$suffix        = -join ((48..57) + (97..122) | Get-Random -Count 6 | ForEach-Object { [char]$_ })
$SqlServerName = "$SqlServerBase-$suffix"

Write-Host "`n=== Authority.Records — Azure Provisioning ===" -ForegroundColor Cyan
Write-Host "Resource Group : $ResourceGroup"
Write-Host "Location       : $Location"
Write-Host "SQL Server     : $SqlServerName"
Write-Host "UI App         : $UIAppName"
Write-Host "Worker App     : $WorkerAppName`n"

# ── 0. Verify az CLI is logged in ──────────────────────────────────────────
az account show --output none 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Not logged in. Run 'az login' first."
    exit 1
}

# ── 1. Resource Group ──────────────────────────────────────────────────────
Write-Host "Creating resource group..." -ForegroundColor Yellow
az group create `
    --name     $ResourceGroup `
    --location $Location `
    --output   table

# ── 2. Azure SQL Server ────────────────────────────────────────────────────
Write-Host "`nCreating Azure SQL Server (this takes ~2 min)..." -ForegroundColor Yellow
az sql server create `
    --name           $SqlServerName `
    --resource-group $ResourceGroup `
    --location       $Location `
    --admin-user     $SqlAdminUser `
    --admin-password $SqlAdminPassword `
    --output         table

# Allow GitHub Actions runners (all Azure-hosted IPs) to reach the SQL server
Write-Host "Opening firewall for Azure services (needed for CI migrations)..." -ForegroundColor Yellow
az sql server firewall-rule create `
    --server         $SqlServerName `
    --resource-group $ResourceGroup `
    --name           AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address   0.0.0.0 `
    --output           table

# ── 3. Azure SQL Database ──────────────────────────────────────────────────
# Serverless GP: auto-pauses after 60 min idle — cost-effective for dev/staging.
# For production under constant load, switch to a provisioned tier (e.g., S2 or GP_Gen5_2).
Write-Host "`nCreating SQL database..." -ForegroundColor Yellow
az sql db create `
    --name            $DatabaseName `
    --server          $SqlServerName `
    --resource-group  $ResourceGroup `
    --edition         GeneralPurpose `
    --family          Gen5 `
    --capacity        2 `
    --compute-model   Serverless `
    --auto-pause-delay 60 `
    --output          table

# ── 4. App Service Plan ────────────────────────────────────────────────────
# B2 minimum for Blazor Server: needs persistent WebSocket connections.
# Consider P1v3 for production workloads.
Write-Host "`nCreating App Service Plan (B2)..." -ForegroundColor Yellow
az appservice plan create `
    --name           $AppServicePlan `
    --resource-group $ResourceGroup `
    --location       $Location `
    --sku            B2 `
    --is-linux `
    --output         table

# Build the Azure SQL connection string once — reused for both apps
$connStr = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;User ID=$SqlAdminUser;***REMOVED***$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# ── 5. Blazor Server UI App ────────────────────────────────────────────────
Write-Host "`nCreating UI App Service..." -ForegroundColor Yellow
az webapp create `
    --name           $UIAppName `
    --resource-group $ResourceGroup `
    --plan           $AppServicePlan `
    --runtime        "DOTNETCORE:9.0" `
    --output         table

# WebSockets required for Blazor Server (SignalR)
az webapp config set `
    --name               $UIAppName `
    --resource-group     $ResourceGroup `
    --web-sockets-enabled true `
    --output             none

# ARR Affinity (sticky sessions) — keeps each user on the same instance
# Important for Blazor Server's stateful circuit model
az webapp update `
    --name           $UIAppName `
    --resource-group $ResourceGroup `
    --client-affinity-enabled true `
    --output         none

# Set production environment + connection string
az webapp config appsettings set `
    --name           $UIAppName `
    --resource-group $ResourceGroup `
    --settings       ASPNETCORE_ENVIRONMENT=Production `
    --output         none

az webapp config connection-string set `
    --name                  $UIAppName `
    --resource-group        $ResourceGroup `
    --settings              DefaultConnection="$connStr" `
    --connection-string-type SQLServer `
    --output                none

# ── 6. Background Worker App ───────────────────────────────────────────────
Write-Host "`nCreating Worker App Service..." -ForegroundColor Yellow
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
    --name                  $WorkerAppName `
    --resource-group        $ResourceGroup `
    --settings              DefaultConnection="$connStr" `
    --connection-string-type SQLServer `
    --output                none

# ── 7. Summary ─────────────────────────────────────────────────────────────
Write-Host "`n✅  Provisioning complete!" -ForegroundColor Green
Write-Host "───────────────────────────────────────────────────────────"
Write-Host "SQL Server  : $SqlServerName.database.windows.net"
Write-Host "UI URL      : https://$UIAppName.azurewebsites.net"
Write-Host "Worker URL  : https://$WorkerAppName.azurewebsites.net"

Write-Host "`n📋  GitHub Secrets to add (Settings → Secrets → Actions):"
Write-Host "  AZURE_SQL_CONNECTION_STRING  =  $connStr"
Write-Host "  AZURE_WEBAPP_UI_NAME         =  $UIAppName"
Write-Host "  AZURE_WEBAPP_WORKER_NAME     =  $WorkerAppName"
Write-Host "  AZURE_CREDENTIALS            =  (see step below)`n"

Write-Host "🔑  Create the deploy service principal (run this once):" -ForegroundColor Cyan
$subId = (az account show --query id -o tsv)
Write-Host "  az ad sp create-for-rbac ``"
Write-Host "    --name 'authority-records-deploy' ``"
Write-Host "    --role contributor ``"
Write-Host "    --scopes /subscriptions/$subId/resourceGroups/$ResourceGroup ``"
Write-Host "    --json-auth"
Write-Host ""
Write-Host "  Copy the JSON output and save it as the AZURE_CREDENTIALS secret.`n"
