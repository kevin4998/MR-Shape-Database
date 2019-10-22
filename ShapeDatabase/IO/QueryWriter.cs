using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.IO;
using ShapeDatabase.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.IO
{
	/// <summary>
	/// Class for writing the query results to a csv file.
	/// </summary>
	public class QueryWriter : IWriter<QueryResult[]>
	{

		#region --- Properties ---

		/// <summary>
		/// The character which is used to seperate values in a csv document.
		/// </summary>
		public static char SeperatorChar => ',';
		/// <summary>
		/// A string value which represent the seperater character.
		/// <see cref="SeperatorChar"/>
		/// </summary>
		public static string Seperator => SeperatorChar.ToString(Settings.Culture);


		private static readonly Lazy<QueryWriter> lazy =
			new Lazy<QueryWriter>(() => new QueryWriter());

		/// <summary>
		/// Provides a writer to convert query results into csv
		/// </summary>
		public static QueryWriter Instance => lazy.Value;
		public ICollection<string> SupportedFormats => new string[] { ".csv" };

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new writer for query results.
		/// </summary>
		private QueryWriter() { }

		#endregion

		#region --- Instance Methods ---

		public void WriteFile(QueryResult[] type, string location)
		{
			using (StreamWriter writer = new StreamWriter(location))
			{
				WriteFile(type, writer);
			}
		}

		public void WriteFile(QueryResult[] type, StreamWriter writer)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			string[] columnNames = new string[Settings.KBestResults + 1];
			columnNames[0] = "QueryMesh";
			for(int i = 1; i <= Settings.KBestResults; i++)
			{
				columnNames[i] = $"K = {i}";
			}
			
			writer.WriteLine(string.Join(Seperator, columnNames));

			foreach(QueryResult queryResult in type)
			{
				writer.WriteLine(queryResult.QueryName + Seperator + string.Join(Seperator, queryResult.Results.Select(x => x.MeshName + " (" + x.MeshDistance + ")" )));
			}

			writer.Flush();
		}

		public Task WriteFileAsync(QueryResult[] results, string location)
		{
			return Task.Run(() => WriteFile(results, location));
		}

		public Task WriteFileAsync(QueryResult[] results, StreamWriter writer)
		{
			return Task.Run(() => WriteFile(results, writer));
		}

		#endregion

	}
}
