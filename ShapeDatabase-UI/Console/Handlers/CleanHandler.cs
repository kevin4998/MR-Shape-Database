using System;
using System.Collections.Generic;
using System.IO;
using ShapeDatabase.UI.Console.Verbs;
using ShapeDatabase.Util;
using static System.Console;
using static ShapeDatabase.UI.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle cleaning operations.
	/// </summary>
	public static class CleanHandler {

		/// <summary>
		/// All cached directories made by the application.
		/// </summary>
		public static ICollection<string> CachedDirs {
			get {
				return new string[] {
					Settings.ShapeTempDir,
					Settings.ShapeFailedDir,
					Settings.ShapeFinalDir,

					Settings.FeatureVectorDir,
					Settings.MeasurementsDir,
					Settings.QueryDir
				};
			}
		}

		/// <summary>
		/// All cached files made by the application.
		/// </summary>
		public static ICollection<string> CachedFiles {
			get {
				return new string[] { Settings.SettingsFile };
			}
		}


		/// <summary>
		/// The operation which will clean all the remnant files and maps from
		/// the device.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(CleanOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			WriteLine(I_StartProc_Clean);
			if (options.CleanSettings)
				CleanFiles();
			CleanDirectories();
			WriteLine(I_EndProc_Clean);
		}


		private static void CleanDirectories() {
			ICollection<string> dirs = CachedDirs;
			using (ProgressBar progress = new ProgressBar(dirs.Count)) { 
				foreach (string dir in dirs) {
					DirectoryInfo info = new DirectoryInfo(dir);
					if (info.Exists)
						info.Delete(true);
					progress.CompleteTask();
				}
			}
		}

		private static void CleanFiles() {
			ICollection<string> dirs = CachedFiles;
			using (ProgressBar progress = new ProgressBar(dirs.Count)) {
				foreach (string file in dirs) {
					FileInfo info = new FileInfo(file);
					if (info.Exists)
						info.Delete();
					progress.CompleteTask();
				}
			}
		}

	}
}
