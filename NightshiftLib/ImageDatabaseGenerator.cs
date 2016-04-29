using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AForge.Imaging.Filters;

namespace NightshiftLib {
    public class ImageDatabaseGenerator {
        readonly string dirPath;
        readonly string dayImagePath;
        readonly string nightImagePath;
        readonly int stepCount;

        readonly ImageDatabase resultDatabase;

        ImageDatabaseGenerator(string newDirPath, string newDayPath, string newNightPath, int newStepCount) {
            dirPath = newDirPath;
            dayImagePath = newDayPath;
            nightImagePath = newNightPath;
            stepCount = newStepCount;

            var dayHash = GetFileHash(dayImagePath);
            var nightHash = GetFileHash(nightImagePath);
            resultDatabase = new ImageDatabase(dirPath, ".jpg", stepCount, dayHash, nightHash);
        }

        static int GetFileHash(string path) {
            //If anything gets thrown, duck and let someone else catch it.
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(path)) {
                    return BitConverter.ToInt32(md5.ComputeHash(stream), 0);
                }
            }
        }

        bool DatabaseExisting() {
            var existingDb = ImageDatabase.LoadDatabase(dirPath);
            return existingDb != null && Equals(existingDb, resultDatabase);
        }

        bool GenerateDatabase() {
            if (!File.Exists(dayImagePath) || !File.Exists(nightImagePath)) {
                return false;
            }
            try {
                CleanupOldDatabase();

                var dayImageOrig = (Bitmap) Image.FromFile(dayImagePath);
                var nightImageOrig = (Bitmap) Image.FromFile(nightImagePath);

                var unit = GraphicsUnit.Pixel;
                var bounds = dayImageOrig.GetBounds(ref unit);
                if (bounds != nightImageOrig.GetBounds(ref unit)) {
                    return false;
                }

                var dayImage = dayImageOrig.Clone(bounds, PixelFormat.Format24bppRgb);
                var nightImage = nightImageOrig.Clone(bounds, PixelFormat.Format24bppRgb);

                var encoder = GetEncoder(ImageFormat.Jpeg);
                var parameters = new EncoderParameters(1) {
                    Param = {
                        [0] = new EncoderParameter(Encoder.Quality, 95L)
                    }
                };

                if (!Directory.Exists(dirPath)) {
                    Directory.CreateDirectory(dirPath);
                }

                for (int step = 0; step <= stepCount; step++) {
                    double blendFactor = step / (double) stepCount;
                    var blendedResult = BlendImages(dayImage, nightImage, blendFactor);

                    string resultPath = Path.Combine(dirPath,
                        string.Format("{0:D3}.jpg", (int) (blendFactor * 255)));


                    blendedResult.Save(resultPath, encoder, parameters);
                }
                return true;
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
                return false;
            }
        }

        void CleanupOldDatabase() {
            if (!Directory.Exists(dirPath)) {
                return;
            }
            foreach
                (var filename in Directory.EnumerateFiles(dirPath).Where(filename => filename.EndsWith(".jpg"))) {
                File.Delete(Path.Combine(dirPath, filename));
            }
        }

        ImageCodecInfo GetEncoder(ImageFormat format) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        Bitmap BlendImages(Bitmap dayImage, Bitmap nightImage, double blendFactor) {
            if (Math.Abs(blendFactor) < 0.00001) {
                return (Bitmap) dayImage.Clone();
            }
            if (Math.Abs(blendFactor - 1.0) < 0.00001) {
                return (Bitmap) nightImage.Clone();
            }

            var filter = new Morph(nightImage) {
                SourcePercent = 1.0 - blendFactor,
            };
            return filter.Apply(dayImage);
        }


        public static ImageDatabase GenerateLoadDatabase(
            string dirPath,
            string dayPath,
            string nightPath,
            int stepCount) {

            try {
                var generator = new ImageDatabaseGenerator(dirPath, dayPath, nightPath, stepCount);
                if (generator.DatabaseExisting()) {
                    return generator.resultDatabase;
                }
                if (!generator.GenerateDatabase()) {
                    return null;
                }
                generator.resultDatabase.SaveDatabase();
                return generator.resultDatabase;
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}