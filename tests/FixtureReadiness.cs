using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OmegaFireflyTests.Assertions;

namespace OmegaFireflyTests;

public static class FixtureReadiness
{
    public static async Task PrepareAsync(bool docker)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        var broker = docker ? "rabbitmq" : "127.0.0.1";
        var service = docker ? "mamtzetza" : "127.0.0.1";
        var mocker = docker ? "mocker" : "127.0.0.1";
        var logPath = "evidence/mamtzetza.jsonl";
        Directory.CreateDirectory("evidence");
        Exception? last = null;
        while (!timeout.IsCancellationRequested)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{broker}:15672/api/queues/%2F/omega-solider-input-queue");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:admin")));
                using var response = await client.SendAsync(request, timeout.Token);
                response.EnsureSuccessStatusCode();
                using var queue = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
                if (queue.RootElement.GetProperty("consumers").GetInt32() < 1 || queue.RootElement.GetProperty("messages").GetInt32() != 0)
                    throw new InvalidDataException("Expected active consumer and empty input queue. Reset the isolated stack.");
                using var buff = await client.GetAsync($"http://{mocker}:8080/api/v1/buff/readiness", timeout.Token);
                buff.EnsureSuccessStatusCode();
                using var title = await client.PostAsync($"http://{mocker}:8080/api/v1/funny-title", new StringContent("{\"favoriteFood\":\"Shawarma\"}", Encoding.UTF8, "application/json"), timeout.Token);
                title.EnsureSuccessStatusCode();
                if (!File.Exists(logPath)) throw new IOException($"Missing real stdout capture: {logPath}");
                var content = await client.GetStringAsync($"http://{service}:9090/metrics", timeout.Token);
                var baseline = new EvidenceBaseline(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), EvidenceValidation.ParseMetrics(content));
                await File.WriteAllTextAsync("evidence/baseline.json", JsonSerializer.Serialize(baseline), timeout.Token);
                Console.WriteLine("Readiness verified; captured pre-batch metrics and start time.");
                return;
            }
            catch (Exception ex) when (!timeout.IsCancellationRequested)
            {
                last = ex;
                await Task.Delay(500, timeout.Token);
            }
        }
        throw new TimeoutException("Stack did not become ready within 120 seconds.", last);
    }
}
