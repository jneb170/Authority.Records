# Authority.Records — AWS Deployment Walkthrough

Production runs on a single **AWS Lightsail** instance (Ubuntu 24.04) with Docker:

```
GitHub push (master)
  └── GitHub Actions (.github/workflows/deploy.yml)
        ├── Build & Test
        ├── Publish Image ──────────► ghcr.io/<owner>/<repo>:<sha>
        └── Deploy (SSH) ───────────► Lightsail instance  /opt/authority-records
                                        ├── caddy  :80/:443  Let's Encrypt TLS, HTTP→HTTPS, WebSockets
                                        └── app    :8080     Blazor Server UI (Modules.Records.UI)
                                              ├── volume app-db       → /data/db       (app.db, auth.db — SQLite)
                                              ├── volume app-keys     → /data/keys     (Data Protection keys)
                                              └── volume app-uploads  → /data/uploads  (mugshots; served only via /mugshots/{id})
```

| File | Purpose |
|------|---------|
| `Dockerfile` | Multi-stage build of the UI host image |
| `deploy/aws/provision-lightsail.ps1` | One-time script: key pair, instance, static IP, firewall, snapshots |
| `deploy/aws/cloud-init.sh` | First-boot setup on the instance (Docker, swap, deploy dir) |
| `deploy/aws/docker-compose.yml` | Production stack (app + Caddy) |
| `deploy/aws/Caddyfile` | Reverse proxy / TLS config |
| `deploy/aws/app.env.example` | Optional app settings (replaces Azure "Application settings") |
| `.github/workflows/deploy.yml` | Build → push image → deploy over SSH |

**Why this shape:** it matches the old Azure footprint (one B1 App Service on the SQLite
provider) at a similar cost (~$12/month for the 2 GB bundle), and a single instance means
Blazor Server circuits need no sticky-session or SignalR backplane setup. Data lives on the
instance's disk, which survives redeploys and reboots, and is backed up by daily
Lightsail snapshots.

---

## Prerequisites

1. **AWS account** with permission to use Lightsail.
2. **AWS CLI v2** — https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html
   — then sign in: `aws configure` (access key) or `aws configure sso`.
3. **OpenSSH client** (`ssh`, `ssh-keygen`) — built into Windows 10+, macOS, Linux.
4. **PowerShell** — Windows PowerShell 5.1 or PowerShell 7+.

---

## Step 1 — Provision the instance (one time)

```powershell
cd C:\path\to\Authority.Records
.\deploy\aws\provision-lightsail.ps1
```

Defaults: region `us-east-1`, bundle `small_3_0` (2 GB RAM), daily snapshots at 06:00 UTC.
Override as needed, e.g. `-Region us-east-2 -BundleId medium_3_0`. List the bundles
available to your account with:

```powershell
aws lightsail get-bundles --query "bundles[?isActive].[bundleId,price,ramSizeInGb]" --output table
```

The script is safe to rerun; it reuses anything that already exists. It:

- generates an SSH key at `~/.ssh/authority-records-deploy` (no passphrase, used by CI) and
  imports the public half into Lightsail
- creates the instance with `cloud-init.sh` as user data (installs Docker + Compose,
  adds 2 GB swap, creates `/opt/authority-records`)
- allocates and attaches a **static IP**
- opens ports 22, 80, 443 (TCP) and 443 (UDP, HTTP/3)
- enables **automatic daily snapshots**

It prints the static IP and the values for the next steps. Docker installation finishes
2–3 minutes after first boot; confirm with:

```powershell
ssh -i ~/.ssh/authority-records-deploy ubuntu@<STATIC_IP> "cloud-init status --wait && docker compose version"
```

**If it reports `status: error`**, see what failed:

```powershell
ssh -i ~/.ssh/authority-records-deploy ubuntu@<STATIC_IP> "cloud-init status --long; sudo tail -n 40 /var/log/cloud-init-output.log"
```

then rerun the setup by hand (it's safe to run more than once; the `-replace` strips any
Windows line endings):

```powershell
((Get-Content deploy\aws\cloud-init.sh -Raw) -replace "`r","") | ssh -i ~/.ssh/authority-records-deploy ubuntu@<STATIC_IP> "sudo sh -s"
```

> Lightsail prepends its own `#!/bin/sh` lines to the user data, so `cloud-init.sh` runs
> under `sh` (dash), not bash. Keep it POSIX: no `pipefail`, `[[ ]]` or arrays.

---

## Step 2 — DNS

At the registrar (Porkbun for `authorityrecords.dev`), replace the old Azure records:

