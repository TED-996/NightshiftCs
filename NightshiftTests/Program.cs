using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NightshiftLib;

namespace NightshiftTests {
	class Program {
		static void Main(string[] args) {
			var location = new Location(47.15, 27.59);
			Console.WriteLine(SunriseSunsetCalculator.GetSunriseSunsetDateTime(location, new DateTime(2016, 3, 18)));
			var localZone = TimeZone.CurrentTimeZone;
			Console.WriteLine(localZone.IsDaylightSavingTime(DateTime.Now));
			Console.WriteLine(localZone.GetUtcOffset(DateTime.Now));
			Console.WriteLine(localZone.GetUtcOffset(new DateTime(2016, 1, 1)));
		}
	}
}
