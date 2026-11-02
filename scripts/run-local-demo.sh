#!/usr/bin/env bash
# Run the Blazor UI host locally the way production runs it: published output,
# ASPNETCORE_ENVIRONMENT=Production, SQLite, demo account + sample dataset seeded.
# Mirrors the ENV block in the Dockerfile. Intended for manual QA and for agents
# driving the app with Playwright (see scripts/demo-browser/).
#
# Usage:
#   scripts/run-local-demo.sh            # publish (if needed) and start in the background
#   scripts/run-local-demo.sh --fresh    # also wipe the SQLite databases first (re-seed)
#   scripts/run-local-demo.sh --stop     # stop a running instance
#
# Environment overrides:
#   DEMO_WORK_DIR       working dir for publish output, data, logs (default /tmp/authority-demo)
#   DEMO_PORT           HTTP port (default 8080)
#   DEMO_ADMIN_PASSWORD password for admin@demo.authorityrecords.dev (default Local-Admin-1!)
#   DEMO_CREATE_LIMIT   Demo__RateLimit__MaxCreatesPerWindow (default: production default, 30/hour,
#                       shared by every demo visitor; raise it for heavy create testing)
#   DEMO_LOG_LEVEL      Logging__LogLevel__Default (default Warning, as in production)
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WORK="${DEMO_WORK_DIR:-/tmp/authority-demo}"
PORT="${DEMO_PORT:-8080}"
BASE="http://127.0.0.1:${PORT}"

FRESH=0
STOP=0
for arg in "$@"; do
  case "$arg" in
    --fresh) FRESH=1 ;;
    --stop) STOP=1 ;;
    -h|--help) sed -n '2,22p' "$0"; exit 0 ;;
    *) echo "Unknown argument: $arg" >&2; exit 2 ;;
  esac
done

stop_app() {
  # Anchor the pattern to the start of the command line. An unanchored
  # `pgrep -f "dotnet Modules.Records.UI.dll"` (or `pkill -f Modules.Records.UI`) also
  # matches any shell whose command text mentions it, e.g. the caller's `bash -c ...`,
  # and kills that shell.
  local pids
  pids="$(pgrep -f '^dotnet Modules\.Records\.UI\.dll' || true)"
  if [[ -n "$pids" ]]; then
    echo "Stopping running instance (pid $pids)"
    kill $pids || true
    for _ in $(seq 1 20); do pgrep -f '^dotnet Modules\.Records\.UI\.dll' >/dev/null || break; sleep 0.5; done
  fi
}

if [[ "$STOP" == 1 ]]; then stop_app; exit 0; fi

# 1. .NET 10 SDK. In Claude Code cloud sessions the dot.net install script host
#    (builds.dotnet.microsoft.com) is blocked, but packages.microsoft.com is reachable.
if ! command -v dotnet >/dev/null 2>&1 || ! dotnet --list-sdks 2>/dev/null | grep -q '^10\.'; then
  echo "Installing .NET 10 SDK from packages.microsoft.com (apt)…"
  . /etc/os-release
  curl -sSL "https://packages.microsoft.com/config/ubuntu/${VERSION_ID}/packages-microsoft-prod.deb" -o /tmp/packages-microsoft-prod.deb
  dpkg -i /tmp/packages-microsoft-prod.deb >/dev/null
  apt-get update -qq
  DEBIAN_FRONTEND=noninteractive apt-get install -y -qq dotnet-sdk-10.0 >/dev/null
fi
dotnet --list-sdks | grep '^10\.' | tail -1

# 2. Publish. Run the *published* output: running bin/Release with
#    ASPNETCORE_ENVIRONMENT=Production makes /_framework/blazor.server.js return 500
#    (the static web assets manifest points at the source tree), so Blazor never starts.
mkdir -p "$WORK"
echo "Publishing Modules.Records.UI to $WORK/pub…"
dotnet publish "$ROOT/Modules.Records.UI" -c Release -o "$WORK/pub" -nologo -v quiet >/dev/null

# 3. Data directories (equivalent of the Docker volumes).
if [[ "$FRESH" == 1 && -d "$WORK/data" ]]; then
  stop_app
  echo "Wiping $WORK/data"
  rm -rf -- "${WORK:?}/data"
fi
mkdir -p "$WORK/data/db" "$WORK/data/keys" "$WORK/data/uploads/mugshots"

# 4. Cache the CDN stylesheets the pages load (Bootstrap, Bootstrap Icons) from the npm
#    registry. cdn.jsdelivr.net is blocked in cloud sessions; scripts/demo-browser/lib.js
#    serves these files instead so screenshots render with real styling. Best effort.
mkdir -p "$WORK/cdn"
grep -rhoE 'cdn\.jsdelivr\.net/npm/[a-z-]+@[0-9.]+' "$ROOT/Modules.Records.UI" --include=*.razor --include=*.cshtml \
  | sort -u | while read -r ref; do
    pkg="${ref#cdn.jsdelivr.net/npm/}"; name="${pkg%@*}"; ver="${pkg#*@}"
    dest="$WORK/cdn/${name}-${ver}"
    if [[ ! -d "$dest/package" ]]; then
      mkdir -p "$dest"
      curl -fsSL "https://registry.npmjs.org/${name}/-/${name}-${ver}.tgz" | tar xz -C "$dest" \
        || echo "warning: could not cache ${name}@${ver} (pages will render unstyled in headless runs)"
    fi
  done

# 5. Start in the background with production-equivalent configuration.
stop_app
echo "Starting on $BASE (log: $WORK/app.log)"
(
  cd "$WORK/pub"
  export ASPNETCORE_ENVIRONMENT=Production
  export ASPNETCORE_URLS="$BASE"
  export DefaultDatabaseProvider=Sqlite
  export ConnectionStrings__SqliteAppConnection="Data Source=$WORK/data/db/app.db;Cache=Shared;Foreign Keys=True"
  export ConnectionStrings__SqliteAuthConnection="Data Source=$WORK/data/db/auth.db;Cache=Shared;Foreign Keys=True"
  export DataProtection__KeysPath="$WORK/data/keys"
  export Mugshots__StoragePath="$WORK/data/uploads/mugshots"
  export Demo__AdminPassword="${DEMO_ADMIN_PASSWORD:-Local-Admin-1!}"
  export Logging__LogLevel__Default="${DEMO_LOG_LEVEL:-Warning}"
  if [[ -n "${DEMO_CREATE_LIMIT:-}" ]]; then export Demo__RateLimit__MaxCreatesPerWindow="$DEMO_CREATE_LIMIT"; fi
  nohup dotnet Modules.Records.UI.dll >"$WORK/app.log" 2>&1 &
  echo $! >"$WORK/app.pid"
)

# 6. Wait for readiness. Poll HTTP rather than grepping the log: at the production log
#    level (Warning) the "Demo account ready" Information line is never written.
for _ in $(seq 1 90); do
  if curl -fs -o /dev/null "$BASE/Account/Login"; then
    echo "Ready: $BASE  (click 'Explore the live demo', or sign in as admin@demo.authorityrecords.dev)"
    exit 0
  fi
  if ! kill -0 "$(cat "$WORK/app.pid")" 2>/dev/null; then
    echo "App exited during startup:" >&2; tail -40 "$WORK/app.log" >&2; exit 1
  fi
  sleep 2
done
echo "Timed out waiting for $BASE" >&2; tail -40 "$WORK/app.log" >&2; exit 1
