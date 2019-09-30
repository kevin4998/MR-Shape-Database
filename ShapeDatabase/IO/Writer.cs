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
	/// <summary>
	/// Class for writing a mesh to an off file.
	/// </summary>
	public class Writer : IWriter<UnstructuredMesh>
	{
		public string[] SupportedFormats { get; } = new string[] { "off" };

		private static readonly Lazy<Writer> lazy =
			new Lazy<Writer>(() => new Writer());

		public static Writer Instance { get { return lazy.Value; } }

		/// <summary>
		/// Writes an unstructured mesh to an off file at a given location.
		/// </summary>
		/// <param name="type">The unstructured mesh that needs to be written to an off file</param>
		/// <param name="location">The location of the off file</param>
		public void WriteFile(UnstructuredMesh type, string location)
		{
			WriteFile(type, new StreamWriter(location));
		}

		/// <summary>
		/// Writes an unstructured mesh to an off file with a given streamwriter
		/// </summary>
		/// <param name="type">The unstructured mesh that needs to be written to an off file</param>
		/// <param name="writer">The streamwriter that needs to be used</param>
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

		/// <summary>
		/// Writes an unstructured mesh to an off file at a given location.
		/// </summary>
		/// <param name="type">The unstructured mesh that needs to be written to an off file</param>
		/// <param name="location">The location of the off file</param>
		public Task WriteFileAsync(UnstructuredMesh type, string location)
		{
			return Task.Run(() => WriteFile(type, location));
		}

		/// <summary>
		/// Writes an unstructured mesh to an off file with a given streamwriter
		/// </summary>
		/// <param name="type">The unstructured mesh that needs to be written to an off file</param>
		/// <param name="writer">The streamwriter that needs to be used</param>
		public Task WriteFileAsync(UnstructuredMesh type, StreamWriter writer)
		{
			return Task.Run(() => WriteFile(type, writer));
		}
	}
}
