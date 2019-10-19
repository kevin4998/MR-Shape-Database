using System;
using System.IO;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A collection of conventions which should be held along all the
	/// IO objects of the ShapesDatabase application.
	/// </summary>
	public static class IOConventions {

		/// <summary>
		/// The message to show that a shape is normalised.
		/// </summary>
		public const string NORMALISED_DATA = "Normalised";

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
				if (found) break;
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

	}
}
