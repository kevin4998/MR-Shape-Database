using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.IO;
using System;
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

		#region -- Exceptions --

		const string EX_END_STREAM = "Cannot read data from the end of the stream.";

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
				throw new ArgumentException(EX_END_STREAM);

			Dictionary<string, FeatureVector> featureVectors = GetFeatureVectors(reader);

			return new FeatureManager(featureVectors);
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

			//First read the descriptor names
			string[] descriptorNames = reader.ReadLine().Split(Seperator).Skip(1).ToArray();

			//Next read the descriptor values
			while (!reader.EndOfStream)
			{
				string line = reader.ReadLine();
				string[] values = line.Split(Seperator);
				string meshName = values[0];
				values = values.Skip(1).ToArray();
				
				List<IDescriptor> descriptors = new List<IDescriptor>();

				for(int i = 0; i < descriptorNames.Length; i++)
				{
					descriptors.Clear();
					string value = values[i];

					//Check whether value is an ElemDescriptor or HistDescriptor
					if(!value.Contains(HistSeperator))
					{
						descriptors.Add(new ElemDescriptor(descriptorNames[i], Convert.ToDouble(value)));
					}
					else
					{
						descriptors.Add(new HistDescriptor(descriptorNames[i], Convert.ToDouble(values[0]), Convert.ToDouble(values[1]), values.Skip(2).Select(x => Int32.Parse(x)).ToArray()));
					}
				}

				featureVectors.Remove(meshName);
				featureVectors.Add(meshName, new FeatureVector(descriptors));
			}

			return featureVectors;
		}

		#endregion
	}
}
