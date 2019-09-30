using ShapeDatabase.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.IO
{
	public class Writer : IWriter<UnstructuredMesh>
	{
		public string[] SupportedFormats { get; } = new string[] { "off" };

		private static readonly Lazy<Writer> lazy =
			new Lazy<Writer>(() => new Writer());

		public static Writer Instance { get { return lazy.Value; } }

		public void WriteFile(UnstructuredMesh type, string location)
		{
			WriteFile(type, new StreamWriter(location));
		}

		public void WriteFile(UnstructuredMesh type, StreamWriter writer)
		{
			//TODO
		}

		public Task WriteFileAsync(UnstructuredMesh type, string location)
		{
			return Task.Run(() => WriteFile(type, location));
		}	

		public Task WriteFileAsync(UnstructuredMesh type, StreamWriter writer)
		{
			return Task.Run(() => WriteFile(type, writer));
		}
	}
}
