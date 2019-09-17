using System;
using System.IO;
using System.Threading.Tasks;
using OpenTK;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A simpel reader implementation to convert OFF files into meshes.
	/// </summary>
	public class OFFReader : IReader<object> {

		const string EX_MISSING_VALUES = "Missing values! Expected 3 values for vertices, faces and edges but received:\"{0}\".";
		const string EX_MISSING_VALUE = "Missing value for {0}.";

		const string EX_INVALID_COORD = "The current line does not give a right amount of coordinates: \'{0}\'.";
		const string EX_MISSING_VERTEXCOUNT = "The current line does not correctly define the amount of vertices for this shape: \'{0}\'.";

		public string[] SupportedFormats { get; } = new string[] { "off" };


		public OFFReader() { }

		public object ConvertFile(StreamReader reader) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (reader.EndOfStream)
				return null;

			string line = reader.ReadLine().Trim();
			// The first line in .off files always start with OFF.
			// Verify that we have the right file format.
			if (!SupportedFormats[0].Equals(line.ToLower())) {
				// This was not the right format, so show the problem to the user/dev.
				throw new InvalidFormatException(line.ToLower(), string.Join(", ", SupportedFormats));
			}

			line = reader.ReadLine().Trim();
			string[] values = line.Split(' ');
			// The second line in .off files specifies the
			// vertices, faces and edges.
			if (values.Length != 3)
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUES, line));
			if (!int.TryParse(values[0], out int vertexCount))
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUE, "vertices"));
			if (!int.TryParse(values[1], out int faceCount))
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUE, "faces"));
			if (!int.TryParse(values[2], out int _/*edgeCount*/))
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUE, "edges"));

			Vector3[] vob = new Vector3[vertexCount];
			// The third line and following define #vertices
			// with their representative x, y and z coordinates.
			for (int index = 0; index < vertexCount && !reader.EndOfStream; index++) {
				line = reader.ReadLine().Trim();
				values = line.Split();

				if (values.Length != 3
					|| !float.TryParse(values[0], out float x)
					|| !float.TryParse(values[1], out float y)
					|| !float.TryParse(values[2], out float z))
					throw new InvalidFormatException(string.Format(EX_INVALID_COORD, line));

				vob[index] = new Vector3(x, y, z);
			}

			uint[] ebo = new uint[faceCount * 3];	// 3 because of triangles.
			// The Fourth section defines the collection of faces.
			for (int index = 0; index < faceCount && !reader.EndOfStream; /* Index increment in code. */ ) {
				line = reader.ReadLine().Trim();
				values = line.Split();

				if (values.Length == 0
					|| !int.TryParse(values[0], out int vertices)
					|| values.Length != (vertices + 1))
					throw new InvalidFormatException(string.Format(EX_MISSING_VERTEXCOUNT, line));

				// TODO Handle other shapes than triangels
				for (int indice = 1; indice <= vertices; indice++)
					uint.TryParse(values[indice], out ebo[index++]);	// TODO Handel incorrect situation right.
			}

			// The end of the file should be reached so return the value.
			return new Tuple<Vector3[], uint[]>(vob, ebo);

		}

		public Task<object> ConvertFileAsync(StreamReader reader) {
			return Task.Run(() => ConvertFile(reader));
		}
	}
}
