using OpenTK;
using ShapeDatabase.Shapes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShapeDatabase.IO
{
	/// <summary>
	/// Class for writing a mesh to an off file.
	/// </summary>
	public class OFFWriter : IWriter<IMesh>
	{
		public string[] SupportedFormats { get; } = new string[] { "off" };

		private static readonly Lazy<OFFWriter> lazy =
			new Lazy<OFFWriter>(() => new OFFWriter());

		public static OFFWriter Instance { get { return lazy.Value; } }

		/// <summary>
		/// Writes an unstructured mesh to an off file at a given location.
		/// </summary>
		/// <param name="type">The unstructured mesh that needs to be written to an off file</param>
		/// <param name="location">The location of the off file</param>
		public void WriteFile(IMesh type, string location)
		{
			WriteFile(type, new StreamWriter(location));
		}

		/// <summary>
		/// Writes an unstructured mesh to an off file with a given streamwriter
		/// </summary>
		/// <param name="type">The unstructured mesh that needs to be written to an off file</param>
		/// <param name="writer">The streamwriter that needs to be used</param>
		public void WriteFile(IMesh type, StreamWriter writer)
		{
			using(writer)
			{
				writer.WriteLine("OFF");
				writer.WriteLine($"{type.VertexCount} {type.FaceCount} 0");
				foreach(Vector3 vertice in type.Vertices)
					writer.WriteLine($"{vertice.X} {vertice.Y} {vertice.Z}");

				foreach(Vector3 face in type.Faces)
					writer.WriteLine($"3 {face.X} {face.Y} {face.Z}");

			}
		}

		/// <summary>
		/// Writes an unstructured mesh to an off file at a given location.
		/// </summary>
		/// <param name="type">The unstructured mesh that needs to be written to an off file</param>
		/// <param name="location">The location of the off file</param>
		public Task WriteFileAsync(IMesh type, string location)
		{
			return Task.Run(() => WriteFile(type, location));
		}

		/// <summary>
		/// Writes an unstructured mesh to an off file with a given streamwriter
		/// </summary>
		/// <param name="type">The unstructured mesh that needs to be written to an off file</param>
		/// <param name="writer">The streamwriter that needs to be used</param>
		public Task WriteFileAsync(IMesh type, StreamWriter writer)
		{
			return Task.Run(() => WriteFile(type, writer));
		}
	}
}
