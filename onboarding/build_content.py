"""Build offline course HTML. Run from this repository root. No graph files are generated."""
from pathlib import Path
from html import escape

ROOT = Path(__file__).parent / 'content'
STYLE = '''body{margin:0;background:#f6f8fc;color:#19283c;font:17px/1.6 system-ui,Arial,sans-serif}main,header,footer{max-width:1060px;margin:auto;padding:28px}header{border-bottom:1px solid #ccd5e3}h1{font-size:2.4rem;line-height:1.15}h2{margin-top:2em;color:#183f75}h3{margin-top:1.6em}a{color:#1259a4}a:focus{outline:3px solid #eaaa2d}nav{display:flex;gap:20px;flex-wrap:wrap}.lead{font-size:1.2rem}.notice{background:#fff3d8;border-left:5px solid #a45c00;padding:18px}.card{background:white;border:1px solid #ccd5e3;border-radius:12px;padding:20px;margin:18px 0}table{border-collapse:collapse;width:100%;background:white}td,th{text-align:left;vertical-align:top;border-bottom:1px solid #d9dfeb;padding:12px}th{background:#e7eef8}pre{background:#172b47;color:#edf5ff;padding:18px;overflow:auto;border-radius:8px}code{font-family:Consolas,monospace}li{margin:.5em 0}.muted{color:#52647c}details{background:white;padding:12px;margin:14px 0}@media(max-width:650px){body{font-size:16px}main,header,footer{padding:18px}table{display:block;overflow:auto}h1{font-size:2rem}}'''

def page(path, title, body):
    destination = ROOT / path
    destination.parent.mkdir(parents=True, exist_ok=True)
    depth = len(Path(path).parts) - 1
    home = '../' * depth
    html = f'''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>{escape(title)} · QaaS onboarding</title><style>{STYLE}</style></head><body><header><nav><a href="{home}index.html">Course index</a><a href="{home}setup.html">Setup & runbook</a><a href="{home}sources.html">Source references</a><a href="{home}verification.html">Verification</a></nav></header><main><h1>{escape(title)}</h1>{body}</main><footer>QaaS application maintenance path · five focused days · offline course materials</footer></body></html>'''
    destination.write_text(html, encoding='utf-8')

