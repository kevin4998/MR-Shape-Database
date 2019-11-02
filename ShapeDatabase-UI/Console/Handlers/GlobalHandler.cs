using System;
using System.Globalization;
using System.IO;
using ShapeDatabase.UI.Console.Verbs;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle basic settings operations for all possible Options.
	/// </summary>
	public static class GlobalHandler {

		/// <summary>
		/// The operation to load in initial settings for this application.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(BaseOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			// Convert Options value to Settings.
			if (options.HasCulture)
				Settings.Culture = CultureInfo.GetCultureInfo(options.Culture);
			Settings.ShowDebug = options.DebugMessages;
			Settings.DirectShutDown = options.AutoExit;
			// Read the Settings.
			string settingsFile = Settings.SettingsFile;
			if (Settings.FileManager.TryRead(settingsFile, out TempSettings settings))
				settings.Finalise();
			// Add previously refined files.
			string finalDir = Settings.ShapeFinalDir;
			if (!Directory.Exists(finalDir))
				Directory.CreateDirectory(finalDir);
			Settings.FileManager.AddDirectoryDirect(Settings.ShapeFinalDir);
		}

	}

}
