using System;
using System.IO;
using ShapeDatabase.Features.Statistics;
using ShapeDatabase.Query;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.UI.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle evaluating the results of different querries.
	/// </summary>
	public static class EvaluateHandler {

		private static MeshLibrary StoredMeshes => Settings.MeshLibrary;
		private static MeshLibrary QueryMeshes => Settings.QueryLibrary;
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
					("Accuracy", (result, cache) => Accuracy(result, cache)),
					("Precision", (result, cache) => Precision(result, cache)),
					("Recall", (result, cache) => Recall(result, cache)),
					("Specificity", (result, cache) => Specificity(result, cache)),
					("Sensitivity", (result, cache) => Sensitivity(result, cache))
				);
				// Return the Record Holder.
				return recordHolder;
			}
		}


		/// <summary>
		/// The operation to evaluate the result of a query.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(EvaluateOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			WriteLine(I_StartProc_Evaluate);

			IRecordHolder<QueryResult> records = EvaluationCalculator;
			QueryResult[] results = QueryHandler.LoadQueryResults(options.ShouldImport);
			records.TakeSnapShot(results);
			if (options.ShouldExport)
				SaveEvaluation(records);

			WriteLine(I_EndProc_Evaluate);
		}


		/// <summary>
		/// Serialises the results of the evaluation to its own file.
		/// </summary>
		/// <param name="records">The holder of the evaluation information.</param>
		private static void SaveEvaluation(IRecordHolder<QueryResult> records) {
			string directory = Settings.QueryDir;
			string filename = Settings.EvaluationFile;

			if (!Directory.Exists(directory))
				Directory.CreateDirectory((directory));

			string path = Path.Combine(directory, filename);
			Settings.FileManager.WriteObject(records, path);
			WriteLine(I_Evaluation_Exp, path);
		}


		#region --- Evaluation Methods ---

		private static MeshEntry GetMesh(string name) {
			if (StoredMeshes.TryGetValue(name, out MeshEntry entry))
				return entry;
			if (QueryMeshes.TryGetValue(name, out entry))
				return entry;
			throw new ArgumentException(EX_NoMesh);
		}
		private static int ClassCount(string className) {
			int count = 0;
			foreach (MeshEntry entry in StoredMeshes)
				if (string.Equals(entry.Class, className,
					StringComparison.InvariantCultureIgnoreCase))
					count++;
			return count;
		}


		private static string NameProvider(QueryResult result) => result.QueryName;

		private static int CacheTotal() => StoredMeshes.Count;
		private static int CacheRelevant(QueryResult result) {
			string queryName = result.QueryName;
			MeshEntry queryEntry = GetMesh(queryName);
			return ClassCount(queryEntry.Class);
		}
		private static int CacheIrrelevant<T>(T result, ICache<T> cache) =>
			cache.GetValue<int, T>("Total", result)
			- cache.GetValue<int, T>("Relevant", result);
		private static int CacheIncorrect<T>(T result, ICache<T> cache) =>
			cache.GetValue<int, T>("Total", result)
			- cache.GetValue<int, T>("Correct", result);


		private static int CacheTP(QueryResult result) {
			string queryName = result.QueryName;
			MeshEntry queryEntry = GetMesh(queryName);
			string clazz = queryEntry.Class;
			int TPCount = 0;

			foreach (QueryItem item in result.Results) {
				MeshEntry itemEntry = GetMesh(item.MeshName);
				if (string.Equals(itemEntry.Class, clazz,
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
		private static int CacheTN<T>(T result, ICache<T> cache) =>
			cache.GetValue<int, T>("Irrelevant", result)
			- cache.GetValue<int, T>("FP", result);


		private static double Accuracy<T>(T result, ICache<T> cache) =>
			((double) (cache.GetValue<int, T>("TP", result)
				+ cache.GetValue<int, T>("TN", result))
			) / cache.GetValue<int, T>("Total", result);
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

		#endregion

	}
}