| Type | Host | Answer |
|------|------|--------|
| `A` | *(blank — apex)* | Lightsail static IP |
| `A` | `www` | Lightsail static IP |

Delete the old `asuid` / `asuid.www` TXT records and the `www` CNAME to
`*.azurewebsites.net`; they are Azure-specific. An apex `ALIAS` works too, but a plain `A`
record is simplest.

> **Porkbun gotcha:** new domains ship with parking records (`ALIAS` at the apex, `CNAME`
> at `www` → `pixie.porkbun.com`). Edit those in place rather than adding alongside them.

Caddy requests Let's Encrypt certificates automatically the first time it starts with
DNS pointing at the instance. Nothing else to configure.

---

## Step 3 — GitHub configuration

In **Settings → Environments → production** (create the environment if needed):

**Secrets**

| Name | Value |
|------|-------|
| `LIGHTSAIL_HOST` | The static IP |
| `LIGHTSAIL_SSH_KEY` | Full contents of `~/.ssh/authority-records-deploy` (the private key) |
| `LIGHTSAIL_KNOWN_HOSTS` | The server's SSH host keys (pins the host key; see below) |

Generate `LIGHTSAIL_KNOWN_HOSTS` by reading the host keys over your (already verified) SSH
connection. Copying commands from a browser or chat overwrites the clipboard, so this saves
to a file instead:

```powershell
$ip = "<STATIC_IP>"   # exactly the value of LIGHTSAIL_HOST
ssh -i ~/.ssh/authority-records-deploy ubuntu@$ip "cat /etc/ssh/ssh_host_ed25519_key.pub /etc/ssh/ssh_host_rsa_key.pub" |
  ForEach-Object { "$ip " + (($_ -split ' ')[0..1] -join ' ') } |
  Tee-Object -FilePath "$HOME\lightsail-known-hosts.txt"
notepad "$HOME\lightsail-known-hosts.txt"   # Ctrl+A, Ctrl+C, paste into the secret
```

It should print two lines, `<STATIC_IP> ssh-ed25519 AAAA…` and `<STATIC_IP> ssh-rsa AAAA…`.

> Avoid `ssh-keyscan` on Windows: the built-in OpenSSH's `ssh-keyscan` fails against
> Ubuntu 24.04 with `choose_kex: unsupported KEX method sntrup761x25519-sha512@openssh.com`.
> (On macOS/Linux, `ssh-keyscan -t ed25519,rsa <STATIC_IP>` works too.)

**Variables**

| Name | Value |
|------|-------|
| `SITE_ADDRESS` | `authorityrecords.dev, www.authorityrecords.dev` |
| `ACME_EMAIL` | Email for Let's Encrypt expiry notices |

Then remove the obsolete Azure entries: secrets `AZURE_CREDENTIALS`,
`AZURE_SQL_CONNECTION_STRING`, `AZURE_WEBAPP_UI_NAME`, and variable
`DEFAULT_DATABASE_PROVIDER`.

The image is pushed to GitHub Container Registry using the workflow's built-in
`GITHUB_TOKEN`; no registry credentials need to be configured. The instance pulls with
that same short-lived token during each deploy.

---

## Step 4 — Deploy

Push to `master` (or run **Actions → Build & Deploy (AWS Lightsail) → Run workflow**).

