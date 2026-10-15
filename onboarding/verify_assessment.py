"""Verify exactly the documented learner commands against a seeded and repaired self-contained lab."""
from pathlib import Path
import argparse
import json
import shutil
import subprocess
import sys

parser=argparse.ArgumentParser()
parser.add_argument('--e2e',action='store_true')
parser.add_argument('--output',required=True)
args=parser.parse_args()
repo=Path(__file__).resolve().parents[1]
out=Path(args.output).resolve()
if out.exists():raise SystemExit('Verification output must be a new disposable directory.')
out.mkdir(parents=True)
for name in ('Mamtzetza','mamtzetza-qaas-tests'):
    source=repo if name=='mamtzetza-qaas-tests' else repo.parent/name
    shutil.copytree(source,out/'sources'/name,
        ignore=shutil.ignore_patterns('.git','bin','obj','TestResults','allure-results','evidence','onboarding'))
shutil.copytree(repo/'onboarding/content/student/Starter',out/'student/Starter')
shutil.copytree(repo/'onboarding/content/teacher',out/'teacher')
subprocess.run([sys.executable,str(out/'teacher/prepare_assessment.py')],check=True)
lab=out/'student/lab'
commands=[['pwsh','-NoProfile','-File',str(lab/'Run-Assessment.ps1')]]
if args.e2e:commands.append(commands[0]+['-E2E'])
summary=[]
for phase,expected in [('before',1),('after',0)]:
    if phase=='after':
        file=lab/'Mamtzetza/src/Mamtzetza/FireflyTransformer.cs'
        source=file.read_text(encoding='utf-8-sig')
        old='if (age < 34) return "C#";'
        if source.count(old)!=1:raise SystemExit('Seed no longer matches intended repair.')
        file.write_text(source.replace(old,'if (age < 35) return "C#";'),encoding='utf-8')
    for index,command in enumerate(commands):
        kind='e2e' if index else 'unit'
        result=subprocess.run(command,cwd=out,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True,errors='replace')
        (out/f'{phase}-{kind}.log').write_text(result.stdout,encoding='utf-8')
        if result.returncode!=expected:
            print(result.stdout[-10000:]);raise SystemExit(f'{phase} {kind}: expected exit {expected}, got {result.returncode}')
        if kind=='unit' and phase=='before' and ('ReportedBoundaryBehavior' not in result.stdout or 'AgeBoundaries' not in result.stdout):
            raise SystemExit('Expected both the seeded service and retargeted starter to reject the boundary.')
        if kind=='e2e':
            suite=lab/'mamtzetza-qaas-tests'
            baseline=json.loads((suite/'evidence/baseline.json').read_text(encoding='utf-8-sig'))
            results=[json.loads(p.read_text(encoding='utf-8-sig')) for p in (suite/'allure-results').glob('*result.json')]
            results=[r for r in results if r['start']>=baseline['StartedAtUnixMs']]
            statuses={r['name']:r['status'] for r in results}
            if len(statuses)!=8:raise SystemExit(f'Missing assessment E2E evidence: {statuses}')
            if phase=='before':
                logical=next(r for r in results if r['name']=='TransformationBusinessLogic')
                if logical['status']!='failed' or 'favorite_coding_language' not in logical.get('statusDetails',{}).get('message',''):
                    raise SystemExit(f'Wrong seeded E2E failure: {logical}')
            elif any(value!='passed' for value in statuses.values()):raise SystemExit(f'Repaired assessment E2E not green: {statuses}')
            (out/f'{phase}-e2e-results.json').write_text(json.dumps(statuses,indent=2),encoding='utf-8')
        summary.append({'phase':phase,'kind':kind,'command':' '.join(command),'exit':result.returncode})
(out/'verification.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print(json.dumps(summary,indent=2))
