using CsvHelper;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.IO;
using ShapeDatabase.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for creating a featuremanager out of a csv with featurevectors.
	/// </summary>
	public class FMReader : IReader<FeatureManager>
	{
		#region --- Properties ---

		/// <summary>
		/// The character which is used to seperate values in a csv document.
		/// </summary>
		public static char Seperator => ',';
		/// <summary>
		/// The character which is used to seperate values of a single histogram descriptor.
		/// </summary>
		public static char HistSeperator => ';';

		#endregion

		#region -- Static Properties --

		private static readonly Lazy<FMReader> lazy =
			new Lazy<FMReader>(() => new FMReader());

		#endregion

		#region -- Instance Properties --

		/// <summary>
		/// Provides a reader creating a featuremanager out of a csv with featurevectors.
		/// </summary>
		public static FMReader Instance => lazy.Value;
		public ICollection<string> SupportedFormats => new string[] { ".csv" };

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new reader for creating a featuremanager out of a csv with featurevectors.
		/// </summary>
		private FMReader() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Converts a streamreader of a csv file to a featuremanager
		/// </summary>
		/// <param name="reader">The streamreader of the csv</param>
		/// <returns>A featuremanager</returns>
		public FeatureManager ConvertFile(StreamReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (reader.EndOfStream)
				throw new ArgumentException(Resources.EX_EndOfStream);

			Dictionary<string, FeatureVector> featureVectors = GetFeatureVectors(reader);

			return new FMBuilder(featureVectors).Build();
		}

		public Task<FeatureManager> ConvertFileAsync(StreamReader reader)
		{
			return Task.Run(() => ConvertFile(reader));
		}

		#endregion

		#region -- Private Methods --

		/// <summary>
		/// Extracts the featurevectors out of a csv file
		/// </summary>
		/// <param name="reader">The streamreader of the csv</param>
		/// <returns>Dictionary with featurevectors per meshname</returns>
		private Dictionary<string, FeatureVector> GetFeatureVectors(StreamReader reader)
		{
			Dictionary<string, FeatureVector> featureVectors = new Dictionary<string, FeatureVector>();

			using (CsvReader csv = new CsvReader(reader)) {
				// Read the header, to see which measures there are.
				IEnumerable<string> names = csv.GetRecords<string>();
				// Find the individual values.
				do {
					// Check to see if there is an entry here.
					if (!csv.TryGetField(FMWriter.MeshName, out string name))
						break;
					// Collect all the descriptors from the CSV.
					IList<IDescriptor> descriptors = new List<IDescriptor>();
					foreach(string descName in names)
						if (csv.TryGetField(descName, out string serialisedDesc))
							if (TryDeserialise(descName, serialisedDesc, out IDescriptor desc))
								descriptors.Add(desc);
					// Combine them and save them as a vector.
					FeatureVector vector = new FeatureVector(descriptors.ToArray());
					featureVectors.Add(name, vector);
				} while (csv.Read());
			}

			return featureVectors;
		}

		private static bool TryDeserialise(string name, string serialised,
											out IDescriptor desc) {
			try { 
				desc = DeserialiseDescriptor(name, serialised);
				return true;
			} catch (NotImplementedException _) {
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
