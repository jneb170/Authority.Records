#!/usr/bin/env bash
# First-boot setup for the Authority.Records Lightsail instance (Ubuntu 24.04).
# Passed as --user-data by provision-lightsail.ps1; runs once, as root.
# Progress: /var/log/cloud-init-output.log on the instance.
set -euxo pipefail

# 2 GB swap: headroom for .NET + SQLite on small instance bundles.
if [ ! -f /swapfile ]; then
  fallocate -l 2G /swapfile
  chmod 600 /swapfile
  mkswap /swapfile
  swapon /swapfile
  echo '/swapfile none swap sw 0 0' >> /etc/fstab
fi

# Docker Engine + Compose plugin from Docker's official apt repository.
apt-get update
apt-get install -y ca-certificates curl
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
chmod a+r /etc/apt/keyrings/docker.asc
. /etc/os-release
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu ${VERSION_CODENAME} stable" \
  > /etc/apt/sources.list.d/docker.list
apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
systemctl enable --now docker

# Let the deploy user run docker without sudo.
usermod -aG docker ubuntu

# Deployment directory: the GitHub Actions workflow copies the compose file and
# Caddyfile here and writes .env on every deploy.
install -d -o ubuntu -g ubuntu -m 0755 /opt/authority-records

# Security updates are applied automatically (unattended-upgrades ships enabled
# on Ubuntu); make sure it's installed.
apt-get install -y unattended-upgrades
