namespace StellarEmpires.Features.Planets.Domain;

/// <summary>
/// Represents the available galaxies in the Stellar Empires universe.
/// </summary>
public enum Galaxy
{
    /// <summary>Unknown galaxy - used as default when galaxy has not been determined.</summary>
    Unknown = 0,

    /// <summary>The Andromeda galaxy with 3 systems and 30 planets.</summary>
    Andromeda = 1,

    /// <summary>The Milky Way galaxy with 2 systems and 50 planets.</summary>
    MilkyWay = 2
}
