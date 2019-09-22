using System;
using System.Collections.Generic;
using System.Globalization;
using CommandLine;
using CommandLine.Text;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI;

namespace ShapeDatabase {
	class Program {

		private static FileManager FileManager { get; set; }

		static void Main(string[] args) {
			Console.WriteLine("Starting up!");

			FileManager = new FileManager();

			Console.WriteLine("Starting converting input!");
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(OnErrors)
				.WithParsed(OnParsedValues);
			Console.WriteLine("Done converting input!");

			Controller controller = new Controller();

			Console.WriteLine("Press any key to exit application.");
			Console.ReadLine();
		}

		/// <summary>
		/// Actions which will be performe don the converted Options
		/// object.
		/// </summary>
		/// <param name="options">Parsed Command Line Argument Collection.</param>
		static void OnParsedValues(Options options) {
			Settings.Culture = CultureInfo.GetCultureInfo(options.Culture);

			Console.WriteLine("Start Processing Meshes.");
			foreach (string dir in options.ShapeDirectories)
				FileManager.AddDirectory(dir);
			MeshLibrary meshes = FileManager.ProcessedMeshes;
			Console.WriteLine($"Shape Count:{meshes.Count}");
			foreach (string name in meshes.Names)
				Console.WriteLine($"\t- {name}");
			Console.WriteLine("Done Processing Meshes.");
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

	/// <summary>
	/// A custom options class for easier command line argument handling.
	/// </summary>
	class Options {

		/// <summary>
		/// A collection of different directories on the computer to find shapes.
		/// </summary>
		[Option('d', "directory",
			Required = true,
			Default = new string[] {"Content/Shapes/Initial"},
			HelpText = "The directories containing all the Shapes.")]
		public IEnumerable<string> ShapeDirectories { get; set; }

		/// <summary>
		/// The culture in which the files were written.
		/// Needed for the conversion process.
		/// </summary>
		[Option('c', "culture",
			Required = false,
			Default = "en",
			HelpText = "The culture in which the files have been written.")]
		public string Culture { get; set; }

		/// <summary>
		/// A collection of examples on how to use this application.
		/// Easier for learning where to find your libraries.
		/// </summary>
		[Usage(ApplicationAlias = "Shape Database")]
		public static IEnumerable<Example> Examples {
			get {
				yield return new Example("Initial Library", new Options() { ShapeDirectories = new string[] { "Content/Shapes/Initial" } });
				yield return new Example("Small Library",	new Options() { ShapeDirectories = new string[] { "Content/Shapes/Small" } });
				yield return new Example("All Library",		new Options() { ShapeDirectories = new string[] { "Content/Shapes/Initial", "Content/Shapes/Original" } });
			}
		}

	}

}
