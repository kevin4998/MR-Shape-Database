using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using ShapeDatabase.Features;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.IO;

namespace ShapeDatabase.IO {
	/// <summary>
	/// Class for writing the featurevectors of the featuremanager to a csv file.
	/// </summary>
	class FMWriter : IWriter<FeatureManager> {

		#region --- Properties ---

		private static readonly Lazy<FMWriter> lazy =
			new Lazy<FMWriter>(() => new FMWriter());

		/// <summary>
		/// Provides a writer to convert <see cref="FeatureManager"/>s into csv.
		/// </summary>
		public static FMWriter Instance => lazy.Value;
		public ICollection<string> SupportedFormats => new string[] { "csv" };

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new writer for featurevectors.
		/// </summary>
		private FMWriter() { }

		#endregion

		#region --- Instance Methods ---

		public void WriteFile(FeatureManager type, StreamWriter writer) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			if (type.FeatureCount == 0)
				return;

			using (CsvWriter csv = new CsvWriter(writer)) {
				csv.Configuration.Delimiter =
					Settings.Culture.TextInfo.ListSeparator;

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

		void IWriter.WriteFile(object type, StreamWriter writer)
			=> WriteFile(type as FeatureManager, writer);

		#endregion

	}
}
