// Playwright helper for driving a local demo instance started with scripts/run-local-demo.sh.
//
//   const { start } = require('./lib');
//   const t = await start();                 // signs in via "Explore the live demo"
//   await t.go('/incidents'); await t.shot('incidents');
//   console.log(t.events);                   // console errors, page errors, HTTP >= 400
//   await t.browser.close();
//
// Handles the cloud-session quirks:
//  * Chromium is pre-installed; require('playwright') works from any directory.
//  * Outbound traffic must go through $HTTPS_PROXY (localhost bypassed).
//  * cdn.jsdelivr.net is blocked, so Bootstrap requests are answered from the npm-registry
//    cache that run-local-demo.sh writes to $DEMO_WORK_DIR/cdn.
//  * The layout scrolls an inner container, so `fullPage` screenshots stop at the viewport.
//    Use a tall viewport (default 1440x2200) to capture whole pages.
//  * Blazor Server keeps a websocket open: never wait for 'networkidle'; wait for 'load'
//    plus a short delay for the circuit to render.
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const WORK = process.env.DEMO_WORK_DIR || '/tmp/authority-demo';
const BASE = process.env.DEMO_BASE_URL || `http://127.0.0.1:${process.env.DEMO_PORT || 8080}`;
const SHOTS = process.env.DEMO_SHOTS_DIR || path.join(WORK, 'shots');

/**
 * @param {object} [o]
 * @param {number} [o.width=1440]
 * @param {number} [o.height=2200]
 * @param {'demo'|{email:string,password:string}|null} [o.login='demo']  null = stay signed out
 */
async function start({ width = 1440, height = 2200, login = 'demo' } = {}) {
  fs.mkdirSync(SHOTS, { recursive: true });
  const proxy = process.env.HTTPS_PROXY ? { server: process.env.HTTPS_PROXY, bypass: '127.0.0.1,localhost' } : undefined;
  const browser = await chromium.launch({ proxy });
  const ctx = await browser.newContext({ viewport: { width, height }, acceptDownloads: true });

  await ctx.route(/cdn\.jsdelivr\.net\/npm\/([a-z-]+)@([\d.]+)\/(.*)$/, (route) => {
    const m = route.request().url().match(/npm\/([a-z-]+)@([\d.]+)\/([^?#]*)/);
    const file = path.join(WORK, 'cdn', `${m[1]}-${m[2]}`, 'package', m[3]);
    return fs.existsSync(file) ? route.fulfill({ path: file }) : route.continue();
  });

  const page = await ctx.newPage();
  const events = [];
  page.on('console', (m) => { if (['error', 'warning'].includes(m.type())) events.push(`console.${m.type()}: ${m.text().slice(0, 300)}`); });
  page.on('pageerror', (e) => events.push(`pageerror: ${e.message.slice(0, 300)}`));
  page.on('response', (r) => { if (r.status() >= 400) events.push(`http${r.status()}: ${r.url()}`); });
  page.on('dialog', (d) => { events.push(`dialog: ${d.message()}`); d.accept(); });

  await page.goto(`${BASE}/Account/Login`);
  if (login === 'demo') {
    await page.click('text=Explore the live demo');
  } else if (login) {
    await page.fill('input[name="Input.Email"]', login.email);
    await page.fill('input[name="Input.Password"]', login.password);
    await page.click('button:has-text("Sign In")');
  }
  await page.waitForLoadState('load');
  await page.waitForTimeout(2000);

  const go = async (p, wait = 2000) => { await page.goto(BASE + p, { waitUntil: 'load' }); await page.waitForTimeout(wait); };
  const shot = (name) => page.screenshot({ path: path.join(SHOTS, `${name}.png`), fullPage: true });
  const blazorError = () => page.evaluate(() => { const e = document.getElementById('blazor-error-ui'); return !!e && getComputedStyle(e).display !== 'none'; });
  return { browser, ctx, page, events, go, shot, blazorError, BASE, SHOTS };
}

module.exports = { start, BASE, WORK, SHOTS };
