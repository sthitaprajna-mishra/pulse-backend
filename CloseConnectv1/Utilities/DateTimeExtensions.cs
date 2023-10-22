namespace CloseConnectv1.Utilities
{
    public static class DateTimeExtensions
    {
        public static DateTime ToLocalTime(this DateTime utcDateTime)
        {
            TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, localTimeZone);
            return localDateTime;
        }
    }
}
