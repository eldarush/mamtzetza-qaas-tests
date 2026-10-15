from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[1]
source = root / 'sources' / 'Mamtzetza'
target = root / 'student' / 'lab' / 'Mamtzetza'
if target.exists():
    raise SystemExit('Refusing to overwrite an existing student lab. Archive it explicitly first.')
shutil.copytree(source, target, ignore=shutil.ignore_patterns('.git', 'bin', 'obj', 'TestResults'))
file = target / 'src' / 'Mamtzetza' / 'FireflyTransformer.cs'
text = file.read_text(encoding='utf-8-sig')
before = 'if (age < 35) return "C#";'
if text.count(before) != 1:
    raise SystemExit('Expected threshold not found exactly once; inspect source before seeding.')
file.write_text(text.replace(before, 'if (age < 34) return "C#";'), encoding='utf-8')
print(f'Prepared disposable assessment at {target}. Teacher: verify the intended boundary failure before use.')
