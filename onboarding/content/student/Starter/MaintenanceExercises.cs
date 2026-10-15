using Mamtzetza;
using Xunit;

public class MaintenanceExercises
{
    [Fact]
    public void WorkingBaseline() => Assert.Equal("Rust", FireflyTransformer.CalculateFavoriteCodingLanguage(24));

    [Fact(Skip = "Student TODO A: replace with all eight literal age boundary cases")]
    public void AgeBoundaryCases() => throw new NotImplementedException();

    [Fact(Skip = "Student TODO B: fake HTTP failure with meaningful fallback, glow and time checks")]
    public void ApiFailureEvidence() => throw new NotImplementedException();

    [Fact(Skip = "Student TODO C: demonstrate a failing-before maintenance regression")]
    public void MaintenanceRegression() => throw new NotImplementedException();
}
