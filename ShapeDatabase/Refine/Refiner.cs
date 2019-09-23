using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Refine
{
	public class Refiner
	{
		public static void ExtendMesh(MeshEntry mesh)
		{
			CallJavaScript("doosabin", mesh.Name, mesh.Name.Remove(mesh.Name.Length - 4, 4) + "(Extended).off");
		}

		private static void CallJavaScript(string script, string inputFile, string outputFile)
		{
			string javaPath = @"C:\Program Files (x86)\Common Files\Oracle\Java\javapath\java.exe";
			var processInfo = new ProcessStartInfo($"{javaPath}", $@"-jar {script}.jar {@"..\Shapes\Initial\" + inputFile} {@"..\Shapes\Initial\" + outputFile}")
			{
				CreateNoWindow = true,
				UseShellExecute = false
			};

			processInfo.WorkingDirectory = @"C:\Users\guusd\Documents\UniversiteitUtrecht\M2.1\MR\MR-Shape-Database\ShapeDatabase\Content\Scripts"; // this is where your jar file is.
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
