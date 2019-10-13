﻿using System;

namespace ShapeDatabase {
	class Program {

		static void Main(string[] args) {
			Console.WriteLine("Starting up!");

			Controller.ProcessArguments(args);

			if (Settings.DirectShutDown)
				return;

			Console.WriteLine("Press enter to exit application.");
			Console.ReadLine();
		}
	}
}
