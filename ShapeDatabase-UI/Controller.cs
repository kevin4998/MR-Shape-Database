using System.Collections.Generic;
using CommandLine;
using ShapeDatabase.UI.Console.Handlers;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.UI.Properties.Resources;

namespace ShapeDatabase.UI {

	/// <summary>
	/// The main application controller.
	/// This object determines what gets performed and what won't.
	/// </summary>
	public static class Controller {

		/// <summary>
		/// Converts the given console arguments and
		/// starts the application based on their input.
		/// </summary>
		/// <param name="args">The actions which this application should perform.</param>
		public static void ProcessArguments(string[] args) {
			WriteLine(I_StartProc_Input);

			Parser.Default.ParseArguments<CleanOptions, ViewOptions, RefineOptions,
										  MeasureOptions, FeatureOptions, QueryOptions,
										  EvaluateOptions>(args)
				.WithParsed<BaseOptions>(GlobalHandler.Start)
				.WithParsed<CleanOptions>(CleanHandler.Start)
				.WithParsed<ViewOptions>(ViewHandler.Start)
				.WithParsed<RefineOptions>(RefineHandler.Start)
				.WithParsed<MeasureOptions>(MeasureHandler.Start)
				.WithParsed<FeatureOptions>(FeatureHandler.Start)
				.WithParsed<QueryOptions>(QueryHandler.Start)
				.WithParsed<EvaluateOptions>(EvaluateHandler.Start)
				.WithNotParsed(OnErrors);
		}

		/// <summary>
		/// Actions which will be performed on all the exceptions
		/// generated when parsing text values.
		/// </summary>
		/// <param name="errors">A collection of generated errors
		/// by the Parser package.</param>
		static void OnErrors(IEnumerable<Error> errors) {
			foreach (Error error in errors)
				WriteLine(error.ToString());
		}

	}
}
