using System;
using System.Collections.Generic;
using System.Linq;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.UI.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle viewing different shapes.
	/// </summary>
	public static class ViewHandler {

		/// <summary>
		/// A collection of strings which will force the program to stop.
		/// </summary>
		public static ICollection<string> ExitArguments => A_ExitArgs.Split(A_ExitSep.ToCharArray());

		/// <summary>
		/// Checks if the specified argument is the command to exit the application.
		/// </summary>
		/// <param name="argument">The command line which could exit the console
		/// or ask for a mesh.</param>
		/// <returns><see langword="true"/> if it is used to exit the application.
		/// </returns>
		public static bool IsExitArgument(string argument) {
			return argument != null
				&& ExitArguments.Contains(argument.ToLower(Settings.Culture));
		}


		/// <summary>
		/// The operation to view refined and unrefined shapes.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(ViewOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			// Load in new shapes to view.
			if (options.HasDirectories)
				foreach(string dir in options.ShapeDirectories)
					Settings.FileManager.AddDirectoryDirect(dir);
			// Notify the user of their options.
			ShowShapeCount();
			// Prompt them to choose a file.
			DisplayShapes();
		}


		/// <summary>
		/// Tell the user how many shapes are currently loaded.
		/// </summary>
		private static void ShowShapeCount() {
			MeshLibrary meshes = Settings.FileManager.ProcessedMeshes;
			WriteLine(I_ShapeCount, meshes.Count);
			foreach (string name in meshes.Names)
				WriteLine($"\t- {name}");
		}

		/// <summary>
		/// Viewing mode for the application.
		/// This requests the application to read the shapes
		/// and let the user view and analyse them themselves.
		/// </summary>
		private static void DisplayShapes() {
			MeshLibrary meshes = Settings.FileManager.ProcessedMeshes;
			Settings.DirectShutDown = false;
			do {
				WriteLine();    // Empty line for clearance.
				WriteLine(I_ShapeSelect_Prompt, ExitArguments.FirstOrDefault());

				string input = ReadLine();
				if (IsExitArgument(input))
					Settings.DirectShutDown = true;
				else if (meshes.Names.Contains(input))
					RunWindow(meshes[input].Mesh);
				else
					WriteLine(I_UnknownCommand, input);
			} while (!Settings.DirectShutDown);
		}

		/// <summary>
		/// Visualises the specified mesh on the window.
		/// </summary>
		/// <param name="mesh"></param>
		private static void RunWindow(IMesh mesh) {
			using (Window.Window window = new Window.Window(800, 600, A_WindowName, mesh)) {
				window.Run(60.0);
			}
		}

	}

}
