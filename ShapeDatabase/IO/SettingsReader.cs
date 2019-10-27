using System;
using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A reader to serialise Settings for this application.
	/// </summary>
	public class SettingsReader : IReader<TempSettings> {

		#region --- Properties ---

		/// <summary>
		/// Instantiates a new Reader to convert ini file settings to
		/// the application type settings.
		/// </summary>
		public static SettingsReader Instance => lazy.Value;

		private static readonly Lazy<SettingsReader> lazy
			= new Lazy<SettingsReader>();


		public ICollection<string> SupportedFormats => new string[] { "ini" };

		#endregion

		#region --- Constructor Methods ---

		private SettingsReader() { }

		#endregion

		#region --- Instance Methods ---

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
