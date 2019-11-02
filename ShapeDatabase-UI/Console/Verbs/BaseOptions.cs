using System;
using System.Collections.Generic;
using CommandLine;

namespace ShapeDatabase.UI.Console.Verbs {

	/// <summary>
	/// A collection of global properties for all options.
	/// </summary>
	public abstract class BaseOptions {

		/// <summary>
		/// If the culture for this application is specified.
		/// </summary>
		public bool HasCulture => Culture != null;

		/// <summary>
		/// The culture in which the files were written.
		/// Needed for the conversion process.
		/// </summary>
		[Option('c', "culture",
			Required = false,
			Default  = null,
			HelpText = "The culture in which the files have been written.")]
		public string Culture { get; set; }

		/// <summary>
		/// Describes if debug messages should be shown in the console.
		/// </summary>
		[Option('a', "all",
			Required = false,
			Default  = false,
			HelpText = "If debug messages should be shown in console.")]
		public bool DebugMessages { get; set; }

		/// <summary>
		/// Describes if the application should automatically exit when it is done.
		/// </summary>
		[Option("exit",
			Required = false,
			Default  = false,
			HelpText = "If the application should automatically exit when it is done.")]
		public bool AutoExit { get; set; }

	}

	/// <summary>
	/// Properties that work on options which make use of newly loaded shapes.
	/// </summary>
	public abstract class ShapeOptions : BaseOptions {

		/// <summary>
		/// If any directories with shapes has been specified.
		/// </summary>
		public bool HasDirectories {
			get {
				IEnumerable<string> dirs = ShapeDirectories;
				if (dirs == null) return false;
				foreach (string _ in dirs) return true;
				return false;
			}
		}

		/// <summary>
		/// A collection of different directories on the computer to find shapes.
		/// </summary>
		[Option('d', "directory",
			Required = false,
			Default = new string[0],
			HelpText = "The directories containing all the Shapes.")]
		public IEnumerable<string> ShapeDirectories { get; set; }

		/// <summary>
		/// If new shapes should overwrite the cached variants.
		/// </summary>
		[Option('o', "overwrite",
			Required = false,
			Default = true,
			HelpText = "If new shapes should overwrite cached ones.")]
		public bool Overwrite { get; set; }


	}

	/// <summary>
	/// Properties that work on options which need to calculate new results based
	/// of previous operations.
	/// </summary>
	public abstract class CalculateOptions : BaseOptions {

		/// <summary>
		/// Describes if previously stored data should be loaded.
		/// </summary>
		public bool ShouldImport => Import ?? Settings.UseCacheData;
		/// <summary>
		/// Describes if the results from the operations should be saved to a file.
		/// </summary>
		public bool ShouldExport => Export ?? Settings.UseCacheData;

		/// <summary>
		/// Describes if previously stored data should be loaded.
		/// Classes should use <see cref="ShouldImport"/> over this method.
		/// </summary>
		[Option('i', "import",
			Required = false,
			Default  = null,
			HelpText = "If previously stored data should be loaded.")]
		public bool? Import { get; set; }

		/// <summary>
		/// Describes if the results from the operations should be saved to a file.
		/// Classes should use <see cref="ShouldExport"/> over this method.
		/// </summary>
		[Option('e', "export",
			Required = false,
			Default  = null,
			HelpText = "If the settings file should be cleaned.")]
		public bool? Export { get; set; }

	}

}
