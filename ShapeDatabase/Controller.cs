﻿using CommandLine;
using ShapeDatabase.Features.Statistics;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShapeDatabase {

	/// <summary>
	/// The main application controller.
	/// This object determines what gets performed and what won't.
	/// </summary>
    public class Controller {


		/// <summary>
		/// Converts the given console arguments and
		/// starts the application based on their input.
		/// </summary>
		/// <param name="args">The actions which this application should perform.</param>
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



		/// <summary>
		/// Refinement mode for the application.
		/// This requests the application to select the given shapes
		/// and refine them to our specified standard.
		/// </summary>
		/// <param name="dirs">The directories containing shapes.</param>
		static void RefineShapes(IEnumerable<string> dirs) {
			Console.WriteLine("Start Processing Meshes.");
			LoadNewFiles(dirs, true);
			Console.WriteLine("Done Processing Meshes.");
			ShowShapeCount();
		}

		/// <summary>
		/// Measurement mode for the application.
		/// This requests the application to analyse the given shapes
		/// and tell us their statistics.
		/// </summary>
		/// <param name="dirs">The directories containing shapes.</param>
		static void MeasureShapes(IEnumerable<string> dirs) {
			Console.WriteLine("Start Measuring Meshes.");
			LoadNewFiles(dirs, false);

			RecordHolder recordHolder = new RecordHolder(
				("Name",	 (MeshEntry entry) => entry.Name),
				("Class",	 (MeshEntry entry) => entry.Class),
				("Vertices", (MeshEntry entry) => entry.Mesh.VertexCount),
				("Faces",	 (MeshEntry entry) => entry.Mesh.FaceCount),
				("",						 _ => ""), // Empty line to seperate values.
				("Min X",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MinX),
				("Min Y",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MinY),
				("Min Z",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MinZ),
				("Max X",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MaxX),
				("Max Y",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MaxY),
				("Max Z",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MaxZ),
				(" ",                        _ => ""), // Empty line to seperate values.
				("Range X",  (MeshEntry entry) => entry.Mesh.GetBoundingBox().Width),
				("Range Y",  (MeshEntry entry) => entry.Mesh.GetBoundingBox().Height),
				("Range Z",  (MeshEntry entry) => entry.Mesh.GetBoundingBox().Depth),
				("Volume",	 (MeshEntry entry) => entry.Mesh.GetBoundingBox().Volume)
			);
			recordHolder.TakeSnapShot(Settings.MeshLibrary);

			Console.WriteLine("Done Measuring Meshes.");

			const string datetimeFormat = "yyyy-MM-dd-HH-mm-ss";
			string filename = recordHolder.SnapshotTime.ToString(datetimeFormat)
							+ "_"
							+ Settings.MeasurementsFile;
			RecordsWriter.Instance.WriteFile(recordHolder, filename);

			Console.WriteLine($"Statistics exported to: {filename}");

			ShowShapeCount();
		}

		/// <summary>
		/// Viewing mode for the application.
		/// This requests the application to read the shapes
		/// and let the user view and analyse them themselves.
		/// </summary>
		/// <param name="dirs">The directories containing shapes.</param>
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



		/// <summary>
		/// Tell the user how many shapes are currently loaded.
		/// </summary>
		static void ShowShapeCount() {
			MeshLibrary meshes = Settings.FileManager.ProcessedMeshes;
			Console.WriteLine($"{meshes.Count} Shapes are availabe:");
			foreach (string name in meshes.Names)
				Console.WriteLine($"\t- {name}");
		}

		/// <summary>
		/// Visualises the specified mesh on the window.
		/// </summary>
		/// <param name="mesh"></param>
		static void RunWindow(IMesh mesh) {
			using (Window window = new Window(800, 600, "Multimedia Retrieval - K. Westerbaan & G. de Jonge", mesh)) {
				window.Run(60.0);
			}
		}

		/// <summary>
		/// Let the application process the given directories for shapes.
		/// </summary>
		/// <param name="dirs">The directories containing shapes.</param>
		/// <param name="refine">If the shapes should be automatically refined.</param>
		static void LoadNewFiles(IEnumerable<string> dirs, bool refine = false) {
			if (refine)
				foreach (string dir in dirs)
					Settings.FileManager.AddDirectory(dir);
			else
				foreach (string dir in dirs)
					Settings.FileManager.AddDirectoryDirect(dir);

		}

		/// <summary>
		/// Restores previously refined and processed files for
		/// visualisation or measurements.
		/// </summary>
		static void LoadFinalFiles() {
			Settings.FileManager.AddDirectoryDirect(Settings.ShapeFinalDir);
		}

	}
}
