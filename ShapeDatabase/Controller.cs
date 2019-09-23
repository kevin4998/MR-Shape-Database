﻿using CommandLine;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShapeDatabase
{
    public class Controller {

		public static void RunWindow() {
			using (Window window = new Window(800, 600, "Multimedia Retrieval - K. Westerbaan & G. de Jonge")) {
				window.Run(60.0);
			}
		}

		public static void ProcessArguments(string[] args) {
			Console.WriteLine("Starting converting input!");

			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(OnErrors)
				.WithParsed(OnParsedValues);

			Console.WriteLine("Done converting input!");
		}

		public static void ProcessFiles(IEnumerable<string> dirs) {
			Console.WriteLine("Start Processing Meshes.");

			foreach (string dir in dirs)
				Settings.FileManager.AddDirectory(dir);

			MeshLibrary meshes = Settings.FileManager.ProcessedMeshes;
			Console.WriteLine($"Shape Count:{meshes.Count}");
			foreach (string name in meshes.Names)
				Console.WriteLine($"\t- {name}");

			Console.WriteLine("Done Processing Meshes.");
		}


		/// <summary>
		/// Actions which will be performe don the converted Options
		/// object.
		/// </summary>
		/// <param name="options">Parsed Command Line Argument Collection.</param>
		static void OnParsedValues(Options options) {
			Settings.Culture = CultureInfo.GetCultureInfo(options.Culture);
			Settings.ShowDebug = options.DebugMessages;

			ProcessFiles(options.ShapeDirectories);
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

	}
}