page('index.html', 'From reproducible test to safe maintenance', '''
<p class="lead">A five-day practical path through OmegaSolider, Mamtzetza and QaaS tests. Start with a working batch, follow one SoldierId, improve the evidence, diagnose faults, and hand over a reviewed change.</p>
<p class="notice">Graph update is on hold until the parent confirms that QaaS Shampoo has stopped and identifies its final graph and exam. No original graph or original exam has been incorporated. The practical assessment here is a newly proposed assessment, pending reconciliation.</p>
<p>Plan for roughly <strong>30 focused hours plus 5–10 hours for setup, review and retries</strong>. This is a planning estimate assuming existing C#, Git, Docker and HTTP ability; the learner's baseline has not been verified. Move at competency gates rather than the calendar.</p>
<table><thead><tr><th>Day</th><th>What to do</th><th>Evidence needed to progress</th></tr></thead><tbody>
<tr><td><a href="days/day1.html">1 · Baseline & trace</a></td><td>Reproduce the stack and trace one ID across input, transform, HTTP and output.</td><td>Clean reset, passing baseline, annotated ID trace.</td></tr>
<tr><td><a href="days/day2.html">2 · Meaningful assertions</a></td><td>Compare built-in count/contract assertions with ID and semantic checks. Exercise boundaries and mutations.</td><td>A bad output fails for the right reason; all eight age boundaries covered.</td></tr>
<tr><td><a href="days/day3.html">3 · API & diagnosis</a></td><td>Test API disabled, success and failure. Distinguish config, service and test failures.</td><td>Three-mode evidence table and diagnosis supported by logs, HTTP and metrics.</td></tr>
<tr><td><a href="days/day4.html">4 · Maintenance change</a></td><td>Write a failing regression, make one bounded fix and verify its CI/release impact.</td><td>Failing-before/passing-after evidence, small reviewed diff and exact CI commit.</td></tr>
<tr><td><a href="days/day5.html">5 · Practical & handover</a></td><td>Diagnose an unseen seeded maintenance defect and hand over a reproducible repair.</td><td>All five competency gates and a reviewer-reproducible report.</td></tr>
</tbody></table>
<h2>Everything in this package</h2><ul>
<li><a href="setup.html">Prerequisites, exact commands, readiness, reset and diagnosis runbook</a></li>
<li><a href="student/assessment.html">Student practical assessment (new proposal)</a> and <a href="student/starter.html">working starter and intentional TODOs</a></li>
<li><a href="teacher/answers.html">Teacher answers and assessment seed (keep separate from student distribution)</a></li>
<li><a href="rubric.html">Assessment rubric and competency gates</a></li>
<li><a href="graph-status.html">Graph coordination status and finalization gate</a></li>
<li><a href="sources.html">Local source references, repository links and specification workbook</a></li>
<li><a href="verification.html">Exact commits, local/CI results, commands and limitations</a></li>
<li><a href="manifest.json">Package manifest and file hashes</a></li></ul>
<h2>Choose the ownership boundary</h2><p>This week prepares maintenance of an application and its QaaS tests, with existing Runner/Mocker packages. Whether the role also owns QaaS Framework or Runner remains unconfirmed. Framework changes need an additional platform track: hook contracts, protocol/serialization/policy ownership, downstream restore compatibility and reviewed releases. Do not treat this week as certification for framework ownership.</p>
''')

