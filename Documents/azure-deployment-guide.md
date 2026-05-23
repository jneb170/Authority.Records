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
az ad sp create-for-rbac --name 'authority-records-deploy' --role contributor --scopes /subscriptions/{your-sub-id}/resourceGroups/rg-authority-records --json-auth
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

You no longer need worker-app deployment secrets for the default low-cost topology.

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
                                    │         ▲
        └── Deploy UI ──────────────┴─► App Service (UI)
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
- Add a dedicated worker app later only if background throughput or isolation requirements justify a separate host and you have updated the deployment workflow to use it.
- Blazor Server is stateful — scale out requires Azure SignalR Service.

### Worker topology
- The low-cost baseline is UI-only hosting; the UI host runs the active infrastructure services used by the app.
- The dedicated worker app is no longer part of the default deployment workflow.

### Cost-aware deployment guidance
For ongoing cost stewardship, use this operating model unless you have evidence that you need more:

1. Start with the script defaults: `B1` App Service plus low-cap SQL serverless (`0.5` min vCores, `1` max vCore, `60` minute auto-pause).
2. After the first deployment, inspect actual spend in **Cost Management + Billing**.
3. If a dedicated worker app is provisioned, stop it for a day and watch whether anything you care about actually breaks:
    ```powershell
    az webapp stop `
      --resource-group rg-authority-records `
      --name authority-records-worker
    ```
4. If nothing important depends on the worker, either keep it stopped or remove it (and disable any `deploy-worker` job that targets it).
5. If the UI feels memory-constrained, scale the plan to `B2` or `S1` before considering Premium tiers.

The reason to keep a worker around (when one exists) is compatibility with whatever deployment workflow expects it. The reason to stop it is that duplicate background polling can keep Azure SQL serverless online more often than necessary.

### Right-size an existing deployment
If a deployment was provisioned with the older premium defaults (`P1V2` plan, larger SQL serverless caps), use these commands to scale it down without re-provisioning. The worker line is only relevant if a dedicated worker app exists — skip it otherwise.

```powershell
$RG     = "rg-authority-records"
$PLAN   = "asp-authority-records"
$UI     = "authority-records-ui"
$WORKER = "authority-records-worker"
$DB     = "AuthorityRecords"
$SQL    = az sql server list -g $RG --query "[0].name" -o tsv

az appservice plan update -g $RG -n $PLAN --sku B1 --number-of-workers 1

az sql db update -g $RG -s $SQL -n $DB `
  --edition GeneralPurpose --compute-model Serverless --family Gen5 `
  --capacity 1 --min-capacity 0.5 --auto-pause-delay 60

az webapp stop -g $RG -n $WORKER
```

If the UI becomes constrained afterward, scale the plan back up to `B2` or `S1`. If the worker proves unnecessary, disable its deployment job and then delete it.

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

### Custom Domain

Production uses `authorityrecords.dev` (apex, canonical) and
`www.authorityrecords.dev` (301-redirected to apex via
`Modules.Records.UI/Middleware/CanonicalHostRedirectMiddleware.cs`).
This section is the end-to-end runbook for binding a custom domain on Porkbun.
Adapt the values for a different registrar or domain.

#### 1 — Pull the two values you'll need from Azure

```powershell
az webapp show -g rg-authority-records -n authority-records-ui `
  --query "customDomainVerificationId" -o tsv

# The inbound IP isn't always exposed via 'az webapp show' on Linux App Service
# until at least one custom domain is bound. Resolve it from DNS instead:
Resolve-DnsName -Name authority-records-ui.azurewebsites.net -Type A `
  | Where-Object { $_.IPAddress } | Select-Object -First 1 -ExpandProperty IPAddress
```

#### 2 — Add DNS records at the registrar

You need four records. The two `asuid` TXT records are how Azure proves you
control the domain — bindings will fail without them.

| Type    | Host        | Answer                                     |
|---------|-------------|--------------------------------------------|
| `A`     | *(blank)*   | App Service inbound IP from step 1         |
| `TXT`   | `asuid`     | `customDomainVerificationId` from step 1   |
| `CNAME` | `www`       | `authority-records-ui.azurewebsites.net`   |
| `TXT`   | `asuid.www` | `customDomainVerificationId` from step 1   |

