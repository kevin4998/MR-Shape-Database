using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

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

		/// <summary>
		/// Display help information on how to use this verb.
		/// </summary>
		[Usage]
		public static IEnumerable<Example> Examples {
			get {
				yield return new Example("Clean the application including settings files.", new CleanOptions() { CleanSettings = true });
				yield return new Example("Clean the application excluding settings files.", new CleanOptions() { CleanSettings = false });
				yield return new Example("Clean the application and continue.", new CleanOptions() { CleanSettings = false, AutoExit = true, DebugMessages = true });
			}
		}

	}

	/// <summary>
	/// The options to display a collection of shapes to the user.
	/// </summary>
	[Verb("view", HelpText = "View the different Shapes in the system.")]
	public class ViewOptions : ShapeOptions {

		/// <summary>
		/// Display help information on how to use this verb.
		/// </summary>
		[Usage]
		public static IEnumerable<Example> Examples {
			get {
				yield return new Example("View the unrefined demo files.", new ViewOptions() { ShapeDirectories = new string[] { "Content/Shapes/Demo" } });
				yield return new Example("View the unrefined small files.", new ViewOptions() { ShapeDirectories = new string[] { "Content/Shapes/Small" } });
				yield return new Example("View the previously refined files.", new ViewOptions() { });
			}
		}

	}

	/// <summary>
	/// The options to refine a collection of shapes for the user.
	/// </summary>
	[Verb("refine", HelpText = "Refine provided shapes and stores them in the system.")]
	public class RefineOptions : ShapeOptions {

		/// <summary>
		/// Display help information on how to use this verb.
		/// </summary>
		[Usage]
		public static IEnumerable<Example> Examples {
			get {
				yield return new Example("Refine all the files in the demo map, overwriting existing ones.",
										new RefineOptions() { ShapeDirectories = new string[] { "Content/Shapes/Demo" }, Overwrite = true });
				yield return new Example("Refine all the files in the all map.", new RefineOptions() { ShapeDirectories = new string[] { "Content/Shapes/All" } });
			}
		}

	}

	/// <summary>
	/// The options to calculate metrics for the current shapes.
	/// </summary>
	[Verb("measure", HelpText = "Calculate metrics for all the current shapes.")]
	public class MeasureOptions : ShapeOptions {

		/// <summary>
		/// Display help information on how to use this verb.
		/// </summary>
		[Usage]
		public static IEnumerable<Example> Examples {
			get {
				yield return new Example("Measure the unrefined files in the Demo directory, overwriting refined variants.",
										new MeasureOptions() { ShapeDirectories = new string[] { "Content/Shapes/Demo" }, Overwrite = true });
				yield return new Example("Measure all the refined files.", new MeasureOptions() { });
			}
		}

	}

	/// <summary>
	/// The options to calculate features for the current shapes for comparison.
	/// </summary>
	[Verb("feature", HelpText = "Calculate features for all the current shapes.")]
	public class FeatureOptions : CalculateOptions {

		/// <summary>
		/// Display help information on how to use this verb.
		/// </summary>
		[Usage]
		public static IEnumerable<Example> Examples {
			get {
				yield return new Example("Calculate the features from the refined files and exit the application.",
										new FeatureOptions() { AutoExit = true });
				yield return new Example("Calculate the features from the refined files but do not save it.", new FeatureOptions() { Export = false });
			}
		}

	}

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
		/// Display help information on how to use this verb.
		/// </summary>
		[Usage]
		public static IEnumerable<Example> Examples {
			get {
				yield return new Example("Compare all the items in the database with each other with the query size being the size of the class.",
										new QueryOptions() { AutoExit = true, QueryInput = "internal", QuerySize = "class" });
				yield return new Example("Compare with items from the query folder returning the amount of items as specified in the settings.ini file.",
										new QueryOptions() { AutoExit = true, QueryInput = "refine", QuerySize = "kbest" });
			}
		}

	}

	/// <summary>
	/// The options to evaluate the last made query for different metrics.
	/// </summary>
	[Verb("evaluate", HelpText = "Evaluate the precision of querried results.")]
	public class EvaluateOptions : CalculateOptions {

		/// <summary>
		/// The mode which is used to determine the outputed query results.
		/// </summary>
		public EvaluationMode EvaluationMode => GetMode<EvaluationMode>(Evaluation);

		/// <summary>
		/// The mode which is used to determine the outputed query results.
		/// </summary>
		[Option("evalmode",
			Required = false,
			Default = "Individual",
			HelpText = "The mode which is used to determine the outputed query results.")]
		public string Evaluation { get; set; }

		/// <summary>
		/// Display help information on how to use this verb.
		/// </summary>
		[Usage]
		public static IEnumerable<Example> Examples {
			get {
				yield return new Example("Evaluate the last query results showing the metrics for each individual file.",
										new EvaluateOptions() { AutoExit = true, Evaluation = "individual" });
				yield return new Example("Evaluate the last query results showing the metrics per class.",
										new EvaluateOptions() { AutoExit = true, Evaluation = "aggregated" });
			}
		}

	}

}