days = [
('Day 1 — Reproducible baseline and one SoldierId', '''
<p><strong>Goal:</strong> another person can repeat your baseline from the recorded commits. Budget: 6 focused hours.</p>
<ol><li><strong>0–1h:</strong> verify the prerequisites and sibling checkout layout in <a href="../setup.html">setup</a>. Record SDK, Docker/Compose versions, commit hashes and API flag.</li>
<li><strong>1–2h:</strong> run the isolated reset and baseline. Save exit code, baseline.json, stdout, Allure and CI URL (if available). Distinguish local results from remote CI.</li>
<li><strong>2–4h:</strong> pick one generated SoldierId from the actual publication log. Trace its fixture and serialized input into the direct exchange/routing key, worker queue, transformer, GET buff, POST title, output exchange and consumer.</li>
<li><strong>4–5h:</strong> inspect <a href="../sources/OmegaSolider/Protos/omega.proto">omega.proto</a>. Explain tags 1–12 and output tags 13–20. Compare literal expected values with actual output. Identify where an output count could conceal a duplicated ID.</li>
<li><strong>5–6h:</strong> reset and repeat once. Hand the commands to a peer to reproduce. Record any setup problem separately from a business failure.</li></ol>
<h2>Trace worksheet</h2><p>Record: actual SoldierId; fixture row; serialized type; input exchange/queue/routing key; worker receive log; transform log; GET URI; POST request/response; output fields; publication log; consumer result; the assertion that checked it. Use measured evidence; mark any observation you could not capture.</p>
<p><strong>Gate:</strong> clean reproduction and correct identity trace. A screenshot of a green count is insufficient. If prerequisites consume the day, use setup allowance and defer the gate.</p>
<p>Read <a href="../sources.html">source references</a>. Submit the trace and exact command transcript in your handover folder.</p>
'''),
('Day 2 — Assertions that detect wrong behavior', '''
<p><strong>Goal:</strong> prove that the checker rejects broken behavior. Budget: 6 focused hours.</p>
<ol><li><strong>0–1h:</strong> read the eight-row <a href="../sources/mamtzetza-qaas-tests/tests_specification.xlsx">specification</a> and YAML. Explain what each built-in assertion proves and its limits.</li>
<li><strong>1–3h:</strong> run the <a href="../sources/mamtzetza-qaas-tests/regression/EvidenceRegressionTests.cs">assertion regressions</a>. Compare missing/duplicate ID failures with count-only checks. Review why literal fixtures avoid a copied transformation algorithm.</li>
<li><strong>3–4h:</strong> complete starter TODO A: exact age expectations for 24/25, 34/35, 44/45 and 54/55. Avoid deriving expected values with the same rule under test.</li>
<li><strong>4–5h:</strong> in a disposable experiment, mutate one preserved field, one derived field, the title and the timestamp. Capture the specific failing assertion for each and revert the mutation.</li>
<li><strong>5–6h:</strong> explain why a parser accepting protobuf defaults does not prove field correctness; why 5 outputs can hide 1 lost + 1 duplicated record; and why a five-message batch does not prove continuous throughput.</li></ol>
<pre>dotnet test sources/mamtzetza-qaas-tests/regression/AssertionRegressionTests.csproj -c Release
dotnet test sources/Mamtzetza/Mamtzetza.slnx -c Release</pre>
<p><strong>Gate:</strong> each intentional mutation fails for a meaningful reason and the restored baseline passes. Submit a mutation/result table with one representative failing message, not just a final green run.</p>
'''),
('Day 3 — API modes and evidence-based diagnosis', '''
<p><strong>Goal:</strong> distinguish a configuration mistake from a broken service or weak test. Budget: 6 focused hours.</p>
<ol><li><strong>0–1h:</strong> run the default API-success batch. Identify two HTTP requests per soldier and one successful enrichment-attempt counter increment.</li>
<li><strong>1–2h:</strong> inspect the disabled-mode unit test: the handler must never be called; buff is exactly the no-API value; glow contains no bonus. If running a disabled E2E experiment, change both logical and metrics expectations with the service flag.</li>
<li><strong>2–4h:</strong> complete starter TODO B for a failed HTTP request. Exercise GET 503 and POST 503 separately. Confirm base glow, fallback prefix and bounded timestamp. Use a fake handler rather than the public internet.</li>
<li><strong>4–5h:</strong> compare API failure, unavailable broker and missing log capture. For each, write symptom, plausible causes, discriminating observation, cause, repair and rerun result.</li>
<li><strong>5–6h:</strong> inspect metrics baseline/final deltas and correlate log IDs. Repeat one diagnosis from fresh state.</li></ol>
<table><tr><th>Mode</th><th>Expected HTTP behavior</th><th>Expected result</th></tr><tr><td>Disabled</td><td>No request</td><td>No-API buff, base glow</td></tr><tr><td>Success</td><td>GET + POST succeed</td><td>Exact buff/title, base + bonus</td></tr><tr><td>Failure</td><td>GET or POST fails</td><td>Fallback buff, base glow; failure attempt increments</td></tr></table>
<p><strong>Gate:</strong> all three modes have reproducible evidence and a correct diagnosis. Do not change thresholds to make an unexplained failure disappear.</p>
'''),
('Day 4 — A bounded maintenance fix', '''
<p><strong>Goal:</strong> repair one defect with a regression and understand its release boundary. Budget: 6 focused hours.</p>
<ol><li><strong>0–1h:</strong> reproduce a candidate defect in a disposable branch. Suggested learning cases: minimum signed lucky-number overflow or swallowed caller cancellation. Both are already fixed in the supplied reference, so the instructor must restore the old behavior in a disposable copy first.</li>
<li><strong>1–3h:</strong> write a focused test with explicit expected behavior. Run it against the defective version and retain the failure output. Complete starter TODO C as a meaningful regression.</li>
<li><strong>3–4h:</strong> make the smallest safe service fix. Review cancellation semantics, full signed integer domain and resource disposal. Preserve protobuf tags.</li>
<li><strong>4–5h:</strong> run the focused test and relevant complete suites. Run the coordinated E2E stack. Review a diff containing only the intended change and required documentation.</li>
<li><strong>5–6h:</strong> draft a PR explaining trigger, behavior before/after and validation. Match remote CI to the exact head commit. Explain whether this is application code, an external hook or framework core.</li></ol>
<p><strong>Release discussion:</strong> application/test changes do not require a QaaS Framework release. A published protobuf contract change requires a reviewed package version and downstream compatibility checks. Framework SDK/core changes ripple through Common packages, Runner/Mocker, mirror and docs; that is outside this bounded exercise.</p>
<p><strong>Gate:</strong> preserved failing-before evidence, passing-after tests, a scoped reviewed diff and accurate CI status. A blocked CI run must be labeled blocked with its exact cause.</p>
'''),
('Day 5 — Unseen practical and handover', '''
<p><strong>Goal:</strong> independently diagnose and repair a newly seeded maintenance fault. Budget: 6 focused hours.</p>
<ol><li><strong>0–0.5h:</strong> instructor prepares an unseen disposable working copy using the separate teacher instructions. The learner receives <a href="../student/assessment.html">the new practical brief</a> and the working infrastructure.</li>
<li><strong>0.5–2h:</strong> reproduce, gather evidence and narrow the cause. State observations before proposing code changes.</li>
<li><strong>2–4h:</strong> add a focused regression, show it failing before, repair the bounded defect and run relevant suites plus E2E.</li>
<li><strong>4–5h:</strong> prepare the handover: exact commits and commands, test outputs, CI URL/status, limitations, cleanup command and rollback plan.</li>
<li><strong>5–6h:</strong> reviewer reproduces from your report and asks you to explain contract compatibility, assertion strength, API fallback and release impact.</li></ol>
<p>Assess with <a href="../rubric.html">the rubric</a>. All five competency gates are required. If the learner cannot complete one gate, record the evidence and a focused retry plan; elapsed time alone is not completion.</p>
<p class="notice">This assessment is a newly proposed practical. The original exam from QaaS Shampoo is still pending identification and reconciliation.</p>
''')]
for index, (title, body) in enumerate(days, 1):
    page(f'days/day{index}.html', title, body)

