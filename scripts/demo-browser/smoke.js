// Smoke-crawl a local demo instance as the demo visitor, at desktop and phone widths.
// Prints per-page heading, row count, horizontal overflow and any console/HTTP errors,
// and writes screenshots to $DEMO_WORK_DIR/shots (default /tmp/authority-demo/shots).
//
//   scripts/run-local-demo.sh --fresh
//   node scripts/demo-browser/smoke.js            # or pass routes: node smoke.js /incidents /names
const { start } = require('./lib');

const routes = process.argv.slice(2).length ? process.argv.slice(2) : [
  '/', '/incidents', '/incidents/create', '/incidents/10000', '/arrests', '/arrests/10000',
  '/citations', '/citations/10000', '/names', '/names/10000', '/names/10000/relationships',
  '/locations', '/locations/20000', '/admin/configurations', '/admin/picklists',
  '/admin/test-data', '/admin/read-model-rebuild', '/account/change-password', '/does-not-exist',
];

(async () => {
  let problems = 0;
  for (const vp of [{ name: 'desktop', width: 1440, height: 2200 }, { name: 'mobile', width: 390, height: 1600 }]) {
    const t = await start({ width: vp.width, height: vp.height });
    for (const r of routes) {
      t.events.length = 0;
      try {
        await t.go(r, 2500);
        const info = await t.page.evaluate(() => ({
          path: location.pathname,
          title: document.title,
          heading: document.querySelector('h1,h2')?.innerText.trim() ?? '',
          rows: document.querySelectorAll('table tbody tr').length,
          hOverflow: document.documentElement.scrollWidth > window.innerWidth + 1,
        }));
        if (await t.blazorError()) t.events.push('blazor-error-ui visible');
        await t.shot(`${vp.name}${r.replace(/\//g, '_') || '_home'}`);
        const flag = t.events.length || info.hOverflow ? '!!' : 'ok';
        if (flag !== 'ok') problems++;
        console.log(`${flag} ${vp.name.padEnd(7)} ${r.padEnd(30)} -> ${info.path} | "${info.title}" | ${info.heading} | rows=${info.rows}${info.hOverflow ? ' | H-OVERFLOW' : ''}`);
        t.events.forEach((e) => console.log(`     ${e}`));
      } catch (e) {
        problems++;
        console.log(`!! ${vp.name.padEnd(7)} ${r.padEnd(30)} crawl error: ${e.message.split('\n')[0]}`);
      }
    }
    await t.browser.close();
  }
  console.log(`\n${problems} page(s) flagged. Screenshots: ${require('./lib').SHOTS}`);
})();
