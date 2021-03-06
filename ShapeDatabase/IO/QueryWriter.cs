﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.IO;
using ShapeDatabase.Query;

using static ShapeDatabase.IO.IOConventions;

namespace ShapeDatabase.IO {

	/// <summary>
	/// Class for writing the query results to a csv file.
	/// </summary>
	class QueryWriter : IWriter<QueryResult[]> {

		#region --- Properties ---

		private static readonly Lazy<QueryWriter> lazy =
			new Lazy<QueryWriter>(() => new QueryWriter());

		/// <summary>
		/// Provides a writer to convert query results into csv.
		/// </summary>
		public static QueryWriter Instance => lazy.Value;

		/// <summary>
		/// Collection containing the supported results.
		/// </summary>
		public ICollection<string> SupportedFormats => new string[] { "csv" };

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new writer for query results.
		/// </summary>
		private QueryWriter() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Writes an array of queryresults  to a file, given a streamwriter.
		/// </summary>
		/// <param name="type">The array with queryresults.</param>
		/// <param name="writer">The streamwriter.</param>
		public void WriteFile(QueryResult[] type, StreamWriter writer) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			int maxEntries = 0;
			foreach(QueryResult result in type)
				maxEntries = Math.Max(maxEntries, result.Count);

			using (CsvWriter csv = CsvWriter(writer)) {
				//csv.Configuration.Delimiter = ";";

				// Header of the CSV file.
				csv.WriteField(IOConventions.MeshName);
				for (int i = 1; i <= maxEntries; i++)
					csv.WriteField($"K = {i}");
				csv.NextRecord();
				// Each item in the CSV file.
				foreach (QueryResult result in type) {
					csv.WriteField(result.QueryName);
					foreach (QueryItem item in result.Results)
						csv.WriteField(item);
					csv.NextRecord();
				}
				// Finally make sure that all the data is written.
				csv.Flush();
			}
		}

		void IWriter.WriteFile(object type, StreamWriter writer)
			=> WriteFile(type as QueryResult[], writer);

		#endregion
	}
}