page('setup.html', 'Prerequisites and operating runbook', '''
<h2>Start from the included source snapshots</h2><p>Extract the entire ZIP. Keep <code>sources/OmegaSolider</code>, <code>sources/Mamtzetza</code> and <code>sources/mamtzetza-qaas-tests</code> together. Do not flatten the directories. The code is self-contained in this package; first restore and Docker image pulls require network access.</p>
<p>Install .NET SDK 10.0.203, Docker Desktop with Linux containers and Compose v2, Git and PowerShell 7. Check the versions before proceeding:</p>
<pre>dotnet --version
docker version
docker compose version
git --version</pre>
<p>Ports 5672, 15672, 8080 and 9090 must be free. Use the dedicated Compose project. Do not stop another learner's stack or delete unrelated Docker volumes.</p>
<h2>Build and local tests</h2><pre>dotnet restore sources/Mamtzetza/Mamtzetza.slnx --configfile sources/Mamtzetza/NuGet.config
dotnet test sources/Mamtzetza/Mamtzetza.slnx -c Release --no-restore --logger trx
dotnet restore sources/mamtzetza-qaas-tests/MamtzetzaTests.slnx --configfile sources/mamtzetza-qaas-tests/NuGet.config
dotnet test sources/mamtzetza-qaas-tests/regression/AssertionRegressionTests.csproj -c Release --no-restore --logger trx
dotnet test sources/OmegaSolider/tests/OmegaSolider.ContractTests/OmegaSolider.ContractTests.csproj -c Release --logger trx</pre>
<h2>Clean end-to-end run</h2><pre>cd sources/mamtzetza-qaas-tests
./scripts/Run-Isolated.ps1 -Reset</pre>
<p>The reset is destructive only to the named <code>qaas-onboarding</code> project. The runner polls readiness for up to 120 seconds, captures pre-batch metric values/start time and then executes the batch. Keep the default API flag true for the supplied E2E expectations. Every batch generates fresh IDs; run in isolation to make numeric deltas meaningful.</p>
<h2>Evidence and cleanup</h2><p>Save <code>allure-results/</code>, <code>evidence/baseline.json</code>, <code>evidence/mamtzetza.jsonl</code>, <code>evidence/compose.log</code>, test exit code and TRX. The verification page distinguishes outputs captured for this delivery from instructions to run yourself.</p>
<pre>docker compose -p qaas-onboarding down --volumes --remove-orphans</pre>
<h2>Diagnose before changing code</h2><table><tr><th>Symptom</th><th>Discriminating check</th><th>Action</th></tr>
<tr><td>Docker API unavailable</td><td><code>docker version</code> lacks server</td><td>Start Docker Desktop normally; select Linux containers. Keep security/network settings unchanged.</td></tr>
<tr><td>Missing sibling build context</td><td>Check <code>../Mamtzetza/src/Mamtzetza/Mamtzetza.csproj</code></td><td>Restore included sibling layout.</td></tr>
<tr><td>Readiness timeout</td><td>Broker health, queue consumers/messages, both mock routes, metrics and stdout file</td><td>Repair the specific dependency; reset only the isolated project if queues are stale.</td></tr>
<tr><td>Buff mismatch</td><td>Compare worker API flag with both assertion configurations</td><td>Align scenario and expectations; retain off/success/failure coverage.</td></tr>
<tr><td>Metric delta mismatch</td><td>Inspect numeric baseline/final values and other publishers</td><td>Remove concurrent traffic or investigate dropped/duplicated work. Do not substitute name checks.</td></tr>
<tr><td>Missing actual logs</td><td>Worker LOG_FILE_PATH and shared run-evidence volume</td><td>Repair capture. Do not paste a sample log to pass the assertion.</td></tr>
<tr><td>NuGet timeout</td><td>Restore output distinguishes network from compile/test failure</td><td>Retry supported restore on a working connection; retain failures in report.</td></tr></table>
<p>Full Elasticsearch/Fluent Bit administration is optional. The current collector tails the actual shared stdout file. See the <a href="sources/mamtzetza-qaas-tests/README.md">tests README</a> for local-process alternatives and the optional profile.</p>
''')

