using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace ShapeDatabase {
	
	/// <summary>
	/// A custom options class for easier command line argument handling.
	/// </summary>
	class Options {

		/// <summary>
		/// A collection of different directories on the computer to find shapes.
		/// </summary>
		[Option('d', "directory",
			Required = true,
			Default = new string[] { "Content/Shapes/Initial" },
			HelpText = "The directories containing all the Shapes.")]
		public IEnumerable<string> ShapeDirectories { get; set; }

		/// <summary>
		/// The process of operations that this application should perform.
		/// </summary>
		[Option('m', "mode",
			Required = false,
			Default = "VIEW",
			HelpText = "A property to define what should happen to the specified shapes." +
			"\nSupported Modes are: REFINE, MEASURE, VIEW")]
		public string Mode { get; set; }

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
		/// Describes if debug messages should be shown in the console.
		/// </summary>
		[Option('a', "all",
			Required = false,
			Default = false,
			HelpText = "If debug messages should be shown in console.")]
		public bool DebugMessages { get; set; }

		/// <summary>
		/// Describes if previously saved data for speed should be kept during this
		/// execution.
		/// </summary>
		[Option("clean",
			Required = false,
			Default = false,
			HelpText = "If the cached should be removed as if it was a clean install.")]
		public bool CleanStart { get; set; }

		/// <summary>
		/// A collection of examples on how to use this application.
		/// Easier for learning where to find your libraries.
		/// </summary>
		[Usage(ApplicationAlias = "Shape Database")]
		public static IEnumerable<Example> Examples {
			get {
				yield return new Example("Initial Library", new Options() { ShapeDirectories = new string[] { "Content/Shapes/Initial" } });
				yield return new Example("Small Library",	new Options() { ShapeDirectories = new string[] { "Content/Shapes/Small" } });
				yield return new Example("All Libraries",	new Options() { ShapeDirectories = new string[] { "Content/Shapes/Initial", "Content/Shapes/Original" } });
			}
		}

	}

	/// <summary>
	/// Different execution processes which this application can follow.
	/// </summary>
	[Flags]
	public enum OperationMode {

		/// <summary>
		/// Describes that the behaviour is not specified.
		/// </summary>
		NONE = 0,
		/// <summary>
		/// Describes that the user wants to view the shapes.
		/// </summary>
		VIEW = 1,
		/// <summary>
		/// Describes that the application should refine these shapes.
		/// </summary>
		REFINE = 2,
		/// <summary>
		/// Describes that the application should export measurements from the shapes.
		/// </summary>
		MEASURE = 4,
		/// <summary>
		/// Describes that the user wants to extract features from the shapes.
		/// </summary>
		FEATURES = 8,
	}

}
