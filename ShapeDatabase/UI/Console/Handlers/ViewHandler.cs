using System;
using System.Linq;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle viewing different shapes.
	/// </summary>
	public static class ViewHandler {


		/// <summary>
		/// The operation to view refined and unrefined shapes.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static int Start(ViewOptions options) {
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
			return 0;
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
				WriteLine(I_ShapeSelect_Prompt, Settings.ExitArguments.FirstOrDefault());

				string input = ReadLine();
				if (Settings.ExitArguments.Contains(input.ToLower(Settings.Culture)))
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
			using (Window window = new Window(800, 600, A_WindowName, mesh)) {
				window.Run(60.0);
			}
		}

	}

}
