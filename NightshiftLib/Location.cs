namespace NightshiftLib {
    public struct Location {
        public readonly double Latitude;
        public readonly double Longitude;

        public Location(double latitude, double longitude) {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}