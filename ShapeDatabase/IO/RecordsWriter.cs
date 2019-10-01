using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ShapeDatabase.Features.Statistics;

namespace ShapeDatabase.IO {

	public class RecordsWriter : IWriter<RecordHolder> {

		#region --- Properties ---

		public static char SeperatorChar => ';';
		public static string Seperator => SeperatorChar.ToString();

		private readonly Lazy<RecordsWriter> lazy = new Lazy<RecordsWriter>();

		public RecordsWriter Instance => lazy.Value;
		public string[] SupportedFormats => new string[] { ".csv" };

		#endregion

		#region --- Constructor Methods ---

		private RecordsWriter() { }

		#endregion

		#region --- Instance Methods ---

		public void WriteFile(RecordHolder records, string location) {
			using (StreamWriter writer = new StreamWriter(location)) {
				WriteFile(records, writer);
			}
		}

		public void WriteFile(RecordHolder records, StreamWriter writer) {
			if (records == null)
				throw new ArgumentNullException(nameof(records));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			// First line specify our Measurement Names.
			writer.WriteLine(string.Join(Seperator, records.MeasureNames.ToArray()));
			// Next lines specify our entries.
			foreach (Record record in records)
				writer.WriteLine(string.Join(Seperator, record.ToArray()));
			// Finally make sure that all the data is written.
			writer.Flush();
		}

		public Task WriteFileAsync(RecordHolder records, string location) {
			return Task.Run(() => WriteFile(records, location));
		}

		public Task WriteFileAsync(RecordHolder records, StreamWriter writer) {
			return Task.Run(() => WriteFile(records, writer));
		}

		#endregion

	}
}
