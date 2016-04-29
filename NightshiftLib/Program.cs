using System;
using NightshiftLib;
using NodaTime;

namespace NightshiftCs {
	class Program {
		static void Main(string[] args) {
		    var result = ImageDatabaseGenerator.GenerateLoadDatabase(
		        @"E:\Development\NightshiftTest",
		        @"C:\Users\TED\Pictures\wallpapers\mountain_lp_edited.jpg",
		        @"C:\Users\TED\Pictures\wallpapers\mountain_lp_night.jpg",
		        40);
            Console.WriteLine("Out: " + (result?.ToString() ?? "null"));
		}
	}
}
