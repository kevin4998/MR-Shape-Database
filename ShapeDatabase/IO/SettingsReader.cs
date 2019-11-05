using System;
using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A reader for serialised settings for this application.
	/// </summary>
	class SettingsReader : IReader<TempSettings> {

		#region --- Properties ---

		/// <summary>
		/// Instantiates a new Reader to convert ini file settings to
		/// the application type settings.
		/// </summary>
		public static SettingsReader Instance => lazy.Value;

		private static readonly Lazy<SettingsReader> lazy
			= new Lazy<SettingsReader>(() => new SettingsReader());

		/// <summary>
		/// A collection containing the supported formats.
		/// </summary>
		public ICollection<string> SupportedFormats => new string[] { "ini" };

		#endregion

		#region --- Constructor Methods ---

		private SettingsReader() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Reads the settings file given a streamreader.
		/// </summary>
		/// <param name="reader">The streamreader.</param>
		/// <returns>The settings.</returns>
		public TempSettings ConvertFile(StreamReader reader) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			FileIniDataParser parser = new FileIniDataParser();
			IniData data = parser.ReadData(reader);

			TempSettings settings = new TempSettings();

			// Weight values for Descriptors
			foreach (KeyData keyData in data[nameof(settings.Weights)])
				if (double.TryParse(keyData.Value, out double weight))
					settings.AddWeight(keyData.KeyName, weight);
			// Variables and counts in Settings
			foreach (KeyData keyData in data[nameof(settings.Variables)])
				if (int.TryParse(keyData.Value, out int variable))
					settings.AddVariable(keyData.KeyName, variable);
			// Weight values for Descriptors
			foreach (KeyData keyData in data[nameof(settings.Flow)])
				if (bool.TryParse(keyData.Value, out bool flow))
					settings.AddFlow(keyData.KeyName, flow);

			return settings;
		}

		object IReader.ConvertFile(StreamReader reader) => ConvertFile(reader);

		#endregion
	}
}
