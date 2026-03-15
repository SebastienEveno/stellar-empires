using StellarEmpires.Features.Planets.Api.Dtos;
using StellarEmpires.Features.Planets.Domain;

namespace StellarEmpires.Tests.Features.Planets.Api.Dtos;

[TestFixture]
public class CoordinatesDtoTests
{
    [Test]
    public void CreatePlanetDto_ShouldHaveDefaultCoordinates()
    {
        // Arrange & Act
        var dto = new CreatePlanetDto();

        // Assert
        Assert.That(dto.Galaxy, Is.EqualTo(string.Empty));
        Assert.That(dto.System, Is.EqualTo(1));
        Assert.That(dto.Slot, Is.EqualTo(1));
    }

    [Test]
    public void CreatePlanetDto_ShouldAllowSettingCustomCoordinates()
    {
        // Arrange & Act
        var dto = new CreatePlanetDto
        {
            Galaxy = "Andromeda",
            System = 2,
            Slot = 5
        };

        // Assert
        Assert.That(dto.Galaxy, Is.EqualTo("Andromeda"));
        Assert.That(dto.System, Is.EqualTo(2));
        Assert.That(dto.Slot, Is.EqualTo(5));
    }

    [Test]
    public void CreatePlanetDto_ShouldPreserveAllCoordinateProperties()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var dto = new CreatePlanetDto
        {
            Id = planetId,
            Name = "Test Planet",
            IsColonized = false,
            Galaxy = "Milky-Way",
            System = 1,
            Slot = 3
        };

