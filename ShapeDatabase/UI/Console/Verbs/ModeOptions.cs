using System;
using CommandLine;

namespace ShapeDatabase.UI.Console.Verbs {

	/// <summary>
	/// The options to clean and remove all existing files for a new restart.
	/// </summary>
	[Verb("clean", HelpText = "Delete all cached data for a clean restart.")]
	public class CleanOptions {

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
	public class QueryOptions : CalculateOptions { }

	/// <summary>
	/// The options to evaluate the last made query for different metrics.
	/// </summary>
	[Verb("evaluate", HelpText = "Evaluate the precision of querried results.")]
	public class EvaluateOptions : CalculateOptions { }

}