1. ✅ Build & Test
2. ✅ Publish Image (`ghcr.io/<owner>/<repo>:<commit sha>`)
3. ✅ Deploy to Lightsail: copies the compose stack, writes `.env`, pulls, restarts, and
   waits for the app to answer before reporting success (prints app logs if it doesn't)

On first boot against the empty volume the app applies all SQLite migrations, seeds the
demo account, and is ready at https://authorityrecords.dev.

> **First smoke test before DNS is ready:** set `SITE_ADDRESS` to `:80` and browse to
> `http://<STATIC_IP>`. Switch it back to the real hostnames once DNS resolves and redeploy
> to get HTTPS.

### First Super user

Development seeding does not run in production, so a fresh database has no Super user.
Bootstrap one once:

```bash
cd /opt/authority-records
cat >> app.env <<'EOF'
Bootstrap__SuperUserEmail='you@example.com'
Bootstrap__SuperUserPassword='<password>'
EOF
chmod 600 app.env
docker compose up -d
docker compose logs app | grep -i bootstrap
```

The password must be at least 8 characters with an uppercase letter, a lowercase letter, a
digit and a symbol (ASP.NET Identity's rules). Keep the single quotes: they make Compose
take the value literally, so a `$` in the password isn't treated as a variable. The
password itself can't contain a `'`. If the log shows `Super user bootstrap failed: …`, it
names the rule that wasn't met; fix the value with `nano app.env` and run
`docker compose up -d` again.

On startup the app creates the standard roles (Super, Admin, Supervisor, Officer,
Dispatcher) and the Super user. It logs `Bootstrapped Super user …`. Sign in, then
**delete those two lines** (`sed -i '/^Bootstrap__/d' app.env`) and run
`docker compose up -d` again. The step is
a no-op once any Super user exists, so it never resets a password.

---

## Operations

All commands run on the instance (`ssh -i ~/.ssh/authority-records-deploy ubuntu@<STATIC_IP>`),
in `/opt/authority-records`.

| Task | Command |
|------|---------|
| Status | `docker compose ps` |
| App logs (live) | `docker compose logs -f app` |
| Proxy / TLS logs | `docker compose logs -f caddy` |
| Restart | `docker compose restart app` |
| Apply `app.env` changes | `docker compose up -d` |
| Roll back | Re-run an earlier successful workflow run (**Actions → run → Re-run all jobs**), or edit `APP_IMAGE` in `.env` to an older `:<sha>` tag and `docker compose up -d` |

### App settings & secrets

Anything previously set as an Azure App Service *Application setting* goes in
`/opt/authority-records/app.env` (start from `deploy/aws/app.env.example`). Use `__` for
nested keys, e.g. `Demo__RateLimit__WindowMinutes=60`. Deploys never overwrite this file.

The database provider and paths are fixed in the image (`DefaultDatabaseProvider=Sqlite`,
`/data/db/app.db`, `/data/db/auth.db`, `DataProtection__KeysPath=/data/keys`).

### Public demo

On first start the app seeds the **Demo Police Department**: a demo admin
(`admin@demo.authorityrecords.dev`), the public demo account that the sign-in page's
**Explore the live demo** button uses (Supervisor role), and a curated sample dataset.
The dataset is seeded once and skipped once any Name exists in the demo jurisdiction.

- **Demo admin password:** set `Demo__AdminPassword` in `app.env`. It is re-applied on
  every start. Without it, the account has a random password and can't be used.
- **About panel links:** `About__AuthorName`, `About__LinkedInUrl`, `About__WebsiteUrl`
  and `About__GitHubUrl`. Set `About__Enabled=false` to hide the panel.
- **Turn the demo off:** `Demo__Enabled=false`.
- **Re-seed after visitors have changed the data:** stop the app, delete the demo
  agency's records, and restart. The simplest way is to restore an instance snapshot.

### Backups

- **Automatic:** Lightsail takes a daily instance snapshot (last 7 kept). Restore by
  creating a new instance from a snapshot in the Lightsail console, then move the static
  IP to it.
- **Manual snapshot** before risky changes:
  `aws lightsail create-instance-snapshot --instance-name authority-records --instance-snapshot-name pre-change-$(Get-Date -Format yyyyMMdd)`
- **Copy the databases off the box:**
  ```bash
  cd /opt/authority-records
  docker compose stop app
  docker run --rm -v authority-records_app-db:/db -v "$PWD":/out alpine \
    tar czf /out/db-backup.tgz -C /db .
  docker compose start app
  ```
  then `scp` `db-backup.tgz` down. (Stopping the app guarantees the SQLite WAL is
  checkpointed; snapshots of the running instance are crash-consistent, which SQLite
  recovers from automatically.)

### Scaling

- Vertical: create a larger bundle from a snapshot (`medium_3_0` = 4 GB) and move the static IP.
- Horizontal scale-out is **not** supported by this topology: SQLite and local mugshot
  storage are single-node, and Blazor Server circuits are stateful. Going multi-instance
  would mean moving to RDS SQL Server (the SQL Server provider is still supported via
  `DefaultDatabaseProvider=SqlServer` + `ConnectionStrings__DefaultConnection`), S3 for
  mugshots, a shared Data Protection key store, and an ALB with sticky sessions.

### Security notes

- SSH is key-only (Lightsail Ubuntu default). Port 22 is open to the internet because
  GitHub-hosted runners have no fixed IP range; restrict it in the Lightsail console
  (**Networking → IPv4 Firewall**) if you deploy from a self-hosted runner instead.
- Ubuntu security updates install automatically (`unattended-upgrades`). Reboot
  occasionally for kernel updates: the containers restart on their own
  (`restart: unless-stopped`).
- The app container runs as a non-root user. Data Protection keys are stored unencrypted
  on the `app-keys` volume (same as on Azure App Service); access to the instance is
  equivalent to access to those keys.