page('student/starter.html', 'Student starter', '''
<p>The supplied infrastructure and reference repositories are complete. Your exercise project contains one working baseline and three explicitly skipped TODOs, keeping intentional gaps separate from setup defects.</p>
<pre>dotnet restore student/Starter/Starter.csproj --configfile sources/Mamtzetza/NuGet.config
dotnet test student/Starter/Starter.csproj -c Release --no-restore</pre>
<p>Open <a href="Starter/MaintenanceExercises.cs">MaintenanceExercises.cs</a> and <a href="Starter/Starter.csproj">Starter.csproj</a>.</p>
<ol><li>TODO A: replace the skipped placeholder with a theory covering all eight age boundaries using literal expectations.</li><li>TODO B: add an HTTP failure test with a fake handler; check request behavior, fallback, base glow and timestamp.</li><li>TODO C: add an independently chosen meaningful regression that rejects a real mutation. Retain failing-before output.</li></ol>
<p>Remove each Skip attribute only when implementing that task. Skipped tests do not satisfy competency gates. The full teacher answers and reference regressions are in the separately labeled teacher section; instructors should distribute only student material during assessment.</p>
<p>Record commands, evidence and interpretation in a handover folder. Do not edit protobuf tags or weaken assertions to make tests pass.</p>
''')

