﻿using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using ShapeDatabase.Features.Statistics;

using static ShapeDatabase.IO.IOConventions;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A writer to convert a <see cref="RecordHolder"/> class into a csv formatted file.
	/// </summary>
	class RecordsWriter : IWriter<IRecordHolder> {

		#region --- Properties ---

		private static readonly Lazy<RecordsWriter> lazy =
			new Lazy<RecordsWriter>(() => new RecordsWriter());

		/// <summary>
		/// Provides a writer to convert <see cref="RecordHolder"/>s into csv.
		/// </summary>
		public static RecordsWriter Instance => lazy.Value;

		/// <summary>
		/// A collection containing the supported formats.
		/// </summary>
		public ICollection<string> SupportedFormats => new string[] { "csv" };

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new writer for records.
		/// </summary>
		private RecordsWriter() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Writes a recordholder to a file, given a streamwriter.
		/// </summary>
		/// <param name="records">The recordholder.</param>
		/// <param name="writer">The streamwriter.</param>
		public void WriteFile(IRecordHolder records, StreamWriter writer) {
			if (records == null)
				throw new ArgumentNullException(nameof(records));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			using (CsvWriter csv = CsvWriter(writer)) {
				// First line specify our Measurement Names.
				csv.WriteField(MeshName);
				foreach (string name in records.MeasureNames)
					csv.WriteField(name);
				csv.NextRecord();
				// Next lines specify our entries.
				foreach (Record record in records) {
					csv.WriteField(record.Name);
					foreach ((string _, object value) in record.Measures)
						if (value is string sValue)
							csv.WriteField(sValue);
						else if (value is IConvertible cValue)
							csv.WriteField(cValue.ToString(Settings.Culture));
						else
							csv.WriteField(value.ToString());
					csv.NextRecord();
				}
				// Finally make sure that all the data is written.
				csv.Flush();
			}
		}

		void IWriter.WriteFile(object type, StreamWriter writer)
			=> WriteFile(type as IRecordHolder, writer);

		#endregion

	}

}
