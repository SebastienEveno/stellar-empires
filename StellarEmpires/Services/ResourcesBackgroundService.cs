namespace StellarEmpires.Services;

public class ResourcesBackgroundService : BackgroundService
{
    private readonly IResourcesService _resourcesService;
    private readonly IMinesService _minesService;

    public ResourcesBackgroundService(IResourcesService resourcesService, IMinesService minesService)
    {
        _resourcesService = resourcesService;
        _minesService = minesService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _resourcesService.IncreaseResources(_minesService.GetMines());

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}