page('student/assessment.html', 'Practical maintenance assessment — new proposal', '''
<p class="notice">This is a newly proposed assessment, pending reconciliation with the original QaaS Shampoo exam. It is not a recovered copy of that exam.</p>
<p>You receive a working disposable copy containing one bounded maintenance defect. A reviewer has observed incorrect behavior in a valid input case. Diagnose the cause and submit a safe repair with meaningful tests.</p>
<ol><li>Reproduce the baseline and the reported symptom. Record exact commands and observed results.</li><li>Trace a real SoldierId through contract, input queue, transform, mock HTTP and output. Identify the earliest point where behavior diverges from expectations.</li><li>Write an independent regression that fails on the defective version. Include explicit expected fields and the evidence needed to distinguish service, configuration and assertion failures.</li><li>Make a small repair. Run the focused regression, relevant complete suites and the coordinated end-to-end batch. Preserve protobuf compatibility.</li><li>Prepare a reviewable diff and handover with commits, CI URL and exact head, results, limitations, cleanup and rollback. Explain release impact and ownership boundary.</li></ol>
<h2>Commands for the prepared assessment lab</h2><p>After the instructor supplies the lab, run these from the package root. They build the service inside <code>student/lab/Mamtzetza</code>; the lab starter and Compose suite both target that same service.</p><pre>./student/lab/Run-Assessment.ps1
./student/lab/Run-Assessment.ps1 -E2E</pre><p>The general setup and root starter commands remain reference-training commands. They do not assess your seeded lab. Repair only the lab source, then rerun both assessment commands.</p><p><strong>Submission:</strong> code diff, failing-before/passing-after logs, ID trace, test report, short diagnosis and review notes. See <a href="../rubric.html">rubric</a>. Ask for missing evidence when necessary rather than inventing it. CI blocked by unavailable access is reported as blocked; it is never replaced by a claim that local tests are CI.</p>
<p>Allowed: source code, official package documentation, the supplied runbook and normal development tools. During the unseen exercise, teacher seed instructions and answer sources are withheld by the instructor.</p>
''')

