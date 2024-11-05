using StellarEmpires.Helpers;

namespace StellarEmpires.Tests.Helpers;

[TestFixture]
public class DateTimeProviderTests
{
	[SetUp]
	public void SetUp()
	{
		// Ensure that DateTimeProvider is reset to its default behavior before each test
		DateTimeProvider.ResetUtcNow();
	}

	[Test]
	public void UtcNow_ShouldReturnCurrentUtcTime()
	{
		// Arrange
		var expectedUtcNow = DateTime.UtcNow;

		// Act
		var actualUtcNow = DateTimeProvider.UtcNow;

		// Assert
		Assert.That(actualUtcNow, Is.EqualTo(expectedUtcNow).Within(TimeSpan.FromSeconds(1)),
			"UtcNow should return the current UTC time");
	}

	[Test]
	public void SetUtcNow_ShouldChangeUtcNowToCustomValue()
	{
		// Arrange
		var customDateTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => customDateTime);

		// Act
		var actualUtcNow = DateTimeProvider.UtcNow;

		// Assert
		Assert.That(actualUtcNow, Is.EqualTo(customDateTime),
			"UtcNow should return the custom date time after setting it");
	}

	[Test]
	public void ResetUtcNow_ShouldResetToDefaultUtcNowBehavior()
	{
		// Arrange
		var customDateTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => customDateTime);

		// Act
		DateTimeProvider.ResetUtcNow();
		var actualUtcNow = DateTimeProvider.UtcNow;
		var expectedUtcNow = DateTime.UtcNow;

		// Assert
		Assert.That(actualUtcNow, Is.EqualTo(expectedUtcNow).Within(TimeSpan.FromSeconds(1)),
			"UtcNow should return the current UTC time after reset");
	}

	[Test]
	public void SetUtcNow_ShouldThrowArgumentNullException_WhenNullIsPassed()
	{
		// Act & Assert
		Assert.That(() => DateTimeProvider.SetUtcNow(null),
			Throws.ArgumentNullException.With.Property("ParamName").EqualTo("utcNowFunc"),
			"SetUtcNow should throw ArgumentNullException when null is passed");
	}
}
