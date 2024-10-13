using StellarEmpires.Domain.Models;

namespace StellarEmpires.Services;

public interface IMinesService
{
    IReadOnlyList<Mine> GetMines();
    void IncrementMineLevel(ResourceType resourceType);
}