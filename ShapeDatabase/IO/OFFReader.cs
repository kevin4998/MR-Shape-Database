using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util;
using OpenTK;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A simpel reader implementation to convert OFF files into meshes.
	/// </summary>
	public class OFFReader : IReader<UnstructuredMesh> {

		const string EX_END_STREAM = "Cannot read data from the end of the stream.";

		const string EX_MISSING_VALUES = "Missing values! Expected 3 values for vertices, faces and edges but received:\"{0}\".";
		const string EX_MISSING_VALUE = "Missing value for {0}.";

		const string EX_INVALID_COORD = "The current line does not give a right amount of coordinates: \'{0}\'.";
		const string EX_MISSING_VERTEXCOUNT = "The current line does not correctly define the amount of vertices for this shape: \'{0}\'.";

		const string EX_NON_TRIANGLE = "Only triangle shapes are supported.";
		const string EX_NON_NUMBER = "Could not convert the number \'{0}\'.";

		private static readonly Lazy<OFFReader> instance = new Lazy<OFFReader>(() => new OFFReader());

		public string[] SupportedFormats { get; } = new string[] { "off" };


		private OFFReader() { }

		public UnstructuredMesh ConvertFile(StreamReader reader) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (reader.EndOfStream)
				throw new ArgumentException(EX_END_STREAM);


			string line = reader.ReadLine().Trim();
			// The first line in .off files always start with OFF.
			// Verify that we have the right file format.
			if (!SupportedFormats[0].Equals(line.ToLower())) {
				// This was not the right format, so show the problem to the user/dev.
				throw new InvalidFormatException(line.ToLower(), string.Join(", ", SupportedFormats));
			}

			NumberStyles numberStyle = NumberStyles.Any;
			CultureInfo culture = Settings.Culture;

			line = reader.ReadLine().Trim();
			string[] values = line.Split(' ');
			// The second line in .off files specifies the
			// vertices, faces and edges.
			if (values.Length != 3)
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUES, line));
			if (!uint.TryParse(values[0], numberStyle, culture, out uint vertexCount))
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUE, "vertices"));
			if (!uint.TryParse(values[1], numberStyle, culture, out uint faceCount))
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUE, "faces"));
			if (!uint.TryParse(values[2], numberStyle, culture, out uint _/*edgeCount*/))
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUE, "edges"));

			float minValue = int.MaxValue;
			float maxValue = int.MinValue;
			Vector3[] vob = new Vector3[vertexCount];	// 3 dimensional space
			// The third line and following define #vertices
			// with their representative x, y and z coordinates.
			for (uint index = 0; index < vertexCount && !reader.EndOfStream; index++) {
				line = reader.ReadLine().Trim();
				values = line.Split();

				if (values.Length != 3
					|| !float.TryParse(values[0], numberStyle, culture, out float x)
					|| !float.TryParse(values[1], numberStyle, culture, out float y)
					|| !float.TryParse(values[2], numberStyle, culture, out float z))
					throw new InvalidFormatException(string.Format(EX_INVALID_COORD, line));

				minValue = Math.Min(minValue, x);
				minValue = Math.Min(minValue, y);
				minValue = Math.Min(minValue, z);

				maxValue = Math.Max(maxValue, x);
				maxValue = Math.Max(maxValue, y);
				maxValue = Math.Max(maxValue, z);

				vob[index] = new Vector3(x, y, z);
			}


			uint max = faceCount * 3; // 3 because of triangles
			uint[] ebo = new uint[max];
			// The Fourth section defines the collection of faces.
			for (uint index = 0; index < max && !reader.EndOfStream; /* Index increment in code. */ ) {
				line = reader.ReadLine().Trim();
				values = line.Split();

				if (values.Length == 0
					|| !uint.TryParse(values[0], numberStyle, culture, out uint vertices)
					|| values.Length != (vertices + 1))
					throw new InvalidFormatException(string.Format(EX_MISSING_VERTEXCOUNT, line));

				if (vertices != 3)
					throw new NotImplementedException(EX_NON_TRIANGLE);

				for (uint indice = 1; indice <= vertices; indice++)
					if (!uint.TryParse(values[indice], numberStyle, culture, out ebo[index++]))
						throw new InvalidFormatException(string.Format(EX_NON_NUMBER, values[indice]));
			}


			// The end of the file should be reached so return the value.
			return new UnstructuredMesh(vob.Normalise(minValue, maxValue), ebo);

		}

		public Task<UnstructuredMesh> ConvertFileAsync(StreamReader reader) {
			return Task.Run(() => ConvertFile(reader));
		}

		public static OFFReader Instance { get; } = instance.Value;

	}

}
