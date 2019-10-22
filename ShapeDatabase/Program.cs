using System;
using System.Resources;
using ShapeDatabase.Features;
using ShapeDatabase.Properties;

// Define what the default languages is for this application.
// We do NOT define the US or Great Brittain constant on purpose.
[assembly:NeutralResourcesLanguage("en")]
namespace ShapeDatabase {

	class Program {

		static void Main(string[] args) {

			ANN.BuildAndSave();


			Console.WriteLine(Resources.I_StartUp);

			Controller.ProcessArguments(args);

			if (Settings.DirectShutDown)
				return;

			Console.WriteLine(Resources.I_ExitPropmt);
			Console.ReadLine();
		}
	}
}
