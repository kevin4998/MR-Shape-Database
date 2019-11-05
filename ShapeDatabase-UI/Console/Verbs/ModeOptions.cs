using System;
using System.Collections.Generic;
using CommandLine;
using ShapeDatabase.UI.Properties;
using ShapeDatabase.Util;

namespace ShapeDatabase.UI.Console.Verbs {

	/// <summary>
	/// The options to clean and remove all existing files for a new restart.
	/// </summary>
	[Verb("clean", HelpText = "Delete all cached data for a clean restart.")]
	public class CleanOptions : BaseOptions {

		/// <summary>
		/// Descibes if the settings file should be cleaned.
		/// </summary>
		[Option("settings",
			Required = false,
			Default  = false,
			HelpText = "If the settings file should be cleaned.")]
		public bool CleanSettings { get; set; }

	}

	/// <summary>
	/// The options to display a collection of shapes to the user.
	/// </summary>
	[Verb("view", HelpText = "View the different Shapes in the system.")]
	public class ViewOptions : ShapeOptions { }

	/// <summary>
	/// The options to refine a collection of shapes for the user.
	/// </summary>
	[Verb("refine", HelpText = "Refine provided shapes and stores them in the system.")]
	public class RefineOptions : ShapeOptions { }

	/// <summary>
	/// The options to calculate metrics for the current shapes.
	/// </summary>
	[Verb("measure", HelpText = "Calculate metrics for all the current shapes.")]
	public class MeasureOptions : ShapeOptions { }

	/// <summary>
	/// The options to calculate features for the current shapes for comparison.
	/// </summary>
	[Verb("feature", HelpText = "Calculate features for all the current shapes.")]
	public class FeatureOptions : CalculateOptions { }

	/// <summary>
	/// The options to compare shapes for similarity.
	/// </summary>
	[Verb("query", HelpText = "Compare existing shapes with query shapes to find the most similar.")]
	public class QueryOptions : CalculateOptions {

		/// <summary>
		/// If any directories with shapes has been specified.
		/// </summary>
		public bool HasDirectories {
			get {
				IEnumerable<string> dirs = QueryDirectories;
				if (dirs == null)
					return false;
				foreach (string _ in dirs)
					return true;
				return false;
			}
		}

		/// <summary>
		/// A collection of different directories on the computer to find query shapes.
		/// </summary>
		[Option('d', "directory",
			Required = false,
			Default = new string[0],
			HelpText = "The directories containing all the Shapes.")]
		public IEnumerable<string> QueryDirectories { get; set; }


		/// <summary>
		/// The mode which is used to determine which shapes to query with.
		/// </summary>
		public QueryInputMode QueryInputMode => GetMode<QueryInputMode>(QueryInput);
		/// <summary>
		/// The mode which is used to determine which shapes to query with.
		/// </summary>
		public QuerySizeMode QuerySizeMode => GetMode<QuerySizeMode>(QuerySize);

		/// <summary>
		/// The mode which is used to determine which shapes to query with.
		/// </summary>
		[Option("queryinput",
			Required = false,
			Default = "Refine",
			HelpText = "The mode which is used to determine which shapes to query with.")]
		public string QueryInput { get; set; }

		/// <summary>
		/// The mode which is used to determine the query size.
		/// </summary>
		[Option("querysize",
			Required = false,
			Default = "KBest",
			HelpText = "The mode which is used to determine the query size.")]
		public string QuerySize { get; set; }

		/// <summary>
		/// The mode which is used to determine the query results.
		/// </summary>
		[Option("queryresult",
			Required = false,
			Default = "Individual",
			HelpText = "The mode which is used to determine the query results.")]
		public string QueryResult { get; set; }

	}

	/// <summary>
	/// The options to evaluate the last made query for different metrics.
	/// </summary>
	[Verb("evaluate", HelpText = "Evaluate the precision of querried results.")]
	public class EvaluateOptions : CalculateOptions { }

}
