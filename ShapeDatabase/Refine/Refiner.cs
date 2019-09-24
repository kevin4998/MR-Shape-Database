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
		public static void ExtendMesh(string inputFileDirectory, string outputFileDirectory)
		{
			CallJavaScript("doosabin", inputFileDirectory, outputFileDirectory);
		}

		public static void SimplifyMesh(string inputFileDirectory, string outputFileDirectory)
		{
			CallJavaScript("cleanoff", inputFileDirectory, outputFileDirectory);
		}

		public static void MakeTriangles(string inputFileDirectory, string outputFileDirectory)
		{
			CallJavaScript("tess", inputFileDirectory, outputFileDirectory);
		}

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
