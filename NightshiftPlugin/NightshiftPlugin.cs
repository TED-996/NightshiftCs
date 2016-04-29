// Uncomment these only if you want to export GetString() or ExecuteBang().
//#define DLLEXPORT_GETSTRING
//#define DLLEXPORT_EXECUTEBANG

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Rainmeter;


namespace NightshiftPlugin {
    internal class Measure {
        readonly dynamic ticker;

        internal Measure(dynamic newTicker) {
            ticker = newTicker;
        }

        internal void Reload(API api, ref double maxValue) {
        }

        internal double Update() {
            var wallpaperId = ticker.Update();
            API.Log(API.LogType.Debug, $"Ticker updated to ID {wallpaperId}.");
            return wallpaperId;
        }

#if DLLEXPORT_GETSTRING
        internal string GetString() {
            return "";
        }
#endif

#if DLLEXPORT_EXECUTEBANG
        internal void ExecuteBang(string args) {
        }
#endif
    }

    public static class Plugin {
        static string dllDir;

        static Type sunsetCalcType;
        static Type locationType;
        static Type generatorType;
        static Type tickerType;

        static ActionTextWriter writer;
        static TextWriter consoleOut;


#if DLLEXPORT_GETSTRING
        static IntPtr StringBuffer = IntPtr.Zero;
#endif

        // Plugin options:
        // DllFolder: Path
        // WallpaperDir: Path
        // DayWallpaper: Path
        // NightWallpaper: Path
        // StepCount: int
        // Latitude: Double
        // Longitude: Double

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm) {
            writer = new ActionTextWriter(str => API.Log(API.LogType.Debug, str));
            consoleOut = Console.Out;
            //Hook up Console.Out
            Console.SetOut(writer);

            try {
                var api = new API(rm);
                InitTypes(api);

                
                // var location = new Location(api.ReadDouble("Latitude", 0.0), api.ReadDouble("Longitude", 0.0));
                dynamic location = Activator.CreateInstance(
                    locationType,
                    new object[] {
                        api.ReadDouble("Latitude", 0.0),
                        api.ReadDouble("Longitude", 0.0)
                    });

                try {
                    //var sunriseSunset = SunriseSunsetCalculator.GetSunriseSunsetDateTime(location, DateTime.Today);
                    dynamic sunriseSunset = sunsetCalcType.InvokeMember(
                        "GetSunriseSunsetDateTime",
                        BindingFlags.InvokeMethod,
                        null,
                        null,
                        new object[] {
                            location, DateTime.Today
                        });
                    API.Log(API.LogType.Debug, "Trying to generate image database.");

                    var wallpaperDir = api.ReadPath("DbPath", null);
                    var dayWallpaper = api.ReadPath("DayImage", null);
                    var nightWallpaper = api.ReadPath("NightImage", null);
                    var stepCount = api.ReadInt("StepCount", 40);

                    if (!Directory.Exists(wallpaperDir)) {
                        API.Log(API.LogType.Warning, "Wallpaper directory not found.");
                        return;
                    }
                    if (!File.Exists(dayWallpaper)) {
                        API.Log(API.LogType.Warning, "Day wallpaper not found.");
                        return;
                    }
                    if (!File.Exists(nightWallpaper)) {
                        API.Log(API.LogType.Warning, "Night wallpaper not found.");
                        return;
                    }

                    dynamic database = generatorType.InvokeMember(
                        "GenerateLoadDatabase",
                        BindingFlags.InvokeMethod,
                        null,
                        null,
                        new object[] {
                            wallpaperDir,
                            dayWallpaper,
                            nightWallpaper,
                            stepCount
                        });
                    if (database == null) {
                        API.Log(API.LogType.Error,
                            "Could not initialize image database. Check your parameters and try again.");
                        return;
                    }
                    var ticker = Activator.CreateInstance(
                        tickerType,
                        new object[] {
                            database,
                            sunriseSunset
                        });
                    data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure(ticker)));
                    API.Log(API.LogType.Debug, "Nightshift plugin initialized correctly.\n" +
                                                $"Sunrise/Sunset at {sunriseSunset}");
                }
                catch (Exception ex) {
                    API.Log(API.LogType.Error,
                        "Could not initialize Nightshift. Check your parameters and try again.\n" +
                        "Error information: " + ex);
                }
            }
            catch (Exception ex) {
                API.Log(API.LogType.Error, "Catastrophic error initializing Nightshift. Exception: " + ex);
            }
        }

        static void InitTypes(API rmApi) {
            // let it throw, let it throw, let it throw
            dllDir = rmApi.ReadPath("DllFolder", "");
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyDependsResolve;
            try {
                var dll = Assembly.LoadFile(Path.Combine(dllDir, "NightshiftLib.dll"));

                sunsetCalcType = dll.GetType("NightshiftLib.SunriseSunsetCalculator", true);
                locationType = dll.GetType("NightshiftLib.Location", true);
                generatorType = dll.GetType("NightshiftLib.ImageDatabaseGenerator", true);
                tickerType = dll.GetType("NightshiftLib.NightshiftTicker", true);
            }
            catch (Exception ex) {
                API.Log(API.LogType.Error, $"Could not find DLL or its types. Exception: " + ex);
                throw;
            }
        }

        static Assembly AssemblyDependsResolve(object sender, ResolveEventArgs e) {
            string dllName = e.Name.Split(',')[0] + ".dll";
            string dllPath = Path.Combine(dllDir, dllName);
            if (!File.Exists(dllPath)) {
                return null;
            }
            return Assembly.LoadFrom(dllPath);
        }

        [DllExport]
        public static void Finalize(IntPtr data) {
            GCHandle.FromIntPtr(data).Free();

            Console.SetOut(consoleOut);
            writer.Dispose();
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyDependsResolve;

#if DLLEXPORT_GETSTRING
            if (StringBuffer != IntPtr.Zero) {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }
#endif
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue) {
            Measure measure = (Measure) GCHandle.FromIntPtr(data).Target;
            measure.Reload(new API(rm), ref maxValue);
        }

        [DllExport]
        public static double Update(IntPtr data) {
            Console.SetOut(writer);
            Measure measure = (Measure) GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }

#if DLLEXPORT_GETSTRING
        [DllExport]
        public static IntPtr GetString(IntPtr data) {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            if (StringBuffer != IntPtr.Zero) {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }

            string stringValue = measure.GetString();
            if (stringValue != null) {
                StringBuffer = Marshal.StringToHGlobalUni(stringValue);
            }

            return StringBuffer;
        }
#endif

#if DLLEXPORT_EXECUTEBANG
        [DllExport]
        public static void ExecuteBang(IntPtr data, IntPtr args) {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.ExecuteBang(Marshal.PtrToStringUni(args));
        }
#endif
    }
}
