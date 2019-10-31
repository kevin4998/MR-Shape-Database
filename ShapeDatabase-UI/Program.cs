﻿using ShapeDatabase.UI.Properties;
using static System.Console;

namespace ShapeDatabase.UI {
	class Program {
		static void Main(string[] args) {
			WriteLine(Resources.I_StartUp);

			Controller.ProcessArguments(args);

			if (Settings.DirectShutDown)
				return;

			WriteLine(Resources.I_ExitPropmt);
			ReadLine();
		}
	}
}