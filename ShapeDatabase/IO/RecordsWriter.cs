using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using ShapeDatabase.Features.Statistics;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A writer to convert a <see cref="RecordHolder"/> class into a csv formatted file.
	/// </summary>
	public class RecordsWriter : IWriter<RecordHolder> {

		#region --- Properties ---

		private static readonly Lazy<RecordsWriter> lazy =
			new Lazy<RecordsWriter>(() => new RecordsWriter());

		/// <summary>
		/// Provides a writer to convert <see cref="RecordHolder"/>s into csv.
		/// </summary>
		public static RecordsWriter Instance => lazy.Value;


		public ICollection<string> SupportedFormats => new string[] { ".csv" };

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new writer for records.
		/// </summary>
		private RecordsWriter() { }

		#endregion

		#region --- Instance Methods ---

		public void WriteFile(RecordHolder records, StreamWriter writer) {
			if (records == null)
				throw new ArgumentNullException(nameof(records));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			using (CsvWriter csv = new CsvWriter(writer)) {
				// First line specify our Measurement Names.
				foreach(string name in records.MeasureNames)
					csv.WriteField(name);
				csv.NextRecord();
				// Next lines specify our entries.
				foreach (Record record in records) { 
					foreach((string _, object value) in record.Measures)
						csv.WriteField(value);
					csv.NextRecord();
				}
				// Finally make sure that all the data is written.
				csv.Flush();
			}
		}

		void IWriter.WriteFile(object type, StreamWriter writer)
			=> WriteFile(type as RecordHolder, writer);

		#endregion

	}
}
