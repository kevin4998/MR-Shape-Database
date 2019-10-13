using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using g3;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A specific writer implementaion which serialises <see cref="GeometryMesh"/>es
	/// using the <see cref="g3"/> library's serialisers.
	/// </summary>
	public class GeomOffWriter : IWriter<GeometryMesh> {

		#region --- Properties ---

		private static readonly Lazy<GeomOffWriter> lazy = new Lazy<GeomOffWriter>(
			() => new GeomOffWriter()
		);
		/// <summary>
		/// Provides in instance of the writer which uses external serialisation methods.
		/// </summary>
		public static GeomOffWriter Instance => lazy.Value;

		public string[] SupportedFormats { get; } = new string[] { "off" };
		private WriteOptions Options { get; } = WriteOptions.Defaults;

		#endregion

		#region --- Constructor Methods ---

		private GeomOffWriter() { }

		#endregion

		#region --- Instance Methods ---

		public void WriteFile(GeometryMesh type, string location) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException(nameof(location));
			
			IOWriteResult result = StandardMeshWriter.WriteMesh(
										location,
										type.Base,
										Options);
			if (result.code != IOCode.Ok)
				throw new G3WriterException(result);

			using (StreamWriter writer = File.AppendText(location)) {
				IOConventions.WriteIfNormalised(type.IsNormalised, writer);
			}
		}

		public void WriteFile(GeometryMesh type, StreamWriter writer) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			g3.OFFWriter offWriter = new g3.OFFWriter();
			List<WriteMesh> serialisableMeshes = new List<WriteMesh>() {
				new WriteMesh(type.Base)
			};

			IOWriteResult result = offWriter.Write(writer,
												   serialisableMeshes,
												   Options);
			if (result.code != IOCode.Ok)
				throw new G3WriterException(result);

			IOConventions.WriteIfNormalised(type.IsNormalised, writer);
		}

		public Task WriteFileAsync(GeometryMesh type, string location) {
			return Task.Run(() => WriteFile(type, location));
		}

		public Task WriteFileAsync(GeometryMesh type, StreamWriter writer) {
			return Task.Run(() => WriteFile(type, writer));
		}

		#endregion

	}
}
