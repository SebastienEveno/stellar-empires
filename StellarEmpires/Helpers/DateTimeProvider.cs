namespace StellarEmpires.Helpers;

public static class DateTimeProvider
{
	private static Func<DateTime> _utcNowFunc = () => DateTime.UtcNow;

	public static DateTime UtcNow => _utcNowFunc();

	public static void SetUtcNow(Func<DateTime> utcNowFunc)
	{
		_utcNowFunc = utcNowFunc ?? throw new ArgumentNullException(nameof(utcNowFunc));
	}

	public static void ResetUtcNow()
	{
		_utcNowFunc = () => DateTime.UtcNow;
	}
}
