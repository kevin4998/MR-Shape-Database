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
