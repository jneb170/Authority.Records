<#
.SYNOPSIS
    One-time provisioning of the AWS Lightsail instance that hosts Authority.Records.

.DESCRIPTION
    Creates (or reuses, so it is safe to rerun):
      - an SSH key pair for GitHub Actions deploys (generated locally, public key imported)
      - an Ubuntu 24.04 Lightsail instance, bootstrapped with Docker by cloud-init.sh
      - a static IPv4 address attached to the instance
      - firewall rules: 22 (SSH), 80 and 443 (Caddy / Let's Encrypt)
      - daily automatic snapshots (backups of the SQLite databases and uploads)

    Requires the AWS CLI v2 signed in to the target account (aws configure / aws sso login)
    and OpenSSH's ssh-keygen (built into Windows 10+, macOS and Linux).

.EXAMPLE
    .\deploy\aws\provision-lightsail.ps1

.EXAMPLE
    .\deploy\aws\provision-lightsail.ps1 -Region us-east-2 -BundleId medium_3_0
#>
[CmdletBinding()]
param(
    [string] $Region           = "us-east-1",
    [string] $AvailabilityZone = "",            # defaults to "<Region>a"
    [string] $InstanceName     = "authority-records",
    [string] $StaticIpName     = "authority-records-ip",
    [string] $KeyPairName      = "authority-records-deploy",
    [string] $KeyPath          = (Join-Path $HOME ".ssh/authority-records-deploy"),
    [string] $BlueprintId      = "ubuntu_24_04",
    # 2 GB RAM. List bundles with:
    #   aws lightsail get-bundles --query "bundles[?isActive].[bundleId,price,ramSizeInGb]" --output table
    [string] $BundleId         = "small_3_0",
    [string] $SnapshotTimeUtc  = "06:00"
)

$ErrorActionPreference = "Stop"
if ($PSVersionTable.PSVersion.Major -ge 7) { $PSNativeCommandUseErrorActionPreference = $false }

if (-not $AvailabilityZone) { $AvailabilityZone = "${Region}a" }
$cloudInit = Join-Path $PSScriptRoot "cloud-init.sh"

function Invoke-Aws {
    # Runs an AWS CLI command in the target region and throws on failure.
    # (Local 'Continue' so Windows PowerShell 5.1 doesn't throw on native stderr.)
    $ErrorActionPreference = "Continue"
    $output = & aws @args --region $Region 2>&1
    if ($LASTEXITCODE -ne 0) { throw "aws $($args -join ' ') failed:`n$output" }
    return $output
}

function Test-Aws {
    # Returns $true if the AWS CLI command succeeds (used for existence checks).
    $ErrorActionPreference = "Continue"
    & aws @args --region $Region *> $null
    return ($LASTEXITCODE -eq 0)
}

Write-Host "==> Checking AWS CLI identity" -ForegroundColor Cyan
Invoke-Aws sts get-caller-identity --query Arn --output text | Write-Host

# ── Validate blueprint and bundle (AWS occasionally renames these) ───────────
$blueprints = Invoke-Aws lightsail get-blueprints --query "blueprints[?isActive].blueprintId" --output text
if (($blueprints -split '\s+') -notcontains $BlueprintId) {
    throw "Blueprint '$BlueprintId' is not available in $Region. Active blueprints: $blueprints"
}
$bundles = Invoke-Aws lightsail get-bundles --query "bundles[?isActive].bundleId" --output text
if (($bundles -split '\s+') -notcontains $BundleId) {
    throw "Bundle '$BundleId' is not available in $Region. Active bundles: $bundles"
}

# ── SSH key pair ─────────────────────────────────────────────────────────────
Write-Host "==> SSH key pair '$KeyPairName'" -ForegroundColor Cyan
if (-not (Test-Path $KeyPath)) {
    New-Item -ItemType Directory -Force -Path (Split-Path $KeyPath) | Out-Null
    # Empty passphrase (CI uses the key unattended). Windows PowerShell 5.1 and
    # PowerShell < 7.3 drop a bare "" argument, so it must be passed as '""' there.
    $legacyArgPassing = $PSVersionTable.PSVersion -lt [version]"7.3"
    $emptyPassphrase = if ($legacyArgPassing) { '""' } else { "" }
    & ssh-keygen -t rsa -b 4096 -N $emptyPassphrase -C "authority-records-deploy" -f $KeyPath | Out-Null
    if (-not (Test-Path $KeyPath)) { throw "ssh-keygen failed to create $KeyPath" }
    Write-Host "    Generated $KeyPath"
}
if (-not (Test-Aws lightsail get-key-pair --key-pair-name $KeyPairName)) {
    $publicKey = (Get-Content "$KeyPath.pub" -Raw).Trim()
    Invoke-Aws lightsail import-key-pair --key-pair-name $KeyPairName --public-key-base64 $publicKey | Out-Null
    Write-Host "    Imported public key into Lightsail"
} else {
    Write-Host "    Already exists in Lightsail"
}

# ── Instance ─────────────────────────────────────────────────────────────────
Write-Host "==> Instance '$InstanceName' ($BundleId, $BlueprintId, $AvailabilityZone)" -ForegroundColor Cyan
if (-not (Test-Aws lightsail get-instance --instance-name $InstanceName)) {
    Invoke-Aws lightsail create-instances `
        --instance-names $InstanceName `
        --availability-zone $AvailabilityZone `
        --blueprint-id $BlueprintId `
        --bundle-id $BundleId `
        --key-pair-name $KeyPairName `
        --user-data "file://$cloudInit" | Out-Null
    Write-Host "    Created"
} else {
    Write-Host "    Already exists"
}

Write-Host "    Waiting for the instance to reach 'running'..."
for ($i = 0; $i -lt 60; $i++) {
    $state = Invoke-Aws lightsail get-instance-state --instance-name $InstanceName --query state.name --output text
    if ($state -eq "running") { break }
    Start-Sleep -Seconds 5
}
if ($state -ne "running") { throw "Instance did not reach 'running' (last state: $state)" }

# ── Static IP ────────────────────────────────────────────────────────────────
Write-Host "==> Static IP '$StaticIpName'" -ForegroundColor Cyan
if (-not (Test-Aws lightsail get-static-ip --static-ip-name $StaticIpName)) {
    Invoke-Aws lightsail allocate-static-ip --static-ip-name $StaticIpName | Out-Null
}
$attachedTo = Invoke-Aws lightsail get-static-ip --static-ip-name $StaticIpName --query staticIp.attachedTo --output text
if ($attachedTo -ne $InstanceName) {
    Invoke-Aws lightsail attach-static-ip --static-ip-name $StaticIpName --instance-name $InstanceName | Out-Null
}
$ip = Invoke-Aws lightsail get-static-ip --static-ip-name $StaticIpName --query staticIp.ipAddress --output text

# ── Firewall ─────────────────────────────────────────────────────────────────
Write-Host "==> Firewall: 22/tcp, 80/tcp, 443/tcp, 443/udp" -ForegroundColor Cyan
Invoke-Aws lightsail put-instance-public-ports --instance-name $InstanceName --port-infos `
    "fromPort=22,toPort=22,protocol=tcp" `
    "fromPort=80,toPort=80,protocol=tcp" `
    "fromPort=443,toPort=443,protocol=tcp" `
    "fromPort=443,toPort=443,protocol=udp" | Out-Null

# ── Automatic snapshots (backups) ────────────────────────────────────────────
Write-Host "==> Daily automatic snapshots at $SnapshotTimeUtc UTC" -ForegroundColor Cyan
Invoke-Aws lightsail enable-add-on --resource-name $InstanceName `
    --add-on-request "addOnType=AutoSnapshot,autoSnapshotAddOnRequest={snapshotTimeOfDay=$SnapshotTimeUtc}" | Out-Null

# ── Summary ──────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "Provisioning complete." -ForegroundColor Green
Write-Host "  Static IP:     $ip"
Write-Host "  SSH:           ssh -i `"$KeyPath`" ubuntu@$ip"
Write-Host "  Docker setup:  takes 2-3 minutes after first boot; check with"
Write-Host "                 ssh -i `"$KeyPath`" ubuntu@$ip 'cloud-init status --wait && docker --version'"
Write-Host ""
Write-Host "Next steps (see Documents/aws-deployment-guide.md):"
Write-Host "  1. DNS: point A records for the apex and www at $ip"
Write-Host "  2. GitHub secrets (production environment):"
Write-Host "       LIGHTSAIL_HOST         = $ip"
Write-Host "       LIGHTSAIL_SSH_KEY      = contents of $KeyPath"
Write-Host "       LIGHTSAIL_KNOWN_HOSTS  = server host keys (command in the guide, Step 3)"
Write-Host "  3. GitHub variables (production environment):"
Write-Host "       SITE_ADDRESS = authorityrecords.dev, www.authorityrecords.dev"
Write-Host "       ACME_EMAIL   = an email for Let's Encrypt expiry notices"
Write-Host "  4. Push to master (or run the workflow manually) to deploy."
