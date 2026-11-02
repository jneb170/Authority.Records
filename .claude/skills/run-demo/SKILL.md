---
name: run-demo
description: Start a local, production-equivalent instance of the Authority Records demo (Blazor UI host, Production env, SQLite, seeded demo agency) and drive it with Playwright. Use when asked to run, demo, smoke-test, screenshot, QA or "click through" the app, to reproduce a UI bug, or to verify a UI change in the real app. Includes cloud-session gotchas (blocked hosts, SDK install, CDN, headless browser) learned the hard way.
---

# Run and drive the local demo

The live demo (https://authorityrecords.dev) and the production image on GHCR are **not
reachable from Claude Code cloud sessions**: the site is blocked by the network policy and
the GHCR package is private. Run a local, production-equivalent copy instead. It seeds the same demo
agency, users and dataset as production.

## Quick start

```bash
scripts/run-local-demo.sh --fresh          # install SDK if missing, publish, start on :8080, wait until ready
node scripts/demo-browser/smoke.js         # crawl key pages at desktop + phone width, report errors, save screenshots
scripts/run-local-demo.sh --stop           # stop it
```

* Work dir: `/tmp/authority-demo` (`DEMO_WORK_DIR`): `pub/`, `data/` (SQLite DBs, keys, mugshots), `app.log`, `shots/`, `cdn/`.
* `--fresh` wipes the databases, so the demo dataset is re-seeded on start. Without it, data persists between runs.
* First run takes ~2-3 min (apt SDK install + restore); later runs take ~20 s.
* Screenshots: open them with the Read tool (`/tmp/authority-demo/shots/*.png`).

For custom flows, write a small script using the helper:

```js
const { start } = require('/home/user/Authority.Records/scripts/demo-browser/lib');
(async () => {
  const t = await start();                       // signs in via "Explore the live demo"
  await t.go('/incidents/create');
  await t.page.click('button:has-text("Generate")');
  await t.page.click('button[type=submit]');
  await t.page.waitForTimeout(3000);
  await t.shot('incident-created');
  console.log(t.page.url(), t.events);           // events = console errors, page errors, HTTP >= 400
  await t.browser.close();
})();
```

`start({ login: { email, password } })` signs in with credentials; `start({ login: null })` stays signed out.
Use two `start()` calls for two independent visitors (separate browsers and cookies).

## Accounts and seeded data

| Who | How | Role |
|---|---|---|
| Demo visitor | "Explore the live demo" button (posts `?handler=Demo`; no password needed) | Supervisor, Demo Police Department |
| `demo@authorityrecords.dev` / `Demo@1234` | Sign-in form (password is a public constant in `Demo/DemoUserDefaults.cs`) | same account |
| `admin@demo.authorityrecords.dev` | Password = `DEMO_ADMIN_PASSWORD` (default `Local-Admin-1!`) | Admin, so you can see Charges, Logs, Users, Agencies, Hotkeys, Location Settings |

Seeded record numbers on a fresh DB: incidents `10000-10009` (incident #s like `26-004102`),
arrests `10000-10003`, citations `10000-10005`, names `10000-10013`, locations `20000-20013`.
Routes: `/incidents/{n}`, `/arrests/{n}`, `/citations/{n}`, `/names/{n}`, `/locations/{n}`, plus
`/relationships` on each; `/citations/{n}/texas-pdf`, `/citations/{n}/texas-print`, `/names/{n}/print`.

**Each "Explore the live demo" click creates a separate visitor account** ("Demo Visitor NNNN", Supervisor,
claim `ar:demo-visitor`; see `Demo/DemoVisitorAccounts.cs`), so two `start()` calls are two different users
and locking works between them. The create limit (30 per hour) is per visitor; all visitors together are
capped at 600 per hour. Test Data → "Generate 50 Name Records" stops at 30 with "Stopped after 30 of 50
records: demo limit reached". Signing in with the `demo@…` password form still uses the shared account.
For create-heavy testing start with `DEMO_CREATE_LIMIT=1000 scripts/run-local-demo.sh --fresh`.

## Lessons learned (cloud sessions)

1. **No `dotnet` preinstalled, and the dot.net install script fails** (`builds.dotnet.microsoft.com` is blocked).
   `packages.microsoft.com` works: the script adds the Microsoft apt repo and installs `dotnet-sdk-10.0`.
   SDK 10.0.1xx satisfies `global.json` (`rollForward: latestFeature`). NuGet (`api.nuget.org`) is reachable.
2. **Run the published output, not `bin/Release`.** With `ASPNETCORE_ENVIRONMENT=Production`, running the
   build output makes `/_framework/blazor.server.js` return 500 (`FileNotFoundException` under
   `Modules.Records.UI/wwwroot/_framework`), so no page becomes interactive. `dotnet publish` fixes it.
3. **Don't wait on log lines.** Production logging is `Warning`, so the "Demo account ready" line never
   appears. Poll `GET /Account/Login` for 200 (the script does this).
4. **cdn.jsdelivr.net is blocked**, so Bootstrap and Bootstrap Icons don't load and pages render unstyled.
   That makes visual review meaningless and adds console errors that aren't real bugs.
   The script caches the exact versions from `registry.npmjs.org` (reachable), and `lib.js` serves them by
   intercepting the CDN requests. If you see `ERR_TUNNEL_CONNECTION_FAILED` for jsdelivr, the cache is missing.
5. **Playwright:** Chromium is preinstalled and `require('playwright')` works anywhere. Don't run
   `playwright install`. Launch with `proxy: { server: process.env.HTTPS_PROXY, bypass: '127.0.0.1,localhost' }`.
6. **Blazor Server keeps a websocket open.** Don't use `waitUntil: 'networkidle'`; use `'load'` plus about 2 s.
7. **`fullPage` screenshots stop at the viewport** because the layout scrolls an inner container. Use a tall
   viewport (the helper defaults to 1440x2200) instead.
8. There is **no `<main>` element**, so scope selectors with page-specific text or classes. Headings are `h1`/`h2`.
9. **Headless Chromium can't render PDFs**, so the Texas print page shows an empty iframe. Fetch the PDF
   instead (`page.request.get('/citations/10005/texas-pdf')`), save it, and render it with
   `pdftoppm -r 150 -png file.pdf out` (installed), then view the PNG.
10. **Process control:** unanchored `pkill -f Modules.Records.UI` or `pgrep -f "dotnet Modules.Records.UI.dll"`
    also match the Bash tool's own `bash -c '…'` wrapper (its command text contains the pattern) and kill your
    shell (exit 144). Use `scripts/run-local-demo.sh --stop`, or anchor the pattern:
    `pgrep -f '^dotnet Modules\.Records\.UI\.dll'`.
11. **Docker** is installed but the daemon isn't running. Start it with `dockerd &>/tmp/dockerd.log &` if needed.
    `docker build` of the repo's `Dockerfile` is an alternative to step 2, but it is slower.
12. **Timestamps show in UTC** in the UI (issue #95), which is expected in this container. Don't misread
   this as a seeding bug.
13. The app log is noisy with `OutboxProcessor` "Unknown type …DomainEvent`1" warnings and dead-letter
    errors. That is a known bug (issue #93), not something your change caused.
14. Data persists in `/tmp/authority-demo/data` between runs. Use `--fresh` when a test needs pristine
    seed data, for example to reproduce the demo-quota problem or to get record numbers starting at 10000.

## Known issues already filed (don't re-file; reference them)

From the 2026-11-02 demo review, jneb170/Authority.Records issues #90-#102:
#90 shared demo create quota · #91 demo password changeable / logs everyone out · #92 locking not
demonstrable with a shared account · #93 outbox can't resolve generic events · #94 blank 404 and missing
`/Error` page · #95 UI times in server time zone · #96 `h1` focus outline after navigation · #97 Charges and Logs
shown to Supervisors but Admin-only · #98 Texas citation PDF layout and data · #99 synthetic-looking demo data
· #100 dashboard metrics and greeting · #101 UI polish checklist · #102 vulnerable NuGet packages.
Check the issue list for current state before filing new ones.
