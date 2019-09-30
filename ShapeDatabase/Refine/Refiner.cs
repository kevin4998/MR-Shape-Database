using System;
using System.Diagnostics;
using System.IO;
using OpenTK;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Refine {

	/// <summary>
	/// Class for extending meshes with too few faces or vertices.
	/// </summary>
	public class ExtendRefiner : IRefiner<UnstructuredMesh> {

		#region --- Properties ---

		private static readonly int MIN_FACES = 100;
		private static readonly int MIN_VERTICES = 100;

		private static readonly Lazy<ExtendRefiner> lazy =
			new Lazy<ExtendRefiner> (() => new ExtendRefiner());

		/// <summary>
		/// Gives a refiner to extend meshes.
		/// </summary>
		public static ExtendRefiner Instance { get { return lazy.Value; } }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new refiner to increase the amount of faces in a shape.
		/// </summary>
		private ExtendRefiner() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Checkes whether mesh has too few faces or vertices.
		/// </summary>
		/// <param name="mesh">The item to check for refinement.</param>
		/// <returns><see langword="true"/> if a refinement operation is needed
		/// for an optimal shape.</returns>
		public bool RequireRefinement(UnstructuredMesh mesh) {
			return mesh.FacesCount < MIN_FACES || mesh.VerticesCount < MIN_VERTICES;
		}

		/// <summary>
		/// Extends mesh by applying one iteration of the Doo-Sabin algorithm.
		/// Overwrites the specified file.
		/// </summary>
		/// <param name="file">The file containing the shape to refine.</param>
		/// <exception cref="ArgumentNullException">If the given file is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given file does not exist.
		/// </exception>
		public void RefineMesh(IWriter<UnstructuredMesh> writer, UnstructuredMesh mesh, FileInfo file, int attemps = 0) {
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (!file.Exists)
				throw new ArgumentException("File {0} does not exist.", file.FullName);

			Refiner.Split4Triangles(mesh, writer, file.FullName);
			Refiner.CallJavaScript("cleanoff", file.FullName, file.FullName, "0.0001");
			Refiner.CallJavaScript("doosabin", file.FullName, file.FullName);
			Refiner.CallJavaScript("tess", file.FullName, file.FullName);
		}

		#endregion

	}

	/// <summary>
	/// Class for simplifying meshes with too few faces or vertices.
	/// </summary>
	public class SimplifyRefiner : IRefiner<UnstructuredMesh> {

		#region --- Properties ---

		private static readonly int MAX_FACES = 5000;
		private static readonly int MAX_VERTICES = 5000;
		public static readonly float RANGE = 0.00001f;

		private static readonly Lazy<SimplifyRefiner> lazy =
			new Lazy<SimplifyRefiner>(() => new SimplifyRefiner());

		/// <summary>
		/// Gives a refiner to simplify meshes.
		/// </summary>
		public static SimplifyRefiner Instance { get { return lazy.Value; } }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new refiner to reduce the amount of faces in a shape.
		/// </summary>
		private SimplifyRefiner() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Checks whether mesh has too many faces or vertices.
		/// </summary>
		/// <param name="mesh">The item to check for refinement.</param>
		/// <returns><see langword="true"/> if the given mesh has too many faces.
		/// </returns>
		public bool RequireRefinement(UnstructuredMesh mesh) {
			return mesh.FacesCount > MAX_FACES || mesh.VerticesCount > MAX_VERTICES;
		}

		/// <summary>
		/// Simplifies mesh by reducing number of faces and triangles. Overwrites the .off file.
		/// </summary>
		/// <param name="file">The file containing the shape to refine.</param>
		/// <exception cref="ArgumentNullException">If the given file is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given file does not exist.
		/// </exception>
		public void RefineMesh(IWriter<UnstructuredMesh> _, UnstructuredMesh __, FileInfo file, int attemps = 0) {
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (!file.Exists)
				throw new ArgumentException("File {0} does not exist.", file.FullName);

			Refiner.CallJavaScript("cleanoff", file.FullName, file.FullName, (RANGE * (attemps + 1)).ToString());
			//Refiner.CallJavaScript("tess", file.FullName, file.FullName);
		}

		#endregion

	}

	/// <summary>
	/// Class for functionality related to mesh refinement.
	/// </summary>
	public class Refiner {

		/// <summary>
		/// Calls a Java script located in the Settings.JavaDir directory. Executes script and overwrites the .off file.
		/// </summary>
		/// <param name="script">Name of the Java script located in the Settings.
		/// JavaDir directory</param>
		/// <param name="inputFileDirectory">Path from Java file to the input off file
		/// </param>
		/// <param name="outputFileDirectory">Path from Java file to the output off file
		/// </param>
		/// <exception cref="ArgumentNullException">If any of the given parameters
		/// is <see langword="null"/> or is <see cref="string.Empty"/>.</exception>
		public static void CallJavaScript(string script, string inputFileDirectory,
														 string outputFileDirectory,
														 string param = "") {
			if (string.IsNullOrEmpty(script))
				throw new ArgumentNullException(nameof(script));
			if (string.IsNullOrEmpty(inputFileDirectory))
				throw new ArgumentNullException(nameof(inputFileDirectory));
			if (string.IsNullOrEmpty(outputFileDirectory))
				throw new ArgumentNullException(nameof(outputFileDirectory));

			string javaPath = Settings.JavaDir;

			ProcessStartInfo processInfo =
				new ProcessStartInfo($"{javaPath}",
				$@"-jar {script}.jar {inputFileDirectory} {outputFileDirectory} {param}")
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				WorkingDirectory = Settings.JavaScriptsDir
			};

			using (Process proc = Process.Start(processInfo)) { 
				proc.WaitForExit();
				int exitCode = proc.ExitCode;
				proc.Close();
			}
		}

		/// <summary>
		/// Method which, given a mesh, splits each triangle in 4 triangles and overwrites the original off file.
		/// </summary>
		/// <param name="mesh">The unstructured mesh of which the triangles should be split</param>
		/// <param name="writer">the writer for overwriting the off file</param>
		/// <param name="location">The location (file.FullName) if the off file</param>
		public static void Split4Triangles(UnstructuredMesh mesh, IWriter<UnstructuredMesh> writer, string location)
		{
			uint[] newElements = new uint[mesh.Elements.Length * 4];
			Vector3[] newVertices = new Vector3[mesh.VerticesCount + mesh.FacesCount * 3];

			for (int i = 0; i < mesh.VerticesCount; i++)
			{
				newVertices[i] = mesh.UnstructuredGrid[i];
			}

			for (int i = 0; i < mesh.Elements.Length; i = i + 3)
			{
				newVertices[mesh.VerticesCount + i] = GetMiddle(mesh.UnstructuredGrid[mesh.Elements[i]], mesh.UnstructuredGrid[mesh.Elements[i + 1]]);
				newVertices[mesh.VerticesCount + i + 1] = GetMiddle(mesh.UnstructuredGrid[mesh.Elements[i + 1]], mesh.UnstructuredGrid[mesh.Elements[i + 2]]);
				newVertices[mesh.VerticesCount + i + 2] = GetMiddle(mesh.UnstructuredGrid[mesh.Elements[i + 2]], mesh.UnstructuredGrid[mesh.Elements[i]]);

				newElements[i * 4] = mesh.Elements[i];
				newElements[i * 4 + 1] = (uint)(mesh.VerticesCount + i);
				newElements[i * 4 + 2] = (uint)(mesh.VerticesCount + i + 2);

				newElements[i * 4 + 3] = (uint)(mesh.VerticesCount + i);
				newElements[i * 4 + 4] = mesh.Elements[i + 1];
				newElements[i * 4 + 5] = (uint)(mesh.VerticesCount + i + 1);

				newElements[i * 4 + 6] = (uint)(mesh.VerticesCount + i + 2);
				newElements[i * 4 + 7] = (uint)(mesh.VerticesCount + i + 1);
				newElements[i * 4 + 8] = mesh.Elements[i + 2];

				newElements[i * 4 + 9] = (uint)(mesh.VerticesCount + i);
				newElements[i * 4 + 10] = (uint)(mesh.VerticesCount + i + 1);
				newElements[i * 4 + 11] = (uint)(mesh.VerticesCount + i + 2);
			}

			UnstructuredMesh result = new UnstructuredMesh(newVertices, newElements);

			writer.WriteFile(result, location);
		}
		
		/// <summary>
		/// Given two vertices, return a vertice that lies exactly inbetween.
		/// </summary>
		/// <param name="vert1">The first vertice</param>
		/// <param name="vert2">The second vertice</param>
		/// <returns></returns>
		private static Vector3 GetMiddle(Vector3 vert1, Vector3 vert2)
		{
			double middleX = (vert1.X + vert2.X) / 2.0;
			double middleY = (vert1.Y + vert2.Y) / 2.0;
			double middleZ = (vert1.Z + vert2.Z) / 2.0;

			return new Vector3((float)middleX, (float)middleY, (float)middleZ);
		}
	}
}
