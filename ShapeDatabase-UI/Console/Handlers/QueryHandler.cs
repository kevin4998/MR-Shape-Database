﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ShapeDatabase.Features;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.IO;
using ShapeDatabase.Query;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.UI.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle quering and comparing shapes with
	/// the database shapes.
	/// </summary>
	public static class QueryHandler {

		/// <summary>
		/// The operation to query for similar shapes in the database.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(QueryOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			WriteLine(I_StartProc_Query);

			LoadQueryFiles(options);
			FeatureManager manager = FeatureHandler.LoadFeatures(options.ShouldImport);
			QueryResult[]  results = LoadQueryResults(false, manager, options);
			if (options.ShouldExport)
				SaveQueries(results);
			ShowQueryResults(results);

			WriteLine(I_EndProc_Query);
		}


		/// <summary>
		/// Processes the query shapes (and loads them in memory).
		/// </summary>
		private static void LoadQueryFiles(QueryOptions options) {
			QueryInputMode input = options.QueryInputMode;
			IEnumerable<string> dirs = new string[] {Settings.QueryDir};
			if (options.HasDirectories)
				dirs = options.QueryDirectories;
			FileManager fileManager = Settings.FileManager;

			if (input == QueryInputMode.Internal) {

				fileManager.QueryMeshes = fileManager.ProcessedMeshes;

			} else {
				foreach (string dir in dirs) {
					// Create the directory if it does not exist.
					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);
					// Add the new shapes to the query directory.
					switch (input) {
					case QueryInputMode.Refine:
						fileManager.AddQueryDirectory(dir);
						break;
					case QueryInputMode.Direct:
						fileManager.AddQueryDirectoryDirect(dir);
						break;
					}
				}
			}

			
		}

		/// <summary>
		/// Either read the cached results or recompute them.
		/// </summary>
		/// <param name="import">If previous results should be reloaded.</param>
		/// <param name="manager">The features which can be used for recomputing.</param>
		/// <returns>An array containing the results of different querry operations.
		/// </returns>
		public static QueryResult[] LoadQueryResults(bool import,
													 FeatureManager manager = null,
													 QueryOptions options = null) {
			string filename = Settings.QueryResultsFile;
			string directory = Settings.QueryDir;

			Directory.CreateDirectory(directory);
			string location = Path.Combine(directory, filename);
			// Load from cached data if possible.
			if (import
				&& Settings.FileManager.TryRead(location, out QueryResult[] results)
				&& results.Length != 0) {
				WriteLine(I_Query_Imp, location);
				return results;
			}
			// Load new data.
			return ProcessQuery(manager ?? FeatureHandler.LoadFeatures(import), options);
		}

		/// <summary>
		/// Compare all the querries with the database items.
		/// </summary>
		/// <param name="manager">The holder of all the feature vectors.</param>
		/// <returns>A collection of results of the comparison.</returns>
		private static QueryResult[] ProcessQuery(FeatureManager manager,
												  QueryOptions options) {
			int queryItems = Settings.QueryLibrary.Meshes.Count;
			QueryResult[] queryResults = new QueryResult[queryItems];
			int nextElement = 0;
			QuerySizeMode mode = options.QuerySizeMode;
			Enum.TryParse(mode.ToString(), out QuerySize size);

			if (Settings.MeshLibrary.Count == 0)
				foreach (MeshEntry entry in Settings.QueryLibrary.Meshes)
					queryResults[nextElement++] = new QueryResult(entry.Name);
			else
				queryResults = manager.CalculateResults(
					size,
					Settings.QueryLibrary.Meshes);

			return queryResults;
		}

		/// <summary>
		/// Serialises the results from the querries to their own file for offline
		/// viewing.
		/// </summary>
		/// <param name="queries">The differen querries which were performed.</param>
		private static void SaveQueries(params QueryResult[] queries) {
			WriteLine(I_Query_Exp);

			string directory = Settings.QueryDir;
			string filename = Settings.QueryResultsFile;
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			string location = Path.Combine(directory, filename);
			Settings.FileManager.Write(location, queries);
			WriteLine(I_Query_Exp, location);
		}

		/// <summary>
		/// Shows the user the results of the queries.
		/// </summary>
		static void ShowQueryResults(params QueryResult[] queries) {
			WriteLine(I_QueryCount, queries.Length);
			foreach (QueryResult queryResult in queries)
				WriteLine($"\t- {queryResult}");
		}

	}
}
