using CsvHelper;
using ShapeDatabase.Features;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.IO
{
	/// <summary>
	/// Class for writing the featurevectors of the featuremanager to a csv file.
	/// </summary>
	public class FMWriter : IWriter<FeatureManager>
	{

		#region --- Properties ---

		private static readonly Lazy<FMWriter> lazy =
			new Lazy<FMWriter>(() => new FMWriter());

		/// <summary>
		/// Provides a writer to convert <see cref="FeatureManager"/>s into csv.
		/// </summary>
		public static FMWriter Instance => lazy.Value;
		public ICollection<string> SupportedFormats => new string[] { ".csv" };

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new writer for featurevectors.
		/// </summary>
		private FMWriter() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Writes featurevectors to a csv file.
		/// </summary>
		/// <param name="type">The featuremanager containing the featurevectors</param>
		/// <param name="location">The streamwriter to be used</param>
		public void WriteFile(FeatureManager type, StreamWriter writer)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			using (CsvWriter csv = new CsvWriter(writer)) {
				// First line containing the headers.
				csv.WriteField(IOConventions.MeshName);
				foreach (string name in type.DescriptorNames)
					csv.WriteField(name);
				csv.NextRecord();
				// Next lines containing the entries.
				foreach (KeyValuePair<string, FeatureVector> vector in type.VectorDictionary) {
					csv.WriteField(vector.Key);
					foreach (IDescriptor desc in vector.Value.Descriptors)
						csv.WriteField(desc.Serialize());
					csv.NextRecord();
				}
				// Finally make sure that all the data is written.
				csv.Flush();
			}
		}

		void IO.IWriter.WriteFile(object type, StreamWriter writer)
			=> WriteFile(type as FeatureManager, writer);

		#endregion

	}
}