page('teacher/answers.html', 'Teacher answers and assessment administration', '''
<p><strong>Teacher material — separate from student distribution.</strong> These answers explain the supplied implementation. The original exam remains pending; this new practical needs instructor reconciliation before final use.</p>
<h2>Reference answers by day</h2><ol><li><strong>Baseline:</strong> the default stack enables API enrichment; local worker default is false. The direct routing key is <code>/</code>. Fields 1–12 are preserved, 13–17 are derived, 18 is glow, 19 buff/title and 20 current UTC epoch milliseconds. Logs, outputs and metrics must refer to the current isolated batch.</li>
<li><strong>Assertions:</strong> contract deserialization and counts are useful but insufficient. The logical checker compares all preserved fields, literal derived fixtures and exact enrichment, rejects missing/extra/duplicate IDs and checks current-run time. A copied transform can reproduce the same bug. See <a href="../sources/mamtzetza-qaas-tests/regression/EvidenceRegressionTests.cs">27 assertion regression cases</a>.</li>
<li><strong>API:</strong> off means zero requests and base glow. Success requires GET + POST and exact title/buff. A GET or POST HTTP failure falls back with base glow. External API metrics count one enrichment attempt per soldier. Caller-requested cancellation propagates. See <a href="../sources/Mamtzetza/tests/Mamtzetza.Tests/FireflyTransformerTests.cs">existing off/success tests</a> and <a href="../sources/Mamtzetza/tests/Mamtzetza.Tests/MaintenanceRegressionTests.cs">boundary/failure/edge regressions</a>.</li>
<li><strong>Maintenance:</strong> <code>Math.Abs(int.MinValue)</code> overflows; widen to long before absolute value and reduce before casting. Cancellation must escape the general fallback catch. The package verification includes both tests failing against the original transformer and passing after the fix. Wire comments/docs do not require a contract release.</li>
<li><strong>Assessment:</strong> use a fresh disposable seed, retain its exact mutation, then evaluate evidence and explanation with the rubric. Do not hand the learner this page during the unseen exercise.</li></ol>
<h2>Literal first-fixture answer</h2><p>John Vance: Captain, age 22, Shawarma, Star Trek: TNG, shoe 42, height 175, weight 72, lucky 10, Chess, Mars. Expected technology: Antimatter Warp Core; team: Martian Dust Devils; commander: Captain Jean-Luc Picard; woman: Ada Lovelace; language: Rust; base glow: 211; API-success glow: 261; buff/title: <code>Quantum Disco Sparkles - Supreme Commander of Shawarma</code>. Use a current-run timestamp, not a hardcoded historical time.</p>
<h2>Starter solutions</h2><p>TODO A maps 24→Rust, 25/34→C#, 35/44→Python, 45/54→C++, 55→LISP. TODO B can use <code>ApiFailurePreservesBaseGlowAndTimestamp</code> as a reference, including GET and POST failures. TODO C should reproduce a real defect such as minimum lucky-number overflow or cancellation, not merely assert that a method returns a nonnull object.</p>
<h2>Prepare the new unseen assessment</h2><pre>python teacher/prepare_assessment.py</pre><p>This creates a self-contained <code>student/lab</code> with sibling Mamtzetza and QaaS test repositories, plus a starter retargeted to that lab service. It changes one age boundary and adds a literal age-34 E2E fixture and starter acceptance case. It refuses to overwrite an existing lab. Give the learner the lab, the student brief and runbook; withhold this page and the seed source. Expected repair restores the 35 threshold and adds explicit checks for 34 and 35. Existing unit coverage should reveal the seeded fault, so require a diagnosis and an additional independent regression or integration explanation; a one-line threshold fix alone is insufficient.</p>
<p>Use exactly <code>./student/lab/Run-Assessment.ps1</code> for service plus starter tests, and <code>./student/lab/Run-Assessment.ps1 -E2E</code> for Docker integration. Both commands target the seeded lab source and must fail before repair, then pass after restoring the age threshold. Do not use root reference commands to assess the lab. CI verifies these exact commands and the specific failing assertion before accepting the repair. The seed script is supplied for instructor use and its generated lab is excluded from the package. Do not mutate the original laptop checkout or active Shampoo workspace.</p>
''')

page('rubric.html', 'Assessment rubric', '''
<p>Score each competency 0–2: <strong>0</strong> missing or incorrect; <strong>1</strong> partial or needs substantial help; <strong>2</strong> independently demonstrated with reproducible evidence. Progress requires <strong>2 on all five gates</strong>; a high total cannot compensate for unsafe changes or weak tests.</p>
<table><tr><th>Gate</th><th>Evidence for 2</th><th>Retry when incomplete</th></tr>
<tr><td>Reproducibility</td><td>Fresh isolated reset, exact commits/versions/commands, another person repeats the result.</td><td>Repair setup and rerun from clean state.</td></tr>
<tr><td>Correct diagnosis</td><td>Traceable observations separate config, service and test faults; cause explains the symptom.</td><td>Gather one discriminating observation and revise the hypothesis.</td></tr>
<tr><td>Meaningful tests</td><td>Independent expectations, relevant boundaries, mutation rejected, failing-before/passing-after retained.</td><td>Replace weak checks and prove a bad implementation fails.</td></tr>
<tr><td>Safe reviewed change</td><td>Bounded diff, preserved contract tags, no unrelated edits, appropriate local tests and exact-head CI verification or accurately documented blocker.</td><td>Reduce diff, review impact and resolve recoverable failures.</td></tr>
<tr><td>Explanation & handover</td><td>Clear cause/repair, evidence links, limitations, release boundary, reset and rollback steps.</td><td>Ask a peer to follow the report and fill specific gaps.</td></tr></table>
<p>Remote CI blocked by access cannot be marked green. Record it as a delivery gate requiring follow-up. The ownership boundary (tests atop QaaS versus Framework/Runner ownership) must be resolved before claiming broader readiness.</p>
''')

