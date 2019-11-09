using System;
using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;

namespace ShapeDatabase.IO {

	/// <summary>
	/// Class for writing settings from this application to an ini file.
	/// </summary>
	class SettingsWriter : IWriter<TempSettings> {

		#region --- Properties ---

		/// <summary>
		/// Initialises a new writer to serialise the application settings to an ini file.
		/// </summary>
		public static SettingsWriter Instance => lazy.Value;

		private static readonly Lazy<SettingsWriter> lazy
			= new Lazy<SettingsWriter>(() => new SettingsWriter());

		/// <summary>
		/// Collection containing the supported formats.
		/// </summary>
		public ICollection<string> SupportedFormats => new string[] { "ini" };

		#endregion

		#region --- Constructor Methods ---

		private SettingsWriter() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Writes settings to a file, given a streamwriter.
		/// </summary>
		/// <param name="type">The settings.</param>
		/// <param name="writer">The streamwriter.</param>
		public void WriteFile(TempSettings type, StreamWriter writer) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			FileIniDataParser parser = new FileIniDataParser();
			IniData data = new IniData();

			// Weight values for Descriptors
			foreach ((string name, double value) in type.Weights)
				data[nameof(type.Weights)][name] = value.ToString(Settings.Culture);
			// Variables and counts in Settings
			foreach ((string name, int value) in type.Variables)
				data[nameof(type.Variables)][name] = value.ToString(Settings.Culture);
			// Weight values for Descriptors
			foreach ((string name, bool value) in type.Flow)
				data[nameof(type.Flow)][name] = value.ToString(Settings.Culture);

			parser.WriteData(writer, data);
		}

		/// <summary>
		/// Writes settings to a file, given a streamwriter.
		/// </summary>
		/// <param name="type">The settings.</param>
		/// <param name="writer">The streamwriter.</param>
		public void WriteFile(object type, StreamWriter writer)
			=> WriteFile(type as TempSettings, writer);

		#endregion
	}
}
