using NodaTime;

namespace NightshiftLib {
    public struct SunriseSunset {
        public readonly LocalTime Sunrise;
        public readonly LocalTime Sunset;

        public SunriseSunset(LocalTime sunrise, LocalTime sunset) {
            Sunrise = sunrise;
            Sunset = sunset;
        }

        public override string ToString() {
            return $"{{ Sunrise: {Sunrise}; Sunset: {Sunset} }}";
        }
    }
}