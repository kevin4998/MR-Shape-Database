using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.IO {

	public class GeomOffReader : IReader<GeometryMesh> {

		public string[] SupportedFormats => new string[] { "off" };

		public GeometryMesh ConvertFile(StreamReader reader) {
			throw new NotImplementedException();
		}

		public Task<GeometryMesh> ConvertFileAsync(StreamReader reader) {
			throw new NotImplementedException();
		}
	}
}
