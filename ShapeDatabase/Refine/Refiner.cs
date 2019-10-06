using System;
using System.Diagnostics;
using System.IO;
using g3;
using OpenTK;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Refine {

	/// <summary>
	/// Class for extending meshes with too few faces or vertices.
	/// </summary>
	public class ExtendRefiner : IRefiner<Shapes.IMesh> {

		#region --- Properties ---

		private static readonly int DESIRED_VERTICES = 5000;

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
		/// <param name="mesh">The mesh to check for refinement.</param>
		/// <returns><see langword="true"/> if a refinement operation is needed
		/// for an optimal shape.</returns>
		public bool RequireRefinement(Shapes.IMesh mesh)
		{
			return mesh.VertexCount != DESIRED_VERTICES;
		}
		/// <summary>
		/// Extends mesh by applying one iteration of the Doo-Sabin algorithm.
		/// Overwrites the specified file.
		/// </summary>
		/// <param name="mesh">The mesh that needs to be refined.</param>
		/// <param name="file">The file containing the shape to refine.</param>
		/// <exception cref="ArgumentNullException">If the given file is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given file does not exist.
		/// </exception>
		public void RefineMesh(Shapes.IMesh mesh, FileInfo file) {
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (!file.Exists)
				throw new ArgumentException("File {0} does not exist.", file.FullName);

			DMesh3 meshDMesh3 = (DMesh3)((GeometryMesh)mesh).Base;

			Remesher remesher = new Remesher(meshDMesh3) {
				PreventNormalFlips = true
			};

			float z = 0.025F;
			while (meshDMesh3.VertexCount < 5000)
			{
				remesher.SetTargetEdgeLength(z);
				for (int k = 0; k < 20; ++k)
				{
					remesher.BasicRemeshPass();
				}

				z *= 0.5F;
			}

			Reducer reducer = new Reducer(meshDMesh3);
			reducer.ReduceToVertexCount(5000);
			IO.OFFWriter.Instance.WriteFile((GeometryMesh) meshDMesh3, file.FullName);
		}

		#endregion

	}

	/// <summary>
	/// Class for simplifying meshes with too few faces or vertices.
	/// </summary>
	public class SimplifyRefiner : IRefiner<Shapes.IMesh> {

		#region --- Properties ---

		private static readonly int DESIRED_VERTICES = 5000;

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
		/// <param name="mesh">The mesh to check for refinement.</param>
		/// <returns><see langword="true"/> if the given mesh has too many faces.
		/// </returns>
		public bool RequireRefinement(Shapes.IMesh mesh) {
			return mesh.VertexCount != DESIRED_VERTICES;
		}

		/// <summary>
		/// Simplifies mesh by reducing number of faces and triangles. Overwrites the .off file.
		/// </summary>
		/// /// <param name="mesh">The mesh that needs to be refined.</param>
		/// <param name="file">The file containing the shape to refine.</param>
		/// <exception cref="ArgumentNullException">If the given file is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given file does not exist.
		/// </exception>
		/// 
		public void RefineMesh(Shapes.IMesh mesh, FileInfo file)
		{
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (!file.Exists)
				throw new ArgumentException("File {0} does not exist.", file.FullName);

			DMesh3 meshDMesh3 = mesh as GeometryMesh;

			Reducer reducer = new Reducer(meshDMesh3);
			reducer.ReduceToVertexCount(5000);
			IO.OFFWriter.Instance.WriteFile((GeometryMesh) meshDMesh3, file.FullName);
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
	}
}