page('sources.html', 'Source references and repository links', '''
<p>All implementation claims in this course trace to the included source snapshots. Exact base/head commits, PRs and CI runs are listed in <a href="verification.html">verification</a>.</p><ul>
<li><a href="sources/OmegaSolider/Protos/omega.proto">Published wire schema</a>; <a href="sources/OmegaSolider/README.md">contract documentation</a>; <a href="sources/OmegaSolider/tests/OmegaSolider.ContractTests/WireContractTests.cs">wire regressions</a>; <a href="https://github.com/eldarush/OmegaSolider">OmegaSolider repository</a>.</li>
<li><a href="sources/Mamtzetza/src/Mamtzetza/FireflyTransformer.cs">Transformation and API behavior</a>; <a href="sources/Mamtzetza/src/Mamtzetza/MamtzetzaWorker.cs">RabbitMQ flow</a>; <a href="sources/Mamtzetza/src/Mamtzetza/Program.cs">configuration and UTC logging</a>; <a href="sources/Mamtzetza/src/Mamtzetza/StdoutCapture.cs">actual stdout capture</a>; <a href="sources/Mamtzetza/src/Mamtzetza/MamtzetzaMetrics.cs">metrics</a>; <a href="sources/Mamtzetza/README.md">service README</a>; <a href="https://github.com/eldarush/Mamtzetza">Mamtzetza repository</a>.</li>
<li><a href="sources/mamtzetza-qaas-tests/tests/test-docker.qaas.yaml">Docker test YAML</a>; <a href="sources/mamtzetza-qaas-tests/tests/test.qaas.yaml">local YAML</a>; <a href="sources/mamtzetza-qaas-tests/docker-compose.yml">Compose setup</a>; <a href="sources/mamtzetza-qaas-tests/tests/FixtureReadiness.cs">bounded readiness</a>; <a href="sources/mamtzetza-qaas-tests/tests/fixtures/soldiers.json">literal expectations</a>; <a href="sources/mamtzetza-qaas-tests/tests/Assertions/EvidenceValidation.cs">evidence validation</a>; <a href="sources/mamtzetza-qaas-tests/README.md">tests README</a>; <a href="sources/mamtzetza-qaas-tests/tests_specification.xlsx">eight specifications workbook</a>; <a href="https://github.com/eldarush/mamtzetza-qaas-tests">tests repository</a>.</li></ul>
<p>Optional platform reference: <a href="https://docs.qaas.online">QaaS documentation</a>. The course uses the versions recorded in the included project files and does not require modifying QaaS Framework.</p>
''')

page('graph-status.html', 'Graph coordination gate', '''
<p class="notice">On hold. No graph has been edited, replaced, regenerated or finalized by this implementation task.</p>
<p>The supported task inventory identified <strong>QaaS Shampoo</strong>, task ID <code>01a1162c-41f4-7f21-9060-47920718f262</code>, as active during implementation. This is status evidence only; completion must be explicitly confirmed by the parent.</p>
<p>Required before final graph integration: parent confirmation that the session has stopped; exact final graph and exam artifacts; provenance and source commit identification. Then reconcile implemented changes with those artifacts, preserve graph source and rendered view, update relative links and rerun package verification. Do not infer completion from timestamps or inactivity.</p>
<p>The package currently contains a separately labeled proposed practical assessment. It does not claim that the original exam or graph has been recovered.</p>
''')

# The packager replaces this page with measured results and exact run metadata.
page('verification.html', 'Verification report', '<p>Packaging has not yet populated the measured verification report. This is a preparation build, not a claim of completed delivery.</p>')
print(f'Built course content in {ROOT}')
