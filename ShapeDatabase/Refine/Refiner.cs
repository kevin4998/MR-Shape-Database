using System;
using System.Diagnostics;
using System.IO;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Refine
{
	/// <summary>
	/// Class for extending meshes with too few faces or vertices.
	/// </summary>
	public class ExtendRefiner : IRefiner<UnstructuredMesh>
	{
		private static readonly Lazy<ExtendRefiner> lazy =
			new Lazy<ExtendRefiner> (() => new ExtendRefiner());

		public static ExtendRefiner Instance { get { return lazy.Value; } }

		private ExtendRefiner() { }
	
		/// <summary>
		/// Checkes whether mesh has too few faces or vertices.
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public bool RequireRefinement(UnstructuredMesh mesh)
		{
			return (mesh.FacesCount < 100 || mesh.VerticesCount < 100);
		}

		/// <summary>
		/// Extends mesh by applying one iteration of the Doo-Sabin algorithm. Overwrites the .off file.
		/// </summary>
		/// <param name="file"></param>
		public void RefineMesh(FileInfo file)
		{
			Refiner.CallJavaScript("doosabin", file.FullName, file.FullName);
			Refiner.CallJavaScript("tess", file.FullName, file.FullName);
		}
	}

	/// <summary>
	/// Class for simplifying meshes with too few faces or vertices.
	/// </summary>
	public class SimplifyRefiner : IRefiner<UnstructuredMesh>
	{
		private static readonly Lazy<SimplifyRefiner> lazy =
			new Lazy<SimplifyRefiner>(() => new SimplifyRefiner());

		public static SimplifyRefiner Instance { get { return lazy.Value; } }

		private SimplifyRefiner() { }

		/// <summary>
		/// Checks whether mesh has too many faces or vertices.
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public bool RequireRefinement(UnstructuredMesh mesh)
		{
			return (mesh.FacesCount > 5000 || mesh.VerticesCount > 5000);
		}

		/// <summary>
		/// Simplifies mesh by reducing number of faces and triangles. Overwrites the .off file.
		/// </summary>
		/// <param name="file"></param>
		public void RefineMesh(FileInfo file)
		{
			Refiner.CallJavaScript("cleanoff", file.FullName, file.FullName);
			Refiner.CallJavaScript("tess", file.FullName, file.FullName);
		}
	}

	/// <summary>
	/// Class for functionality related to mesh refinement.
	/// </summary>
	public class Refiner
	{
		/// <summary>
		/// Calls a Java script located in the Settings.JavaDir directory. Executes script and overwrites the .off file.
		/// </summary>
		/// <param name="script">Name of the Java script located in the Settings.JavaDir directory</param>
		/// <param name="inputFileDirectory">Path from Java file to the input off file</param>
		/// <param name="outputFileDirectory">Path from Java file to the output off file</param>
		public static void CallJavaScript(string script, string inputFileDirectory, string outputFileDirectory)
		{
			string javaPath = Settings.JavaDir;

			ProcessStartInfo processInfo = new ProcessStartInfo($"{javaPath}", $@"-jar {script}.jar {inputFileDirectory} {outputFileDirectory}")
			{
				CreateNoWindow = true,
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
