﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using ShapeDatabase.Features;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.IO;
using ShapeDatabase.Properties;

using static ShapeDatabase.IO.IOConventions;

namespace ShapeDatabase.IO {

	/// <summary>
	/// Class for creating a featuremanager out of a csv with featurevectors.
	/// </summary>
	class FMReader : IReader<FeatureManager> {

		#region --- Properties ---

		#region -- Static Properties --

		private static readonly Lazy<FMReader> lazy =
			new Lazy<FMReader>(() => new FMReader());

		/// <summary>
		/// Provides a reader creating a featuremanager out of a csv with featurevectors.
		/// </summary>
		public static FMReader Instance => lazy.Value;

		#endregion

		#region -- Instance Properties --

		/// <summary>
		/// Collection containing all supported formats.
		/// </summary>
		public ICollection<string> SupportedFormats => new string[] { "csv" };

		#endregion

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new reader for creating a featuremanager out of a csv with featurevectors.
		/// </summary>
		private FMReader() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Converts a streamreader of a csv file to a featuremanager.
		/// </summary>
		/// <param name="reader">The streamreader of the csv.</param>
		/// <returns>A featuremanager.</returns>
		public FeatureManager ConvertFile(StreamReader reader) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			FMBuilder builder = new FMBuilder();
			if (!reader.EndOfStream)
				builder.AddFeatures(GetFeatureVectors(reader));
			return builder.Build();
		}

		object IO.IReader.ConvertFile(StreamReader reader) => ConvertFile(reader);

		#endregion

		#region -- Private Methods --

		/// <summary>
		/// Extracts the featurevectors out of a csv file.
		/// </summary>
		/// <param name="reader">The streamreader of the csv.</param>
		/// <returns>Dictionary with featurevectors per meshname.</returns>
		private IDictionary<string, FeatureVector> GetFeatureVectors(StreamReader reader) {
			IDictionary<string, FeatureVector> featureVectors = new Dictionary<string, FeatureVector>();

			using (CsvReader csv = CsvReader(reader)) {
				// Csv syntax for reading headers.
				csv.Read();
				csv.ReadHeader();
				// Read the header, to see which measures there are.
				string[] descNames = FilterHeader(csv);
				// Find the individual values.
				while (csv.Read()) {
					// Check to see if there is an entry here.
					if (!csv.TryGetField(MeshName, out string name))
						break;
					// Collect all the descriptors from the CSV.
					IList<IDescriptor> descriptors = new List<IDescriptor>();
					foreach (string descName in descNames)
						if (csv.TryGetField(descName, out string serialisedDesc))
							if (TryDeserialise(descName, serialisedDesc, out IDescriptor desc))
								descriptors.Add(desc);
					// Combine them and save them as a vector.
					FeatureVector vector = new FeatureVector(descriptors.ToArray());
					featureVectors.Add(name, vector);
				}
			}

			return featureVectors;
		}

		private static bool TryDeserialise(string name, string serialised,
											out IDescriptor desc) {
			try {
				desc = DeserialiseDescriptor(name, serialised);
				return true;
			} catch (NotImplementedException) {
				desc = null;
				return false;
			}
		}

		private static IDescriptor DeserialiseDescriptor(string name,
														 string serialised) {
			//Check whether value is an ElemDescriptor or HistDescriptor
			if (ElemDescriptor.TryParse(name, serialised, out ElemDescriptor edesc))
				return edesc;
			else if (HistDescriptor.TryParse(name, serialised, out HistDescriptor hdesc))
				return hdesc;
			else
				throw new NotImplementedException(
					string.Format(
						Settings.Culture,
						Resources.EX_Not_Supported,
						serialised
					)
				);

		}

		#endregion

	}

}
