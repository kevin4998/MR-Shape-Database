using System;
using System.Collections.Generic;
using System.IO;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle cleaning operations.
	/// </summary>
	public static class CleanHandler {

		/// <summary>
		/// All cached directories made by the application.
		/// </summary>
		public static IEnumerable<string> CachedDirs {
			get {
				yield return Settings.ShapeTempDir;
				yield return Settings.ShapeFailedDir;
				yield return Settings.ShapeFinalDir;

				yield return Settings.FeatureVectorDir;
				yield return Settings.MeasurementsDir;
				yield return Settings.QueryDir;
			}
		}

		/// <summary>
		/// All cached files made by the application.
		/// </summary>
		public static IEnumerable<string> CachedFiles {
			get {
				yield return Settings.SettingsFile;
			}
		}


		/// <summary>
		/// The operation which will clean all the remnant files and maps from
		/// the device.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(CleanOptions options) {
			WriteLine(I_StartClean);
			CleanFiles();
			CleanDirectories();
			WriteLine(I_EndClean);
		}


		private static void CleanDirectories() {
			foreach (string dir in CachedDirs) {
				DirectoryInfo info = new DirectoryInfo(dir);
				if (info.Exists)
					info.Delete(true);
			}
		}

		private static void CleanFiles() {
			foreach (string file in CachedFiles) {
				FileInfo info = new FileInfo(file);
				if (info.Exists)
					info.Delete();
			}
		}

	}
}
