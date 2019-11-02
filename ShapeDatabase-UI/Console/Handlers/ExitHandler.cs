using System;
using ShapeDatabase.UI.Console.Verbs;

namespace ShapeDatabase.UI.Console.Handlers {
	/// <summary>
	/// The class which should handle operrations to finish or close the application.
	/// </summary>
	public static class ExitHandler {

		/// <summary>
		/// The operation to close the application by unloading extra resources
		/// and saving memory data.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(BaseOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			// Write out the settings file.
			string settingsFile = Settings.SettingsFile;
			TempSettings settings = new TempSettings();
			settings.Initialise();
			Settings.FileManager.WriteObject(settings, settingsFile);
		}

	}
}
