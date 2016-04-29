using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NightshiftLib {
    public class ImageDatabase {
        [JsonIgnore]
        readonly string dirPath;

        [JsonProperty]
        readonly string imgFormat;
        [JsonProperty]
        readonly int stepCount;
        [JsonProperty]
        readonly int dayHash;
        [JsonProperty]
        readonly int nightHash;

        public ImageDatabase(string newDirPath, string newImgFormat, int newStepCount,
            int newDayHash, int newNightHash) {
            dirPath = newDirPath;
            imgFormat = newImgFormat;
            stepCount = newStepCount;
            dayHash = newDayHash;
            nightHash = newNightHash;
        }

        public int GetWallpaperId(double nightIndex) {
            int step = (int)(nightIndex * stepCount);
            return (int) (step / (double) stepCount * 255);
        }

        public string GetImagePath(int wallpaperId) {
            return Path.Combine(dirPath, string.Format("{0:D3}.jpg", wallpaperId));
        }

        public bool SaveDatabase(string newDirPath) {
            var jsonPath = Path.Combine(newDirPath, "images.json");

            var savedDatabase = LoadDatabase(newDirPath);
            if (savedDatabase != null && savedDatabase.Equals(this)) {
                return true;
            }
            try {
                File.WriteAllText(jsonPath, JsonConvert.SerializeObject(this));
            }
            catch {
                return false;
            }
            return true;
        }

        public bool SaveDatabase() {
            return SaveDatabase(dirPath);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != GetType()) {
                return false;
            }
            return Equals((ImageDatabase) obj);
        }

       bool Equals(ImageDatabase other) {
            return string.Equals(imgFormat, other.imgFormat) && stepCount == other.stepCount && dayHash == other.dayHash && nightHash == other.nightHash;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = imgFormat?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ stepCount;
                hashCode = (hashCode * 397) ^ dayHash;
                hashCode = (hashCode * 397) ^ nightHash;
                return hashCode;
            }
        }

        public static bool DefaultDatabaseExists() {
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            if (localAppData == null) {
                return false;
            }
            return File.Exists(Path.Combine(localAppData, "Nightshift", "images.json"));
        }

        public static ImageDatabase LoadDefaultDatabase() {
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            if (localAppData == null) {
                return null;
            }
            return LoadDatabase(localAppData);
        }

        public static ImageDatabase LoadDatabase(string dirPath) {
            var jsonPath = Path.Combine(dirPath, "images.json");
            if (!File.Exists(jsonPath)) {
                return null;
            }
            JObject jObj = JObject.Parse(File.ReadAllText(jsonPath));
            return new ImageDatabase(dirPath, (string) jObj[nameof(imgFormat)], (int) jObj[nameof(stepCount)],
                (int) jObj[nameof(dayHash)], (int) jObj[nameof(nightHash)]);
        }
    }
}