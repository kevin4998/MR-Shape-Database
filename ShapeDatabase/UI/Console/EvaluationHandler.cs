using System;
using System.IO;
using ShapeDatabase.Features.Statistics;
using ShapeDatabase.Properties;
using ShapeDatabase.Query;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.UI.Console {

	/// <summary>
	/// A class with the responsibilty of handling all the evaluation operations.
	/// </summary>
	[Obsolete("Use the EvaluationHandler in ShapeDatabase.UI.Console.Handlers")]
	public static class EvaluationHandler {

		private static MeshLibrary StoredMeshes => Settings.MeshLibrary;
		private static MeshLibrary QueryMeshes => Settings.QueryLibrary;

		private static MeshEntry GetMesh(string name) {
			if (StoredMeshes.TryGetValue(name, out MeshEntry entry))
				return entry;
			if (QueryMeshes.TryGetValue(name, out entry))
				return entry;
			throw new ArgumentException(Resources.EX_NoMesh);
		}

		private static int ClassCount(string className) {
			int count = 0;
			foreach (MeshEntry entry in StoredMeshes)
				if (string.Equals(entry.Class, className,
					StringComparison.InvariantCultureIgnoreCase))
					count++;
			return count;
		}


		/// <summary>
		/// The main entry point for handling all the operations concerning
		/// the evaluation process.
		/// </summary>
		public static void Evaluate(params QueryResult[] results) {
			IRecordHolder<QueryResult> records = CreateRecords();
			records.TakeSnapShot(results);

			FileInfo file = PrepareFile();
			Settings.FileManager.WriteObject(records, file.FullName);
		}

		private static FileInfo PrepareFile() {
			string directory = Settings.QueryDir;
			string filename = Settings.EvaluationFile;

			if (!Directory.Exists(directory))
				Directory.CreateDirectory((directory));

			string path = Path.Combine(directory, filename);
			return new FileInfo(path);
		}

		private static IRecordHolder<QueryResult> CreateRecords() {

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

	}

}
