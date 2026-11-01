import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const [serverArgument, outputArgument, consumerArgument] = process.argv.slice(2);
if (!serverArgument || !outputArgument) throw new Error('Usage: node run.mjs <Server executable> <new evidence directory> [consumer DLL]');
const serverPath = fs.realpathSync(serverArgument);
const outputRoot = path.resolve(outputArgument);
const consumerPath = path.resolve(consumerArgument ?? path.join(path.dirname(fileURLToPath(import.meta.url)), 'bin/Release/net10.0/FreeSql.Tests.Provider.SonnetDB.ReleaseAcceptance.dll'));
fs.mkdirSync(outputRoot);
const token = crypto.randomUUID();
const report = { status: 'RUNNING', serverSha256: hash(serverPath), consumerSha256: hash(consumerPath), checks: [], protocolReceipts: {} };
const serverRoot = path.join(outputRoot, 'server');
fs.mkdirSync(serverRoot);
let server;
let allLogs = '';

function hash(file) { return crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex'); }
function child(program, args, options = {}) {
  const process = spawn(program, args, { windowsHide: true, stdio: ['ignore', 'pipe', 'pipe'], ...options });
  process.stdoutText = '';
  process.stderrText = '';
  process.stdout.on('data', bytes => { process.stdoutText += bytes.toString('utf8'); });
  process.stderr.on('data', bytes => { process.stderrText += bytes.toString('utf8'); });
  process.finished = new Promise((resolve, reject) => { process.once('error', reject); process.once('exit', code => resolve(code)); });
  return process;
}
async function runConsumer(scenario, connection, name) {
  const resultFile = path.join(outputRoot, `${name}.json`);
  const process = child('dotnet', [consumerPath, scenario, resultFile], {
    env: { ...globalThis.process.env, SONNETDB_ACCEPTANCE_CONNECTION: connection, NO_PROXY: 'localhost,127.0.0.1,::1' }
  });
  const timer = setTimeout(() => process.kill(), 90000);
  let code;
  try { code = await process.finished; }
  finally {
    clearTimeout(timer);
    fs.writeFileSync(path.join(outputRoot, `${name}.log`), process.stdoutText + process.stderrText);
  }
  if (code !== 0) throw new Error(`${name} failed (${code}); see ${name}.json and ${name}.log`);
  const result = JSON.parse(fs.readFileSync(resultFile, 'utf8'));
  if (result.Status !== 'PASS' || !result.Checks.length) throw new Error(`${name} has no valid PASS report`);
  report.checks.push({ name, count: result.Checks.length, status: result.Status });
  console.log(`PASS ${name}: ${result.Checks.length} business checks`);
}
async function startServer(name) {
  const current = child(serverPath, [
    '--contentRoot', serverRoot, '--hostBuilder:reloadConfigOnChange=false',
    '--Kestrel:Endpoints:Http:Url=http://127.0.0.1:0', '--Kestrel:Endpoints:Http:Protocols=Http1',
    '--Kestrel:Endpoints:FrameH2:Url=http://127.0.0.1:0', '--Kestrel:Endpoints:FrameH2:Protocols=Http2',
    `--SonnetDBServer:DataRoot=${path.join(serverRoot, 'data')}`, '--SonnetDBServer:AllowAnonymousProbes=true',
    `--SonnetDBServer:Tokens:${token}=admin`, '--SonnetDBServer:Mqtt:Enabled=false',
    '--Logging:LogLevel:Microsoft.Hosting.Lifetime=Information', '--Logging:LogLevel:Microsoft.AspNetCore.Hosting.Diagnostics=Information'
  ], { cwd: serverRoot, env: { ...process.env, NO_PROXY: 'localhost,127.0.0.1,::1' } });
  current.name = name;
  server = current;
  const deadline = Date.now() + 60000;
  let addresses;
  while (Date.now() < deadline) {
    if (current.exitCode !== null) throw new Error(`${name} exited during startup`);
    addresses = [...current.stdoutText.matchAll(/Now listening on: (http:\/\/127\.0\.0\.1:\d+)/g)].map(x => x[1]);
    if (addresses.length === 2) break;
    await new Promise(resolve => setTimeout(resolve, 100));
  }
  if (addresses?.length !== 2) throw new Error(`${name} did not expose both listeners`);
  for (const address of addresses) {
    try {
      const response = await fetch(`${address}/healthz`, { signal: AbortSignal.timeout(3000) });
      if (!response.ok) throw new Error('Unhealthy listener');
      current.rest = address;
    } catch { current.frames = address; }
  }
  if (!current.rest || !current.frames) throw new Error('Unable to distinguish HTTP/1.1 and HTTP/2 listeners');
  return current;
}
async function stopServer() {
  if (!server) return;
  const current = server;
  server = undefined;
  if (current.exitCode === null) current.kill();
  await current.finished;
  const safeLogs = (current.stdoutText + current.stderrText).replaceAll(token, '<redacted>');
  allLogs += safeLogs;
  fs.writeFileSync(path.join(outputRoot, `${current.name}.log`), safeLogs);
}
function connection(protocol) {
  const address = protocol === 'rest' ? server.rest : server.frames;
  const database = protocol === 'rest' ? 'release_rest' : 'release_frames';
  return `Data Source=sonnetdb+${address}/${database};Token=${token};Protocol=${protocol};Timeout=30`;
}
try {
  const embedded = `Data Source=${path.join(outputRoot, 'embedded-data')}`;
  await runConsumer('exercise', embedded, 'embedded-exercise');
  await runConsumer('reopen', embedded, 'embedded-reopen');
  await startServer('server-first');
  for (const database of ['release_rest', 'release_frames']) {
    const response = await fetch(`${server.rest}/v1/db`, {
      method: 'POST', headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: database }), signal: AbortSignal.timeout(30000)
    });
    if (!response.ok) throw new Error(`Create acceptance database failed: ${response.status}`);
  }
  for (const protocol of ['rest', 'frame-http2']) await runConsumer('exercise', connection(protocol), `${protocol}-exercise`);
  await stopServer();
  report.restartBoundary = 'Server process terminated after acknowledged commits, then restarted on the same directory; no power-loss claim.';
  await startServer('server-reopened');
  for (const protocol of ['rest', 'frame-http2']) await runConsumer('reopen', connection(protocol), `${protocol}-reopen`);
  await stopServer();
  for (const [name, pattern] of [
    ['restSql', /Request starting HTTP\/1\.1 POST .*\/sql/g],
    ['http2Frames', /Request starting HTTP\/2 POST .*\/v1\/frame/g],
    ['http2Transactions', /Request starting HTTP\/2 POST .*\/sql\/transactions/g]
  ]) {
    const count = [...allLogs.matchAll(pattern)].length;
    if (count === 0) throw new Error(`No actual Server request receipt for ${name}`);
    report.protocolReceipts[name] = count;
  }
  report.status = 'PASS';
} catch (error) {
  report.status = 'FAIL';
  report.failure = error.message;
  process.exitCode = 1;
} finally {
  await stopServer();
  report.finishedUtc = new Date().toISOString();
  fs.writeFileSync(path.join(outputRoot, 'result.json'), JSON.stringify(report, null, 2));
  console.log(JSON.stringify(report));
}
