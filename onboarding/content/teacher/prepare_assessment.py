from pathlib import Path
import json
import shutil

root = Path(__file__).resolve().parents[1]
lab = root / 'student' / 'lab'
if lab.exists():
    raise SystemExit('Refusing to overwrite an existing student lab. Archive it explicitly first.')
for name in ('Mamtzetza', 'mamtzetza-qaas-tests'):
    shutil.copytree(root / 'sources' / name, lab / name,
        ignore=shutil.ignore_patterns('.git', 'bin', 'obj', 'TestResults', 'allure-results', 'evidence', 'onboarding'))
target = lab / 'Mamtzetza'
shutil.copytree(root / 'student' / 'Starter', lab / 'Starter',
    ignore=shutil.ignore_patterns('bin', 'obj', 'TestResults'))
starter = lab / 'Starter' / 'Starter.csproj'
project = starter.read_text(encoding='utf-8-sig')
reference = '../../sources/Mamtzetza/src/Mamtzetza/Mamtzetza.csproj'
if project.count(reference) != 1:
    raise SystemExit('Expected starter project reference not found exactly once.')
starter.write_text(project.replace(reference, '../Mamtzetza/src/Mamtzetza/Mamtzetza.csproj'), encoding='utf-8')
(lab / 'Starter' / 'AssessmentAcceptanceTests.cs').write_text('''using Mamtzetza;
using Xunit;

public class AssessmentAcceptanceTests
{
    [Theory]
    [InlineData(34, "C#")]
    [InlineData(35, "Python")]
    public void ReportedBoundaryBehavior(int age, string expected) =>
        Assert.Equal(expected, FireflyTransformer.CalculateFavoriteCodingLanguage(age));
}
''', encoding='utf-8')
# The assessment E2E must include the reported boundary. Its expected value is literal.
fixture_path = lab / 'mamtzetza-qaas-tests' / 'tests' / 'fixtures' / 'soldiers.json'
fixtures = json.loads(fixture_path.read_text(encoding='utf-8-sig'))
fixtures[0]['input']['age'] = 34
fixtures[0]['expected']['favoriteCodingLanguage'] = 'C#'
fixture_path.write_text(json.dumps(fixtures, indent=2), encoding='utf-8')
shutil.copy2(root / 'teacher' / 'Run-Assessment.ps1', lab / 'Run-Assessment.ps1')
file = target / 'src' / 'Mamtzetza' / 'FireflyTransformer.cs'
text = file.read_text(encoding='utf-8-sig')
before = 'if (age < 35) return "C#";'
if text.count(before) != 1:
    raise SystemExit('Expected threshold not found exactly once; inspect source before seeding.')
file.write_text(text.replace(before, 'if (age < 34) return "C#";'), encoding='utf-8')
print(f'Prepared self-contained assessment at {lab}. Use its Run-Assessment.ps1 for both unit and E2E commands.')
