using System;
using System.Collections.Generic;
using System.IO;
using ShapeDatabase.Features.Statistics;
using ShapeDatabase.IO;
using ShapeDatabase.Query;
using ShapeDatabase.UI.Console.Verbs;
using ShapeDatabase.Util;
using static ShapeDatabase.UI.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle evaluating the results of different querries.
	/// </summary>
	public static class EvaluateHandler {

		private static IRecordHolder<QueryResult> EvaluationCalculator {
			get {
				ICachedRecordHolder<QueryResult> recordHolder =
				new CachedRecordHolder<QueryResult>(NameProvider);
				// Cache values to identify the True Positive, etc. grid.
				recordHolder.Cache
					.AddLazyValue(
						("Total", () => CacheTotal())
					).AddLazyValue(
						("Relevant", result => CacheRelevant(result)),
						("Correct", result => result.Count),
						("TP", result => CacheTP(result))
					).AddLazyValue(
						("Irrelevant", (result, cache) => CacheIrrelevant(result, cache)),
						("Incorrect", (result, cache) => CacheIncorrect(result, cache)),

						("FP", (result, cache) => CacheFP(result, cache)),
						("FN", (result, cache) => CacheFN(result, cache)),
						("TN", (result, cache) => CacheTN(result, cache))
					);
				// Calculate the Metrics.
				recordHolder.AddMeasure(
					("Class",		(result, _)		=> ClassProvider(result)),
					("Accuracy",	(result, cache) => Accuracy		(result, cache)),
					("Precision",	(result, cache) => Precision	(result, cache)),
					("Recall",		(result, cache) => Recall		(result, cache)),
					("Specificity", (result, cache) => Specificity	(result, cache)),
					("Sensitivity", (result, cache) => Sensitivity	(result, cache)),
					("F1",			(result, cache) => F1			(result, cache))
				);
				// Return the Record Holder.
				return recordHolder;
			}
		}
		private static RecordMerger RecordMerger =>
			new RecordMerger(ClassRecord)
					.AddMeasure<int>(c => Average(c))
					.AddMeasure<double>(c => AverageDouble(c))
					.AddMeasure("Class", c => ClassChoice(c));


		/// <summary>
		/// The operation to evaluate the result of a query.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(EvaluateOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Logger.Log(I_StartProc_Evaluate);

			IRecordHolder<QueryResult> records = EvaluationCalculator;
			QueryResult[] results = QueryHandler.LoadQueryResults(options.ShouldImport);
			records.TakeSnapShot(results);

			IRecordHolder holder = null;
			switch (options.EvaluationMode) {
			case EvaluationMode.Individual:
				holder = records;
				break;
			case EvaluationMode.Aggregated:
				holder = RecordMerger.Merge(records);
				break;
			}

			if (options.ShouldExport)
				SaveEvaluation(holder);

			Logger.Log(I_EndProc_Evaluate);
		}


		/// <summary>
		/// Serialises the results of the evaluation to its own file.
		/// </summary>
		/// <param name="records">The holder of the evaluation information.</param>
		private static void SaveEvaluation(IRecordHolder records) {
			string directory = Settings.QueryDir;
			string filename = Settings.EvaluationFile;

			if (!Directory.Exists(directory))
				Directory.CreateDirectory((directory));

			string path = Path.Combine(directory, filename);
			Settings.FileManager.Write(path, records);
			Logger.Log(I_Evaluation_Exp, path);
		}


		#region --- Evaluation Methods ---

		private static string NameProvider(QueryResult result) => result.QueryName;
		private static string ClassProvider(QueryResult result) =>
			Settings.FileManager.ClassByShapeName(result.QueryName);
		private static string ClassRecord(Record record) =>
			Settings.FileManager.ClassByShapeName(record.Name, false);
		private static int Average(ICollection<object> collection) {
			if (collection.Count == 0) return 0;
			int sum = 0;
			foreach(object value in collection)
				if (value is int number)
					sum = checked(sum + number);
			return sum / collection.Count;
		}
		private static double AverageDouble(ICollection<object> collection) {
			if (collection.Count == 0)
				return 0;
			double sum = 0;
			foreach (object value in collection)
				if (value is double number)
					sum = checked(sum + number);
			return sum / collection.Count;
		}
		private static string ClassChoice(ICollection<object> collection) {
			foreach(object obj in collection)
				if (obj is string name)
					return name;
			return null;
		}


		private static int CacheTotal() => Settings.MeshLibrary.Count;
		private static int CacheRelevant(QueryResult result) {
			FileManager manager = Settings.FileManager;
			string queryName = result.QueryName;
			string clazz = manager.ClassByShapeName(queryName, false);
			return manager.ShapesInClass(clazz);
		}
		private static int CacheIrrelevant<T>(T result, ICache<T> cache) {
			int TT = cache.GetValue<int, T>("Total", result);
			int RV = cache.GetValue<int, T>("Relevant", result);

			return TT - RV;
		}

		private static int CacheIncorrect<T>(T result, ICache<T> cache) =>
			cache.GetValue<int, T>("Total", result)
			- cache.GetValue<int, T>("Correct", result);


		private static int CacheTP(QueryResult result) {
			string clazz = ClassProvider(result);
			int TPCount = 0;

			foreach (QueryItem item in result.Results) {
				string itemClazz = Settings.FileManager
										   .ClassByShapeName(item.MeshName, false);
				if (string.Equals(itemClazz, clazz,
					StringComparison.OrdinalIgnoreCase))
					TPCount++;
			}

			return TPCount;
		}
		private static int CacheFP<T>(T result, ICache<T> cache) =>
			cache.GetValue<int, T>("Correct", result)
			- cache.GetValue<int, T>("TP", result);
		private static int CacheFN<T>(T result, ICache<T> cache) =>
			cache.GetValue<int, T>("Relevant", result)
			- cache.GetValue<int, T>("TP", result);
		private static int CacheTN<T>(T result, ICache<T> cache) {
			int IR = cache.GetValue<int, T>("Irrelevant", result);
			int FP = cache.GetValue<int, T>("FP", result);

			return IR - FP;
		}

		private static double Accuracy<T>(T result, ICache<T> cache) {
			double TP = cache.GetValue<int, T>("TP", result);
			double TN = cache.GetValue<int, T>("TN", result);
			double TT = cache.GetValue<int, T>("Total", result);

			return (TP + TN) / TT;
		}

		private static double Precision<T>(T result, ICache<T> cache) =>
			((double) cache.GetValue<int, T>("TP", result))
			/ cache.GetValue<int, T>("Correct", result);
		private static double Recall<T>(T result, ICache<T> cache) =>
			((double) cache.GetValue<int, T>("TP", result))
			/ cache.GetValue<int, T>("Relevant", result);
		private static double Specificity<T>(T result, ICache<T> cache) =>
			((double) cache.GetValue<int, T>("TN", result))
			/ cache.GetValue<int, T>("Irrelevant", result);
		private static double Sensitivity<T>(T result, ICache<T> cache) =>
			((double) cache.GetValue<int, T>("TP", result))
			/ cache.GetValue<int, T>("Irrelevant", result);
		private static double F1<T>(T result, ICache<T> cache) {
			double precision = Precision(result, cache);
			double recall = Recall(result, cache);

			if (IsZero(precision) || IsZero(recall)) return 0;
			return (2 * precision * recall) / (precision + recall);
		}

		private static bool IsZero(double value) {
			return value > -double.Epsilon && value < double.Epsilon;
		}

		#endregion

	}
}
