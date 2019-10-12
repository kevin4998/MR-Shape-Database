using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ShapeDatabase.Shapes;
using OpenTK;
using System.Runtime.CompilerServices;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A simpel reader implementation to convert OFF files into meshes.
	/// </summary>
	[Obsolete("Make use of the new GeomOffReader to read in shapes.")]
	public class OFFReader : IReader<IMesh> {

		#region --- Properties ---

		#region -- Exceptions --

		const string EX_END_STREAM = "Cannot read data from the end of the stream.";

		const string EX_MISSING_VALUES = "Missing values! Expected 3 values for vertices, faces and edges but received:\"{0}\".";
		const string EX_MISSING_VALUE = "Missing value for {0}.";

		const string EX_INVALID_COORD = "The current line does not give a right amount of coordinates: \'{0}\'.";
		const string EX_MISSING_VERTEXCOUNT = "The current line does not correctly define the amount of vertices for this shape: \'{0}\'.";

		const string EX_NON_TRIANGLE = "Only triangle shapes are supported.";
		const string EX_NON_NUMBER = "Could not convert the number \'{0}\'.";

		#endregion

		#region -- Static Properties --

		private static readonly Lazy<OFFReader> instance =
			new Lazy<OFFReader>(() => new OFFReader());
		/// <summary>
		/// A reader which can convert .off files into <see cref="IMesh"/>es.
		/// </summary>
		public static OFFReader Instance => instance.Value;

		#endregion

		#region -- Instance Properties --

		public string[] SupportedFormats { get; } = new string[] { "off" };

		#endregion

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instantiates a new reader to convert files into
		/// <see cref="IMesh"/>es.
		/// </summary>
		private OFFReader() { }

		#endregion

		#region --- Instance Methods ---

		#region -- Public Methods --

		public IMesh ConvertFile(StreamReader reader) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (reader.EndOfStream)
				throw new ArgumentException(EX_END_STREAM);

			// Check if the file contains the right format.
			CheckFormat(reader);
			// Check how many vertices and faces we should read.
			(uint vertexCount, uint faceCount, uint _) = ItemCount(reader);
			// Read all the vertices as specified by the amount.
			Vector3[] vob = GetVertices(reader, vertexCount);
			// Read all the faces as specified by the amount.
			uint[]	  ebo = GetFaces(reader, faceCount);
			// Return the mesh but normalised to the [-1,1] range centered on 0,0,0.
			return new UnstructuredMesh(vob, ebo, false);//.Normalise();
		}

		public Task<IMesh> ConvertFileAsync(StreamReader reader) {
			return Task.Run(() => ConvertFile(reader));
		}

		#endregion

		#region -- Private Methods --

		/// <summary>
		/// The first line in .off files always start with OFF.
		/// Verify that we have the right file format.
		/// </summary>
		/// <param name="reader">The current file that is being worked on.</param>
		/// <exception cref="InvalidFormatException">If the lines does not start with OFF.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CheckFormat(StreamReader reader) {
			string line = reader.ReadLine().Trim();
			if (!SupportedFormats[0].Equals(line.ToLower()))
				throw new InvalidFormatException(line.ToLower(),
												 string.Join(", ", SupportedFormats));
		}

		/// <summary>
		/// The second line in .off files specifies the vertices, faces and edges.
		/// </summary>
		/// <param name="reader">The current file that is being worked on.</param>
		/// <returns>A tuple of the number of vertices, faces and edges in that order.
		/// </returns>
		/// <exception cref="InvalidFormatException">If one of the numbers
		/// is missing or not correctly represented.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private (uint, uint, uint) ItemCount(StreamReader reader) {
			NumberStyles numberStyle = NumberStyles.Any;
			CultureInfo culture = Settings.Culture;

			string line = reader.ReadLine().Trim();
			string[] values = line.Split(' ');

			if (values.Length != 3)
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUES, line));
			if (!uint.TryParse(values[0], numberStyle, culture, out uint vertexCount))
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUE, "vertices"));
			if (!uint.TryParse(values[1], numberStyle, culture, out uint faceCount))
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUE, "faces"));
			if (!uint.TryParse(values[2], numberStyle, culture, out uint edgeCount))
				throw new InvalidFormatException(string.Format(EX_MISSING_VALUE, "edges"));

			return (vertexCount, faceCount, edgeCount);
		}

		/// <summary>
		/// The next paragraph of lines specify all the vertex points.
		/// </summary>
		/// <param name="reader">The current file that is being worked on.</param>
		/// <param name="vertexCount">The number of lines with vertices.</param>
		/// <returns>An array containing all the vertices in the right order.</returns>
		/// <exception cref="InvalidFormatException">If the coordinates on one of the lines
		/// does not specify a vertex.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Vector3[] GetVertices(StreamReader reader, uint vertexCount) {
			Vector3[] vob = new Vector3[vertexCount];   // 3 dimensional space
														// The third line and following define #vertices
														// with their representative x, y and z coordinates.
			string line;
			string[] values;
			NumberStyles numberStyle = NumberStyles.Any;
			CultureInfo culture = Settings.Culture;

			for (uint index = 0; index < vertexCount && !reader.EndOfStream; index++) {
				line = reader.ReadLine().Trim();
				values = line.Split();

				if (values.Length != 3
					|| !float.TryParse(values[0], numberStyle, culture, out float x)
					|| !float.TryParse(values[1], numberStyle, culture, out float y)
					|| !float.TryParse(values[2], numberStyle, culture, out float z))
					throw new InvalidFormatException(string.Format(EX_INVALID_COORD, line));

				vob[index] = new Vector3(x, y, z);
			}

			return vob;
		}

		/// <summary>
		/// The last paragraph of lines specify the faces with the previously
		/// specified vertices.
		/// </summary>
		/// <param name="reader">The current file that is being worked on.</param>
		/// <param name="faceCount">The number of lines containing the faces of a triangle.
		/// </param>
		/// <returns>An array containing all the faces in the right order.</returns>
		/// <exception cref="InvalidFormatException">If the numbers on one of the lines
		/// do not specify a face.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint[] GetFaces(StreamReader reader, uint faceCount) {
			uint max = faceCount * 3; // 3 because of triangles
			uint[] ebo = new uint[max];

			string line;
			string[] values;
			NumberStyles numberStyle = NumberStyles.Any;
			CultureInfo culture = Settings.Culture;

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

			return ebo;
		}

		#endregion

		#endregion

	}

}
