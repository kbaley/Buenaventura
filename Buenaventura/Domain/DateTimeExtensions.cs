namespace Buenaventura.Domain;

public static class DateTimeExtensions {
    public static DateTime LastDayOfMonth(this DateTime date) {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59);
    }

    public static DateTime FirstDayOfMonth(this DateTime date) {
        return new DateTime(date.Year, date.Month, 1, 0, 0, 0);
    }
}