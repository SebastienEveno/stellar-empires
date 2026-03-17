using StellarEmpires.Features.Planets.Services;

namespace StellarEmpires.Features.Mines.Services;

/// <summary>
/// Background service that triggers periodic resource production for all planets.
/// Runs production cycles at configured intervals.
/// </summary>
public class ResourcesBackgroundService : BackgroundService
{
    private readonly IPlanetProductionService _productionService;
    private readonly ILogger<ResourcesBackgroundService> _logger;

    // Production cycle duration - one cycle per minute (balanced for production)
    private readonly TimeSpan _productionCycleInterval = TimeSpan.FromMinutes(1);

    // Hours represented by each production cycle
    // For gameplay balance, 1 minute of real time = 1 hour of game time
    private const decimal GameHoursPerCycle = 1;

    public ResourcesBackgroundService(
        IPlanetProductionService productionService,
        ILogger<ResourcesBackgroundService> logger)
    {
        _productionService = productionService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Resources background service starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Starting production cycle.");

                // Trigger production for all planets
                await _productionService.ProduceForAllPlanetsAsync(GameHoursPerCycle);

                _logger.LogDebug("Production cycle completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during production cycle.");
            }

            // Wait until the next production cycle
            await Task.Delay(_productionCycleInterval, stoppingToken);
        }

        _logger.LogInformation("Resources background service stopped.");
    }
}