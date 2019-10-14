using System;
using ShapeDatabase.Properties;

namespace ShapeDatabase {
	class Program {

		static void Main(string[] args) {
			Console.WriteLine(Resources.I_StartUp);

			Controller.ProcessArguments(args);

			if (Settings.DirectShutDown)
				return;

			Console.WriteLine(Resources.I_ExitPropmt);
			Console.ReadLine();
		}
	}
}
