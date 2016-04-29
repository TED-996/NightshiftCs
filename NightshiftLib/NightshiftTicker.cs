using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using NodaTime;
using NodaTime.Extensions;
using static System.Math;

namespace NightshiftLib
{
    public class NightshiftTicker {
        SunriseSunset sunriseSunset;
        ImageDatabase imageDatabase;
        int lastId;

        public NightshiftTicker(ImageDatabase newImageDatabase, SunriseSunset newSunriseSunset) {
            imageDatabase = newImageDatabase;
            sunriseSunset = newSunriseSunset;

            lastId = -1;
        }

        public int Update() {
            LocalTime time =
                SystemClock.Instance.InZone(DateTimeZoneProviders.Bcl.GetSystemDefault()).GetCurrentTimeOfDay();
            return Tick(time);
        }

        int Tick(LocalTime time) {
            var nightIndex = GetNightIndex(time, sunriseSunset);
            var wallpaperId = imageDatabase.GetWallpaperId(nightIndex);
            if (wallpaperId != lastId) {
                var imagePath = imageDatabase.GetImagePath(wallpaperId);
                SetWallpaper(imagePath);
                lastId = wallpaperId;
            }
            return wallpaperId;
        }

        static double GetNightIndex(LocalTime time, SunriseSunset sunriseSunset) {
            var transitionInterval = 1.0;
            var timeHours = TimeToHours(time);
            var sunriseHours = TimeToHours(sunriseSunset.Sunrise);
            var sunsetHours = TimeToHours(sunriseSunset.Sunset);

            var sunriseDelta = sunriseHours - timeHours;
            var sunsetDelta = timeHours - sunsetHours;
            var minAbsDelta = Min(Abs(sunriseDelta), Abs(sunsetDelta));

            if (sunriseHours < sunsetHours) {
                if (sunriseDelta >= 0 || sunsetDelta >= 0) {
                    var minDelta = minAbsDelta;
                    if (minDelta >= transitionInterval / 2) {
                        return 1;
                    }
                    return (minDelta + transitionInterval / 2) / transitionInterval;
                }
                else {
                    var maxDelta = -minAbsDelta;
                    if (maxDelta <= -transitionInterval / 2) {
                        return 0;
                    }
                    return (transitionInterval / 2 + maxDelta) / transitionInterval;
                }
            }
            else {
                if (sunriseDelta >= 0 && sunsetDelta >= 0) {
                    var minDelta = minAbsDelta;
                    if (minDelta >= transitionInterval / 2) {
                        return 1;
                    }
                    return (minDelta + transitionInterval / 2) / transitionInterval;
                }
                else {
                    var maxDelta = -minAbsDelta;
                    if (maxDelta <= -transitionInterval / 2) {
                        return 0;
                    }
                    return (transitionInterval / 2 + maxDelta) / transitionInterval;
                }
            }
        }

        static double TimeToHours(LocalTime time) {
            return time.Hour + time.Minute / 60.0 + time.Second / 3600.0;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        void SetWallpaper(string wallpaperPath) {
            const int SPI_SETDESKWALLPAPER = 20;
            const int SPIF_UPDATEINIFILE = 0x01;
            const int SPIF_SENDWININICHANGE = 0x02;

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                wallpaperPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
