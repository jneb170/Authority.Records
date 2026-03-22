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

The script now defaults to `eastus`. If the target resource group already exists, the script automatically reuses that resource group's location so reruns do not fail on region mismatches.

The script is also rerun-friendly for partial Azure setups:
- existing App Service plans are reused instead of recreated,
- existing UI/worker web apps are reused and reconfigured,
- if an existing UI or worker app is already attached to an App Service plan, the script adopts that existing plan for the rerun,
- if the existing UI and worker apps are attached to different plans, the script stops with a clear error instead of guessing.
- Azure SQL logical server creation now starts with the main deployment region and automatically tries a small fallback region list if Azure is temporarily blocking new SQL server provisioning there.
- You can also force a specific SQL region with `-SqlServerLocation`.

The script will create:
- **Resource Group**: `rg-authority-records`
- **Azure SQL Server** (unique name generated automatically)
- **Azure SQL Database**: `AuthorityRecords` (Serverless GP — auto-pauses when idle)
- **App Service Plan**: B1 Linux by default (low-cost dedicated baseline)
- **UI App Service**: `authority-records-ui`
- **Worker App Service**: optional; provision only when you intentionally want a dedicated worker host
- Connection strings are configured on the provisioned app services automatically

At the end it will print the exact values you need for GitHub secrets.

> **Low-cost baseline**: the script now defaults to a `B1` Linux App Service plan plus a small Azure SQL serverless configuration (`0.5` min vCores, `1` max vCore, `60` minute auto-pause). Scale up later if usage or features justify it.

If you want the cheapest supported starting point, skip the dedicated worker app:

```powershell
.\azure-provision.ps1 -SqlAdminPassword "YourStrongPassword123!" -SkipWorkerApp
```

If Azure is temporarily blocking new SQL logical server creation in your default region, you can also force a specific SQL region:

```powershell
.\azure-provision.ps1 -SqlAdminPassword "YourStrongPassword123!" -SqlServerLocation centralus
```

---

### Step 3 — Create the deploy service principal

The script output will show the exact command. It looks like:

```powershell
az ad sp create-for-rbac --name 'authority-records-deploy' --role contributor --scopes /subscriptions/005819af-adae-4fb5-a027-217bcb76399d/resourceGroups/rg-authority-records --json-auth
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

Only if you provisioned a dedicated worker app:

| Secret Name | Value |
|-------------|-------|
| `AZURE_WEBAPP_WORKER_NAME` | `authority-records-worker` (or your custom name) |

To enable worker deployments in GitHub Actions, also add a repository variable:

| Variable Name | Value |
|---------------|-------|
| `DEPLOY_WORKER_APP` | `true` |

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
4. ✅ Deploy Worker (only when `DEPLOY_WORKER_APP=true`)

First deploy takes ~5 min. Subsequent deploys ~2–3 min.

---

### Step 7 — Verify the deployment

- UI: https://authority-records-ui.azurewebsites.net
- Check App Service → Log stream in Azure Portal for any startup errors
- If you opted into the dedicated worker topology, verify the worker app separately in App Service.

---

## Architecture in Azure

```
GitHub Actions (push to main)
        │
        ├── Migrate AppDbContext   ─┐
        └── Migrate AuthDbContext  ─┤─► Azure SQL Database (AuthorityRecords)
                                    │         ▲              ▲
        ├── Deploy UI ──────────────┼─► App Service (UI)    │
        └── Deploy Worker (optional)┴─► App Service (Worker)─┘
```

---

## Important Notes

### Blazor Server & WebSockets
The provisioning script enables WebSockets and ARR Affinity (sticky sessions) on the UI app.
This is required for Blazor Server's SignalR circuits. If you ever scale to multiple instances,
you'll need Azure SignalR Service (free tier available) to handle cross-instance messaging.

### Scaling
- Start with `B1` for the lowest dedicated-cost baseline on a new deployment.
- Scale up to `B2`, `S1`, or higher if interactive load, CPU, or memory pressure warrants it.
- Add the dedicated worker app only when background throughput justifies a separate host.
- Blazor Server is stateful — scale out requires Azure SignalR Service.

### Worker topology
- The low-cost baseline is UI-only hosting; the UI host already runs the shared infrastructure hosted services used by the app.
- The dedicated worker app is an opt-in topology for cases where background throughput or operational isolation justifies the extra deployment surface.

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
