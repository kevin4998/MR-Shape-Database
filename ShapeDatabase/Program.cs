using System;

namespace ShapeDatabase {
	class Program {

		static void Main(string[] args) {
			Console.WriteLine("Starting up!");

			Controller.ProcessArguments(args);
			Controller.RunWindow();

			if (Settings.DirectShutDown)
				return;

			Console.WriteLine("Press any key to exit application.");
			Console.ReadLine();
		}

	}

}