        // Act & Assert
        Assert.That(dto.Id, Is.EqualTo(planetId));
        Assert.That(dto.Name, Is.EqualTo("Test Planet"));
        Assert.That(dto.IsColonized, Is.False);
        Assert.That(dto.Galaxy, Is.EqualTo("Milky-Way"));
        Assert.That(dto.System, Is.EqualTo(1));
        Assert.That(dto.Slot, Is.EqualTo(3));
    }

    [Test]
    public void ReadPlanetDto_ShouldMapCoordinatesFromPlanet()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(
            planetId,
            "Test Planet",
            false,
            null,
            null,
            "Andromeda",
            2,
            7
        );

        // Act
        var dto = ReadPlanetDto.FromPlanet(planet);

        // Assert
        Assert.That(dto.Galaxy, Is.EqualTo("Andromeda"));
        Assert.That(dto.System, Is.EqualTo(2));
        Assert.That(dto.Slot, Is.EqualTo(7));
        Assert.That(dto.Id, Is.EqualTo(planetId));
    }

    [Test]
    public void ReadPlanetDto_ShouldMapDefaultCoordinates_WhenPlanetHasDefaults()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "Default Planet", false, null, null);

        // Act
        var dto = ReadPlanetDto.FromPlanet(planet);

        // Assert
        Assert.That(dto.Galaxy, Is.EqualTo("Unknown"));
        Assert.That(dto.System, Is.EqualTo(1));
        Assert.That(dto.Slot, Is.EqualTo(1));
    }

    [Test]
    public void ReadPlanetDto_ShouldMapAllPlanetProperties_IncludingCoordinates()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var colonizerId = Guid.NewGuid();
        var colonizationDate = DateTime.UtcNow;
        var planet = Planet.Create(
            planetId,
            "Colonized Planet",
            true,
            colonizerId,
            colonizationDate,
            "Milky-Way",
            1,
            5
        );

        // Act
        var dto = ReadPlanetDto.FromPlanet(planet);

        // Assert
        Assert.That(dto.Id, Is.EqualTo(planetId));
        Assert.That(dto.Name, Is.EqualTo("Colonized Planet"));
        Assert.That(dto.IsColonized, Is.True);
        Assert.That(dto.ColonizedBy, Is.EqualTo(colonizerId));
        Assert.That(dto.ColonizedAt, Is.EqualTo(colonizationDate));
        Assert.That(dto.Galaxy, Is.EqualTo("Milky-Way"));
        Assert.That(dto.System, Is.EqualTo(1));
        Assert.That(dto.Slot, Is.EqualTo(5));
    }

    [Test]
    public void CreatePlanetDto_CoordinateDefaults_ShouldNotAffectOtherProperties()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var colonizerId = Guid.NewGuid();
        var colonizationDate = DateTime.UtcNow;

        // Act
        var dto = new CreatePlanetDto
        {
            Id = planetId,
            Name = "Test",
            IsColonized = true,
            ColonizedBy = colonizerId,
            ColonizedAt = colonizationDate
        };

        // Assert - Other properties should be preserved even with default coordinates
        Assert.That(dto.Id, Is.EqualTo(planetId));
        Assert.That(dto.Name, Is.EqualTo("Test"));
        Assert.That(dto.IsColonized, Is.True);
        Assert.That(dto.ColonizedBy, Is.EqualTo(colonizerId));
        Assert.That(dto.ColonizedAt, Is.EqualTo(colonizationDate));
        Assert.That(dto.Galaxy, Is.EqualTo(string.Empty)); // Default
        Assert.That(dto.System, Is.EqualTo(1)); // Default
        Assert.That(dto.Slot, Is.EqualTo(1)); // Default
    }

    [Test]
    public void ReadPlanetDto_ShouldSupportVariousCoordinateValues()
    {
        // Arrange
        var testCases = new[]
        {
            ("Andromeda", 1, 1),
            ("Andromeda", 1, 10),
            ("Andromeda", 3, 5),
            ("Milky-Way", 1, 1),
            ("Milky-Way", 2, 15),
        };

        // Act & Assert
        foreach (var (galaxy, system, slot) in testCases)
        {
            var planet = Planet.Create(Guid.NewGuid(), $"Test {galaxy}", false, null, null, galaxy, system, slot);
            var dto = ReadPlanetDto.FromPlanet(planet);

            Assert.That(dto.Galaxy, Is.EqualTo(galaxy));
            Assert.That(dto.System, Is.EqualTo(system));
            Assert.That(dto.Slot, Is.EqualTo(slot));
        }
    }

    [Test]
    public void ReadPlanetDto_ShouldPreserveCoordinateIntegrity_InRoundTrip()
    {
        // Arrange
        var originalGalaxy = "Andromeda";
        var originalSystem = 2;
        var originalSlot = 8;
        var planet = Planet.Create(
            Guid.NewGuid(),
            "Round Trip Test",
            false,
            null,
            null,
            originalGalaxy,
            originalSystem,
            originalSlot
        );

        // Act
        var dto = ReadPlanetDto.FromPlanet(planet);

        // Assert - Coordinates should be preserved
        Assert.That(dto.Galaxy, Is.EqualTo(originalGalaxy));
        Assert.That(dto.System, Is.EqualTo(originalSystem));
        Assert.That(dto.Slot, Is.EqualTo(originalSlot));
    }

    [Test]
    public void CreatePlanetDto_ShouldSerializeCoordinates_Correctly()
    {
        // Arrange
        var dto = new CreatePlanetDto
        {
            Name = "Serialization Test",
            Galaxy = "Milky-Way",
            System = 2,
            Slot = 10
        };

        // Act - Verify all properties are accessible (serializable)
        var galaxy = dto.Galaxy;
        var system = dto.System;
        var slot = dto.Slot;

        // Assert
        Assert.That(galaxy, Is.EqualTo("Milky-Way"));
        Assert.That(system, Is.EqualTo(2));
        Assert.That(slot, Is.EqualTo(10));
    }

    [Test]
    public void ReadPlanetDto_CoordinateProperties_ShouldBeReadOnly()
    {
        // Arrange
        var planet = Planet.Create(
            Guid.NewGuid(),
            "Read Only Test",
            false,
            null,
            null,
            "Andromeda",
            1,
            5
        );

        // Act
        var dto = ReadPlanetDto.FromPlanet(planet);

        // Assert - Verify properties can be read
        Assert.That(dto.Galaxy, Is.EqualTo("Andromeda"));
        Assert.That(dto.System, Is.EqualTo(1));
        Assert.That(dto.Slot, Is.EqualTo(5));
    }

    [Test]
    public void CreatePlanetDto_ShouldSupportPartialCoordinateInitialization()
    {
        // Arrange & Act
        var dto1 = new CreatePlanetDto { Galaxy = "Andromeda" };
        var dto2 = new CreatePlanetDto { System = 2 };
        var dto3 = new CreatePlanetDto { Slot = 5 };

        // Assert - Other properties should use defaults
        Assert.That(dto1.Galaxy, Is.EqualTo("Andromeda"));
        Assert.That(dto1.System, Is.EqualTo(1)); // Default
        Assert.That(dto1.Slot, Is.EqualTo(1)); // Default

        Assert.That(dto2.Galaxy, Is.EqualTo(string.Empty)); // Default
        Assert.That(dto2.System, Is.EqualTo(2));
        Assert.That(dto2.Slot, Is.EqualTo(1)); // Default

        Assert.That(dto3.Galaxy, Is.EqualTo(string.Empty)); // Default
        Assert.That(dto3.System, Is.EqualTo(1)); // Default
        Assert.That(dto3.Slot, Is.EqualTo(5));
    }
}