> **Porkbun gotcha:** newly registered domains ship with an `ALIAS` at the apex
> and a `CNAME` at `www`, both pointing to `pixie.porkbun.com` (Porkbun's
> parking page). DNS doesn't allow `A` and `CNAME`/`ALIAS` to coexist at the
> same host, so the editor refuses with "record already exists." **Edit those
> two parking records in place** rather than trying to add alongside them.

> **Apex must be `A`, not `ALIAS`, for the managed cert to issue.** Hostname
> binding works with either, but Azure's managed-cert validation does an
> authoritative DNS query that doesn't follow `ALIAS` flattening, and fails
> with *"The A record for {host} must point to {ip}. The current A record
> points to empty."*

Wait 5–15 minutes for propagation, then verify each record with e.g.
`Resolve-DnsName -Name authorityrecords.dev -Type A -Server 1.1.1.1`.

#### 3 — Bind hostnames to the App Service

```powershell
az webapp config hostname add -g rg-authority-records `
  --webapp-name authority-records-ui --hostname authorityrecords.dev

az webapp config hostname add -g rg-authority-records `
  --webapp-name authority-records-ui --hostname www.authorityrecords.dev
```

Both should report `hostNameType: Verified`. `sslState` will be `null` —
that's expected; the cert binding is the next step.

#### 4 — Issue free managed TLS certificates

```powershell
az webapp config ssl create -g rg-authority-records `
  --name authority-records-ui --hostname authorityrecords.dev

az webapp config ssl create -g rg-authority-records `
  --name authority-records-ui --hostname www.authorityrecords.dev
```

Both calls return immediately with *"Managed Certificate creation in
progress"* — provisioning is asynchronous and takes 2–5 minutes per cert.
**Don't trust `az webapp config ssl list` to confirm completion** — that
command currently filters out managed certs in some CLI versions and returns
`[]` even when the certs exist. Check the underlying ARM resource instead:

```powershell
az resource list -g rg-authority-records `
  --resource-type Microsoft.Web/certificates -o table
```

Wait until both certs show `provisioningState: Succeeded`.

#### 5 — Bind certs and force HTTPS

```powershell
$apexThumb = az resource show -g rg-authority-records `
  --resource-type Microsoft.Web/certificates --name authorityrecords.dev `
  --query "properties.thumbprint" -o tsv

$wwwThumb = az resource show -g rg-authority-records `
  --resource-type Microsoft.Web/certificates --name www.authorityrecords.dev `
  --query "properties.thumbprint" -o tsv

az webapp config ssl bind -g rg-authority-records --name authority-records-ui `
  --certificate-thumbprint $apexThumb --ssl-type SNI --hostname authorityrecords.dev

az webapp config ssl bind -g rg-authority-records --name authority-records-ui `
  --certificate-thumbprint $wwwThumb  --ssl-type SNI --hostname www.authorityrecords.dev

# .dev is on the HSTS preload list — browsers refuse plain HTTP. Required.
az webapp update -g rg-authority-records -n authority-records-ui --https-only true
```

#### 6 — Verify end-to-end

```powershell
Invoke-WebRequest -Uri "https://authorityrecords.dev"     -MaximumRedirection 0 -SkipHttpErrorCheck
Invoke-WebRequest -Uri "https://www.authorityrecords.dev" -MaximumRedirection 0 -SkipHttpErrorCheck
Invoke-WebRequest -Uri "http://authorityrecords.dev"      -MaximumRedirection 0 -SkipHttpErrorCheck
```

Expect: HTTPS endpoints serve from the app (likely a 302 to the login page),
and the HTTP request returns a 301 to the HTTPS apex.

#### Why apex is canonical, not www

`www.authorityrecords.dev` 301s to `authorityrecords.dev` via
`CanonicalHostRedirectMiddleware`, registered in `Program.cs` only for the
non-Development environment. Apex-as-canonical matches modern convention
(github.com, openai.com, anthropic.com). To switch directions, edit the two
constants at the top of that middleware and redeploy.

#### Cert renewal

App Service Managed Certificates auto-renew ~45 days before expiration. No
action required unless DNS changes — if the apex `A` record or `www` `CNAME`
is removed or repointed, renewal will fail silently. Set a calendar reminder
to verify both hostnames are still serving valid certs once a year.
