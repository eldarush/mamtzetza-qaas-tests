if (args.Length == 2 && args[0] == "--isolated")
{
    await OmegaFireflyTests.FixtureReadiness.PrepareAsync(args[1].Contains("docker", StringComparison.OrdinalIgnoreCase));
    args = ["run", args[1]];
}
QaaS.Runner.Bootstrap.New(args).Run();
