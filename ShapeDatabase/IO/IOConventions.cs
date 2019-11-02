using System;
using System.IO;
using CsvHelper;
using ShapeDatabase.Properties;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A collection of conventions which should be held along all the
	/// IO objects of the ShapesDatabase application.
	/// </summary>
	static class IOConventions {

		/// <summary>
		/// The message to show that a shape is normalised.
		/// </summary>
		public const string NORMALISED_DATA = "Normalised";

		/// <summary>
		/// The word to describe the name of a mesh.
		/// </summary>
		public static string MeshName => Resources.F_MeshName;


		/// <summary>
		/// Check if at the end of a given stream it is mentioned that
		/// this stream is normalised. The shape should be read before
		/// checking this condition.
		/// </summary>
		/// <param name="reader">The <see cref="StreamReader"/> which has already
		/// been used to read in a shape, to check for normalisation.</param>
		/// <returns><see langword="true"/> if at the end of the stream it says
		/// that the previously read shape is normalised.</returns>
		public static bool CheckIfNormalised(StreamReader reader) {
			if (reader == null)
				return false;
			// Reset stream if it is at the end.
			// G3 readers read to the end giving us problems.
			if (reader.EndOfStream)
				reader.BaseStream.Seek(0, SeekOrigin.Begin);

			string lastLine;
			bool found = false;
			while (!reader.EndOfStream) {
				lastLine = reader.ReadLine();
				if (!lastLine.StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
					continue;

				string text = lastLine.Substring(2);
				found = NORMALISED_DATA.Equals(text, StringComparison.OrdinalIgnoreCase);
				if (found)
					break;
			}

			return found;

		}

		/// <summary>
		/// Attaches to the end of a file in comments if this shape was normalised
		/// before it was written out.
		/// </summary>
		/// <param name="normalised">If the shape which is saved was normalised.</param>
		/// <param name="writer">The <see cref="StreamWriter"/> where the figure
		/// has been serialised o.</param>
		public static void WriteIfNormalised(bool normalised, StreamWriter writer) {
			if (writer == null)
				return;
			if (normalised) {
				writer.WriteLine();
				writer.WriteLine($"# {NORMALISED_DATA}");
			}
		}


		/// <summary>
		/// A constructor which will create CSV readers all having the same format
		/// convention.
		/// </summary>
		/// <param name="writer">The stream which will provide the underlying
		/// file access for CSV methods.</param>
		/// <returns>A <see cref="CsvHelper.CsvReader"/> which was created
		/// based on the underlying stream. The caller of this method needs
		/// to provide disposing of this writer.</returns>
		public static CsvReader CsvReader(StreamReader reader) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			CsvReader csv = new CsvReader(reader);
			csv.Configuration.Delimiter =
				Settings.Culture.TextInfo.ListSeparator;
			csv.Configuration.IgnoreBlankLines = true;
			return csv;
		}

		/// <summary>
		/// A constructor which will create CSV writers all having the same format
		/// convention.
		/// </summary>
		/// <param name="writer">The stream which will provide the underlying
		/// file access for CSV methods.</param>
		/// <returns>A <see cref="CsvHelper.CsvWriter"/> which was created
		/// based on the underlying stream. The caller of this method needs
		/// to provide disposing of this writer.</returns>
		public static CsvWriter CsvWriter(StreamWriter writer) {
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			CsvWriter csv = new CsvWriter(writer);
			csv.Configuration.Delimiter =
				Settings.Culture.TextInfo.ListSeparator;
			return csv;
		}


		/// <summary>
		/// Filters a CSV header by removing the name of the entry from the collection.
		/// </summary>
		/// <param name="reader">The csv reader containing the header.</param>
		/// <returns>A new array of headers without the name header.</returns>
		public static string[] FilterHeader(CsvReader reader) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			return FilterHeader(reader.Context.HeaderRecord);
		}

		/// <summary>
		/// Filters a CSV header by removing the name of the entry from the collection.
		/// </summary>
		/// <param name="header">The collection of headers in the CSV file.</param>
		/// <returns>A new array of headers without the name header.</returns>
		public static string[] FilterHeader(string[] header) {
			// Name fix because the first element is the name not a descriptor
			string[] descNames = new string[header.Length - 1];
			for (int i = 0; i < descNames.Length;)
				descNames[i++] = header[i];
			return descNames;
		}

	}
}
