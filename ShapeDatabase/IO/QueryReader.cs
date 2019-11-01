using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using ShapeDatabase.IO;
using ShapeDatabase.Query;

using static ShapeDatabase.IO.IOConventions;

namespace ShapeDatabase.IO {
	/// <summary>
	/// Class for reading previous query results from a csv file.
	/// </summary>
	class QueryReader : IReader<QueryResult[]> {

		#region --- Properties ---

		#region -- Static Properties --

		private static readonly Lazy<QueryReader> lazy =
			new Lazy<QueryReader>(() => new QueryReader());

		/// <summary>
		/// Provides a reader creating a featuremanager out of a csv with featurevectors.
		/// </summary>
		public static QueryReader Instance => lazy.Value;

		#endregion

		#region -- Instance Properties --

		public ICollection<string> SupportedFormats => new string[] { "csv" };

		#endregion

		#endregion

		#region --- Constructor Methods ---

		private QueryReader() { }

		#endregion

		#region --- Instance Methods ---

		public QueryResult[] ConvertFile(StreamReader reader) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			List<QueryResult> results = new List<QueryResult>();
			using (CsvReader csv = CsvReader(reader)) {
				// Csv syntax for reading headers.
				csv.Read();
				csv.ReadHeader();
				// Read the header, to see which measures there are.
				string[] kNames = FilterHeader(csv);
				// Find the individual values.
				while (csv.Read()) {
					// Check to see if there is an entry here.
					if (!csv.TryGetField(MeshName, out string name))
						break;
					QueryResult query = new QueryResult(name);
					foreach (string kName in kNames)
						if (csv.TryGetField(kName, out string serialised)
							&& QueryItem.TryParse(serialised, out QueryItem item))
							query.AddItem(item);
					results.Add(query);
				}
			}
			return results.ToArray();
		}

		object IReader.ConvertFile(StreamReader reader) => ConvertFile(reader);

		#endregion

	}

}
