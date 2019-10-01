using CommandLine;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShapeDatabase
{
    public class Controller {


		public static void ProcessArguments(string[] args) {
			Console.WriteLine("Starting converting input!");

			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(OnErrors)
				.WithParsed(OnParsedValues);
		}


		/// <summary>
		/// Actions which will be performe don the converted Options
		/// object.
		/// </summary>
		/// <param name="options">Parsed Command Line Argument Collection.</param>
		static void OnParsedValues(Options options) {
			// Convert Options value to Settings.
			if (Enum.TryParse(options.Mode, true, out OperationMode mode))
				Settings.Mode = mode;
			Settings.Culture = CultureInfo.GetCultureInfo(options.Culture);
			Settings.ShowDebug = options.DebugMessages;

			Console.WriteLine("Done converting input!");
			// Start program activity.
			LoadFinalFiles();
			switch (Settings.Mode) {
			case OperationMode.REFINE:
				RefineShapes(options.ShapeDirectories);
				break;
			case OperationMode.MEASURE:
				MeasureShapes(options.ShapeDirectories);
				break;
			case OperationMode.VIEW:
				ViewShapes(options.ShapeDirectories);
				break;
			}
		}

		/// <summary>
		/// Actions which will be performed on all the exceptions
		/// generated when parsing text values.
		/// </summary>
		/// <param name="errors">A collection of generated errors
		/// by the Parser package.</param>
		static void OnErrors(IEnumerable<Error> errors) {
			foreach (Error error in errors)
				Console.WriteLine(error.ToString());
		}



		static void RefineShapes(IEnumerable<string> dirs) {
			Console.WriteLine("Start Processing Meshes.");
			LoadNewFiles(dirs, true);
			Console.WriteLine("Done Processing Meshes.");
			ShowShapeCount();
		}

		static void MeasureShapes(IEnumerable<string> dirs) {
			Console.WriteLine("Start Measuring Meshes.");
			LoadNewFiles(dirs, false);
			Console.WriteLine("Done Measuring Meshes.");
			ShowShapeCount();
		}
		
		/// <summary>
		/// A method with the purpose of letting users view and analyse their shapes.
		/// </summary>
		static void ViewShapes(IEnumerable<string> dirs) {
			Console.WriteLine("Start Loading Meshes.");
			LoadNewFiles(dirs, false);
			Console.WriteLine("Done Loading Meshes.");

			MeshLibrary meshes = Settings.FileManager.ProcessedMeshes;
			// Notify the user of their options.
			ShowShapeCount();
			// Ask the user for a specific value.
			while (!Settings.DirectShutDown) {
				Console.WriteLine("\nPlease select a shape, " +
								  "or write down 'stop' to exit the program.");
				string input = Console.ReadLine();

				if (Array.IndexOf(Settings.ExitArguments, input.ToLower()) != -1) {
					Settings.DirectShutDown = true;
					break;
				} else if (meshes.Names.Contains(input)) {
					RunWindow(meshes[input].Mesh);
				} else {
					Console.WriteLine($"Unknown command: {input}");
				}
			}
		}



		static void ShowShapeCount() {
			MeshLibrary meshes = Settings.FileManager.ProcessedMeshes;
			Console.WriteLine($"{meshes.Count} Shapes are availabe:");
			foreach (string name in meshes.Names)
				Console.WriteLine($"\t- {name}");
		}

		static void RunWindow(UnstructuredMesh mesh) {
			using (Window window = new Window(800, 600, "Multimedia Retrieval - K. Westerbaan & G. de Jonge", mesh)) {
				window.Run(60.0);
			}
		}

		static void LoadNewFiles(IEnumerable<string> dirs, bool refine = false) {
			if (refine)
				foreach (string dir in dirs)
					Settings.FileManager.AddDirectory(dir);
			else
				foreach (string dir in dirs)
					Settings.FileManager.AddDirectoryDirect(dir);

		}

		static void LoadFinalFiles() {
			Settings.FileManager.AddDirectoryDirect(Settings.ShapeFinalDir);
		}

	}
}
