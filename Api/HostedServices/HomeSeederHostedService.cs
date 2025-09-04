using Microsoft.Extensions.Logging;
using System.Text.Json;
using Api.Models;
using Api.Repositories;

namespace Api.HostedServices
{
    public class HomeSeederHostedService : IHostedService
    {
        private readonly IServiceProvider _sp;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HomeSeederHostedService> _log;

        public HomeSeederHostedService(IServiceProvider sp, IWebHostEnvironment env,
            ILogger<HomeSeederHostedService> log)
        {
            _sp = sp;
            _env = env;
            _log = log;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            string webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            string filePath = Path.Combine(webRoot, "homes.json");

            try
            {
                if (!File.Exists(filePath))
                {
                    _log.LogWarning("homes.json was not found at path: {Path}", filePath);
                    return;
                }

                var json = await File.ReadAllTextAsync(filePath, ct);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _log.LogWarning("homes.json is empty: {Path}", filePath);
                    return;
                }

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var homes = JsonSerializer.Deserialize<List<Home>>(json, opts) ?? new();

                if (homes.Count == 0)
                {
                    _log.LogWarning("homes.json contained zero valid items. Path: {Path}", filePath);
                    return;
                }

                using var scope = _sp.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IHomeRepository>();

                foreach (var h in homes)
                    await repo.UpsertAsync(h);

                _log.LogInformation("Loaded {Count} home(s) from homes.json into memory.", homes.Count);
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning("Seeding homes.json was cancelled.");
                throw;
            }
            catch (JsonException jex)
            {
                _log.LogError(jex, "Failed to parse homes.json. Ensure it has a valid JSON schema. Path: {Path}", filePath);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An unexpected error occurred while seeding homes.json. Path: {Path}", filePath);
            }
        }


        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}