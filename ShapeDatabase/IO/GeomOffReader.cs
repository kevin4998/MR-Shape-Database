using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using g3;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A specific reader which can convert files to the geomtry 3 mesh format.
	/// </summary>
	class GeomOffReader : IReader<GeometryMesh> {

		#region --- Properties ---

		private static readonly Lazy<GeomOffReader> instance =
			new Lazy<GeomOffReader>(() => new GeomOffReader());

		/// <summary>
		/// A reader which can convert .off files into <see cref="GeometryMesh"/>es.
		/// </summary>
		public static GeomOffReader Instance => instance.Value;

		public ICollection<string> SupportedFormats => new string[] { "off" };

		#endregion

		#region --- Constructor Methods ---

		private GeomOffReader() { }

		#endregion

		#region --- Instance Methods ---

		public GeometryMesh ConvertFile(StreamReader reader) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Thread.CurrentThread.CurrentCulture = Settings.Culture;
			DMesh3 mesh = StandardMeshReader.ReadMesh(reader.BaseStream, "off");
			return new GeometryMesh(mesh, IOConventions.CheckIfNormalised(reader));
		}

		object IReader.ConvertFile(StreamReader reader) => ConvertFile(reader);


		#endregion

	}
}
