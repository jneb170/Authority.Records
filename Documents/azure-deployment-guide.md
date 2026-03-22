# Authority.Records — Azure Deployment Walkthrough

## What was set up for you

| File | Purpose |
|------|---------|
| `azure-provision.ps1` | One-time script that creates all Azure resources |
| `.github/workflows/deploy.yml` | CI/CD: builds → migrates → deploys on every push to `main` |
| `Modules.Records.UI/appsettings.Production.json` | Production logging config (no secrets) |
| `Api/appsettings.Production.json` | Production logging config for worker |
| Both context factories updated | Now read env vars so CI migrations work |

---

## Step-by-Step Guide

### Prerequisites (install once)

1. **Azure CLI** — https://learn.microsoft.com/cli/azure/install-azure-cli-windows
2. **Azure Subscription** — https://azure.microsoft.com/free (free tier available)
3. Your GitHub repo must be connected to `github.com` (push the changes below first)

---

### Step 1 — Move local DB credentials out of appsettings.json

The current `appsettings.json` has your local SQL password in plain text. Move it to a
Development override that won't be committed:

1. Create `Modules.Records.UI/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AuthorityRecords;User Id=AuthorityAdmin;Password=idtv3Cc6RhLFt8!;TrustServerCertificate=True;"
  }
}
```

2. Create `Api/appsettings.Development.json` with the same content.

3. Remove the `ConnectionStrings` block from both `appsettings.json` files.

`appsettings.Development.json` is now in `.gitignore` and will never be committed.

---

### Step 2 — Provision Azure resources (one time)

Open PowerShell and run:

```powershell
az login

cd C:\Users\jneb1\source\repos\Authority.Records

.\azure-provision.ps1 -SqlAdminPassword "YourStrongPassword123!"
```

The script will create:
- **Resource Group**: `rg-authority-records`
- **Azure SQL Server** (unique name generated automatically)
- **Azure SQL Database**: `AuthorityRecords` (Serverless GP — auto-pauses when idle)
- **App Service Plan**: B2 Linux (minimum for Blazor Server WebSockets)
- **UI App Service**: `authority-records-ui`
- **Worker App Service**: `authority-records-worker`
- Connection strings are configured on both app services automatically

At the end it will print the exact values you need for GitHub secrets.

> **Cost estimate**: ~$25–35/month for B2 plan + Serverless SQL. Scale down with `-AppServicePlan B1` for dev/staging (but B1 doesn't support WebSockets — use for worker only).

---

### Step 3 — Create the deploy service principal

The script output will show the exact command. It looks like:

```powershell
az ad sp create-for-rbac `
  --name 'authority-records-deploy' `
  --role contributor `
  --scopes /subscriptions/{your-sub-id}/resourceGroups/rg-authority-records `
  --json-auth
```

Copy the entire JSON output — you'll use it in the next step.

---

### Step 4 — Add GitHub Secrets

In your GitHub repo: **Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Value |
|-------------|-------|
| `AZURE_CREDENTIALS` | The JSON from Step 3 |
| `AZURE_SQL_CONNECTION_STRING` | Printed by `azure-provision.ps1` |
| `AZURE_WEBAPP_UI_NAME` | `authority-records-ui` (or your custom name) |
| `AZURE_WEBAPP_WORKER_NAME` | `authority-records-worker` (or your custom name) |

---

### Step 5 — Create a GitHub Environment (optional but recommended)

This adds manual approval before production deploys.

1. Go to **Settings → Environments → New environment**
2. Name it `production`
3. Add required reviewers if desired

The workflow already uses `environment: production` — without this env defined, deploys still work but skip approval.

---

### Step 6 — Push to trigger first deploy

```bash
git add .
git commit -m "Add Azure deployment config"
git push origin main
```

Go to **Actions** tab in GitHub to watch the workflow run. The order is:
1. ✅ Build & Test
2. ✅ Apply DB Migrations (creates tables in Azure SQL)
3. ✅ Deploy UI  
4. ✅ Deploy Worker (parallel with UI)

First deploy takes ~5 min. Subsequent deploys ~2–3 min.

---

### Step 7 — Verify the deployment

- UI: https://authority-records-ui.azurewebsites.net
- Check App Service → Log stream in Azure Portal for any startup errors

---

## Architecture in Azure

```
GitHub Actions (push to main)
        │
        ├── Migrate AppDbContext   ─┐
        └── Migrate AuthDbContext  ─┤─► Azure SQL Database (AuthorityRecords)
                                    │         ▲              ▲
        ├── Deploy UI ──────────────┼─► App Service (UI)    │
        └── Deploy Worker ──────────┴─► App Service (Worker)─┘
```

---

## Important Notes

### Blazor Server & WebSockets
The provisioning script enables WebSockets and ARR Affinity (sticky sessions) on the UI app.
This is required for Blazor Server's SignalR circuits. If you ever scale to multiple instances,
you'll need Azure SignalR Service (free tier available) to handle cross-instance messaging.

### Scaling
- B2 handles ~50–100 concurrent Blazor users  
- For more load: upgrade to P1v3 (`az appservice plan update --sku P1V3`)
- Blazor Server is stateful — scale out requires Azure SignalR Service

### Connection Strings
Azure App Service injects connection strings as environment variables at runtime:
`SQLCONNSTR_DefaultConnection` → maps to `ConnectionStrings:DefaultConnection` in .NET config.
Your `appsettings.json` values are overridden automatically — no code changes needed.

### Database Backups
Azure SQL automatically backs up your database:
- Full backup: weekly
- Differential: every 12 hours  
- Transaction logs: every 5–12 min
- Retention: 7 days (configurable up to 35)

### Custom Domain (optional)
```powershell
az webapp config hostname add `
  --webapp-name authority-records-ui `
  --resource-group rg-authority-records `
  --hostname yourdomain.com
```
Then add a free managed TLS cert:
```powershell
az webapp config ssl create `
  --name authority-records-ui `
  --resource-group rg-authority-records `
  --hostname yourdomain.com
```
