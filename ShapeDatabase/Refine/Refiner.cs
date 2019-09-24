using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI;

namespace ShapeDatabase.Refine
{
	public class Refiner
	{
		/// <summary>
		/// Extends an off file by increasing the number of faces using the Doo-Sabin algorithm.
		/// </summary>
		/// <param name="inputFileDirectory">Path from Java file to the input off file</param>
		/// <param name="outputFileDirectory">Path from Java file to the output off file</param>
		public static void ExtendMesh(string inputFileDirectory, string outputFileDirectory)
		{
			CallJavaScript("doosabin", inputFileDirectory, outputFileDirectory);
		}

		/// <summary>
		/// Simplifies an off file by decreasing the number of faces.
		/// </summary>
		/// <param name="inputFileDirectory">Path from Java file to the input off file</param>
		/// <param name="outputFileDirectory">Path from Java file to the output off file</param>
		public static void SimplifyMesh(string inputFileDirectory, string outputFileDirectory)
		{
			CallJavaScript("cleanoff", inputFileDirectory, outputFileDirectory);
		}

		/// <summary>
		/// Turns all faces of an off file into triangles.
		/// </summary>
		/// <param name="inputFileDirectory">Path from Java file to the input off file</param>
		/// <param name="outputFileDirectory">Path from Java file to the output off file</param>
		public static void MakeTriangles(string inputFileDirectory, string outputFileDirectory)
		{
			CallJavaScript("tess", inputFileDirectory, outputFileDirectory);
		}

		/// <summary>
		/// Calls a Java script located in the Settings.JavaDir directory.
		/// </summary>
		/// <param name="inputFileDirectory">Name of the Java script located in the Settings.JavaDir directory</param>
		/// <param name="inputFileDirectory">Path from Java file to the input off file</param>
		/// <param name="outputFileDirectory">Path from Java file to the output off file</param>
		private static void CallJavaScript(string script, string inputFileDirectory, string outputFileDirectory)
		{
			string javaPath = Settings.JavaDir;

			var processInfo = new ProcessStartInfo($"{javaPath}", $@"-jar {script}.jar {inputFileDirectory} {outputFileDirectory}")
			{
				CreateNoWindow = true,
				UseShellExecute = false
			};

			processInfo.WorkingDirectory = Settings.JavaScriptsDir;
			Process proc;
			proc = Process.Start(processInfo);
			proc.WaitForExit();
			int exitCode = proc.ExitCode;
			proc.Close();
		}
	}
}
