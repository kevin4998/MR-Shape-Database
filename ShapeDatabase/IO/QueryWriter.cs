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
	/// Class for writing the query results to a csv file.
	/// </summary>
	public class QueryWriter : IWriter<Tuple<string, IList<(string, double)>>[]>
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

		/// <summary>
		/// Writes the query results to a csv file.
		/// </summary>
		/// <param name="type">The query results</param>
		/// <param name="location">Location of the csv file</param>
		public void WriteFile(Tuple<string, IList<(string, double)>>[] type, string location)
		{
			using (StreamWriter writer = new StreamWriter(location))
			{
				WriteFile(type, writer);
			}
		}

		/// <summary>
		/// Writes the query results to a csv file.
		/// </summary>
		/// <param name="type">The query results</param>
		/// <param name="location">Location of the csv file</param>
		public void WriteFile(Tuple<string, IList<(string, double)>>[] type, StreamWriter writer)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			string[] columnNames = new string[Settings.KBestResults + 1];
			columnNames[0] = "QueryMesh";
			for(int i = 1; i < Settings.KBestResults + 1; i++)
			{
				columnNames[i] = $"K = {i}";
			}
			
			writer.WriteLine(string.Join(Seperator, columnNames));

			foreach(Tuple<string, IList<(string, double)>> result in type)
			{
				writer.WriteLine(result.Item1 + Seperator + string.Join(Seperator, result.Item2.Select(x => x.Item1 + " (" + x.Item2 + ")" )));
			}

			writer.Flush();
		}

		public Task WriteFileAsync(Tuple<string, IList<(string, double)>>[] results, string location)
		{
			return Task.Run(() => WriteFile(results, location));
		}

		public Task WriteFileAsync(Tuple<string, IList<(string, double)>>[] results, StreamWriter writer)
		{
			return Task.Run(() => WriteFile(results, writer));
		}

		#endregion

	}
}
