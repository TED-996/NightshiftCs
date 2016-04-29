using System;
using NodaTime;

namespace NightshiftLib {
    /// <summary>
    /// Adapted from http://yaddb.blogspot.com/2013/01/how-to-calculate-sunrise-and-sunset.html
    /// </summary>
    public static class SunriseSunsetCalculator {
        public static SunriseSunset GetSunriseSunsetDateTime(Location location, DateTime date) {
            return GetSunriseSunset(location, new LocalDate(date.Year, date.Month, date.Day));
        }

        public static SunriseSunset GetSunriseSunset(Location location, LocalDate localDate) {
            DateTime today = new DateTime(localDate.Year, localDate.Month, localDate.Day);
            var julianDay = SunriseSunsetUtil.calcJD(today);
            var sunriseDouble = SunriseSunsetUtil.calcSunRiseUTC(julianDay, location.Latitude, location.Longitude);
            var sunsetDouble = SunriseSunsetUtil.calcSunSetUTC(julianDay, location.Latitude, location.Longitude);

            TimeZone localZone = TimeZone.CurrentTimeZone;
            int utcOffset = localZone.GetUtcOffset(today).Hours;
            DateTime? sunriseDt = SunriseSunsetUtil.getDateTime(sunriseDouble, utcOffset, today, false);
            DateTime? sunsetDt = SunriseSunsetUtil.getDateTime(sunsetDouble, utcOffset, today, false);

            if (sunriseDt == null || sunsetDt == null) {
                throw new ApplicationException("Could not get sunset time.");
            }

            return new SunriseSunset(ToLocalTime(sunriseDt.Value), ToLocalTime(sunsetDt.Value));
        }

        static LocalTime ToLocalTime(DateTime dateTime) {
            return new LocalTime(dateTime.Hour, dateTime.Minute, dateTime.Second);
        }
    }
}