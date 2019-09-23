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
		public static void ExtendMesh(MeshEntry mesh, bool overwrite)
		{
			CallJavaScript("doosabin", mesh.Name, overwrite);
		}

		public static void SimplyMesh(MeshEntry mesh, bool overwrite)
		{
			CallJavaScript("cleanoff", mesh.Name, overwrite);
		}

		public static void MakeTriangles(MeshEntry mesh, bool overwrite)
		{
			CallJavaScript("tess", mesh.Name, overwrite);
		}

		private static void CallJavaScript(string script, string inputFile, bool overwrite)
		{
			string javaPath = Settings.JavaDir;
			string outputFile = overwrite ? inputFile : inputFile.Remove(inputFile.Length - 4, 4) + "(Extended).off";
			var processInfo = new ProcessStartInfo($"{javaPath}", $@"-jar {script}.jar {@"..\Shapes\Initial\" + inputFile} {@"..\Shapes\Initial\" + outputFile}")
			{
				CreateNoWindow = true,
				UseShellExecute = false
			};

			processInfo.WorkingDirectory = Settings.JavaScriptsDir; // this is where your jar file is.
			Process proc;

			if ((proc = Process.Start(processInfo)) == null)
			{
				throw new InvalidOperationException("??");
			}

			proc.WaitForExit();
			int exitCode = proc.ExitCode;
			proc.Close();
		}
	}
}
