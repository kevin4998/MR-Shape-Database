using System;
using System.IO;
using System.Threading.Tasks;
using g3;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.IO {

	public class GeomOffReader : IReader<GeometryMesh> {

		private static readonly Lazy<GeomOffReader> instance =
			new Lazy<GeomOffReader>(() => new GeomOffReader());
		/// <summary>
		/// A reader which can convert .off files into <see cref="GeometryMesh"/>es.
		/// </summary>
		public static GeomOffReader Instance => instance.Value;

		public string[] SupportedFormats => new string[] { "off" };

		private GeomOffReader() { }

		public GeometryMesh ConvertFile(StreamReader reader) {
			DMesh3 mesh = StandardMeshReader.ReadMesh(reader.BaseStream, "off");
			return new GeometryMesh(mesh);
		}

		public Task<GeometryMesh> ConvertFileAsync(StreamReader reader) {
			return Task.Run(() => ConvertFile(reader));
		}

	}
}
