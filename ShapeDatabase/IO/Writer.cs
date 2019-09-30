using OpenTK;
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
			string val = "C:\\Users\\guusd\\Documents\\UniversiteitUtrecht\\M2.1\\MR\\MR-Shape-Database\\ShapeDatabase\\bin\\Debug\\net48\\Content\\Shapes\\Initial\\Sign\\m1674.off";
			string valtxt = "C:\\Users\\guusd\\Documents\\UniversiteitUtrecht\\M2.1\\MR\\MR-Shape-Database\\ShapeDatabase\\bin\\Debug\\net48\\Content\\Shapes\\Initial\\Sign\\m1674.txt";
			WriteFile(type, new StreamWriter(val));
		}

		public void WriteFile(UnstructuredMesh type, StreamWriter writer)
		{
			using(writer)
			{
				writer.WriteLine("OFF");
				writer.WriteLine($"{type.VerticesCount} {type.FacesCount} 0");
				foreach(Vector3 vertice in type.UnstructuredGrid)
				{
					writer.WriteLine($"{vertice.X} {vertice.Y} {vertice.Z}");
				}
				for(int i = 0; i < type.FacesCount; i++)
				{
					writer.WriteLine($"3 {type.Elements[i * 3]} {type.Elements[i * 3 + 1]} {type.Elements[i * 3 + 2]}");
				}
			}
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
