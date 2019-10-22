using CommandLine;
using ShapeDatabase.Features.Statistics;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI;
using ShapeDatabase.Features;
using System;
using System.Collections.Generic;
using System.Globalization;
using ShapeDatabase.Features.Descriptors;
using System.Linq;
using System.Reflection;
using System.IO;

using static ShapeDatabase.Properties.Resources;
using System.Threading.Tasks;

namespace ShapeDatabase {

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
			Console.WriteLine(I_StartProc_Input);

			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(OnErrors)
				.WithParsed(OnParsedValues);
		}


		/// <summary>
		/// Actions which will be performe don the converted Options
		/// object.
		/// </summary>
		/// <param name="options">Parsed Command Line Argument Collection.</param>
		static void OnParsedValues(Options options) {
			// Convert Options value to Settings.
			if (Enum.TryParse(options.Mode, true, out OperationModes mode))
				Settings.Mode = mode | OperationModes.VIEW;
			Settings.Culture = CultureInfo.GetCultureInfo(options.Culture);
			Settings.ShowDebug = options.DebugMessages;

			if (options.CleanStart) {
				Console.WriteLine(I_StartClean);
				string[] cachedDirs = new string[] {
					Settings.ShapeFailedDir,
					Settings.ShapeFinalDir,
					Settings.ShapeTempDir
					//Settings.MeasurementsFile
				};
				foreach (string dir in cachedDirs) {
					DirectoryInfo info = new DirectoryInfo(dir);
					info.Delete(true);
				}
				Console.WriteLine(I_EndClean);
			}

			Console.WriteLine(I_EndProc_Input);
			// Start program activity.
			LoadFinalFiles();
			if (Settings.Mode.HasFlag(OperationModes.REFINE))
				RefineShapes(options.ShapeDirectories);
			if (Settings.Mode.HasFlag(OperationModes.MEASURE))
				MeasureShapes(options.ShapeDirectories);
			if (Settings.Mode.HasFlag(OperationModes.FEATURES))
				ExtractFeatures(options.ShapeDirectories);
			if (Settings.Mode.HasFlag(OperationModes.QUERY))
				QueryShapes(ExtractFeatures(options.ShapeDirectories));
			if (Settings.Mode.HasFlag(OperationModes.VIEW))
				ViewShapes(options.ShapeDirectories);
		}

		/// <summary>
		/// Actions which will be performed on all the exceptions
		/// generated when parsing text values.
		/// </summary>
		/// <param name="errors">A collection of generated errors
		/// by the Parser package.</param>
		static void OnErrors(IEnumerable<Error> errors) {
			foreach (Error error in errors)
				Console.WriteLine(error.ToString());
		}



		/// <summary>
		/// Refinement mode for the application.
		/// This requests the application to select the given shapes
		/// and refine them to our specified standard.
		/// </summary>
		/// <param name="dirs">The directories containing shapes.</param>
		static void RefineShapes(IEnumerable<string> dirs) {
			Console.WriteLine(I_StartProc_Mesh);
			LoadNewFiles(dirs, true);
			Console.WriteLine(I_EndProc_Mesh);
			ShowShapeCount();
		}

		/// <summary>
		/// Measurement mode for the application.
		/// This requests the application to analyse the given shapes
		/// and tell us their statistics.
		/// </summary>
		/// <param name="dirs">The directories containing shapes.</param>
		static void MeasureShapes(IEnumerable<string> dirs) {
			Console.WriteLine(I_StartMeasure);
			LoadNewFiles(dirs, false);

			RecordHolder recordHolder = new RecordHolder(
				("Name",	 (MeshEntry entry) => entry.Name),
				("Class",	 (MeshEntry entry) => entry.Class),
				("Vertices", (MeshEntry entry) => entry.Mesh.VertexCount),
				("Faces",	 (MeshEntry entry) => entry.Mesh.FaceCount),
				("",						 _ => ""), // Empty line to seperate values.
				("Min X",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MinX),
				("Min Y",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MinY),
				("Min Z",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MinZ),
				("Max X",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MaxX),
				("Max Y",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MaxY),
				("Max Z",    (MeshEntry entry) => entry.Mesh.GetBoundingBox().MaxZ),
				(" ",                        _ => ""), // Empty line to seperate values.
				("Range X",  (MeshEntry entry) => entry.Mesh.GetBoundingBox().Width),
				("Range Y",  (MeshEntry entry) => entry.Mesh.GetBoundingBox().Height),
				("Range Z",  (MeshEntry entry) => entry.Mesh.GetBoundingBox().Depth),
				("Volume",	 (MeshEntry entry) => entry.Mesh.GetBoundingBox().Volume)
			);
			recordHolder.TakeSnapShot(Settings.MeshLibrary);

			Console.WriteLine(I_EndMeasure);

			// Preparation data to construct path and filename.
			CultureInfo culture = Settings.Culture;
			DateTime dateTime = recordHolder.SnapshotTime;
			string timeformat = F_DateFormat;
			string fileformat = F_File_Measure;
			string fileApendix = Settings.MeasurementsFile;
			// Example dir:
			// Content/Analysis
			string directory = Settings.MeasurementsDir;
			// Example time:
			// 2019-10-04-13-22-52
			string time = dateTime.ToString(timeformat, culture);
			// Example file:
			// 2019-10-04-13-22-52_measures.csv
			string filename = string.Format(culture, fileformat, time, fileApendix);
			// Example loc:
			// Content/Analysis/2019-10-04-13-22-52_measures.csv
			string location = Path.Combine(directory, filename);
			// Export the generated file
			Directory.CreateDirectory(directory);
			RecordsWriter.Instance.WriteFile(recordHolder, location);
			Console.WriteLine(I_Measure_Exp, filename);

			ShowShapeCount();
		}

		/// <summary>
		/// Viewing mode for the application.
		/// This requests the application to read the shapes
		/// and let the user view and analyse them themselves.
		/// </summary>
		/// <param name="dirs">The directories containing shapes.</param>
		static void ViewShapes(IEnumerable<string> dirs) {
			LoadNewFiles(dirs, false);

			MeshLibrary meshes = Settings.FileManager.ProcessedMeshes;
			// Notify the user of their options.
			ShowShapeCount();
			// Ask the user for a specific value.
			while (!Settings.DirectShutDown) {
				Console.WriteLine();	// Empty line for clearance.
				Console.WriteLine(I_ShapeSelect_Prompt,
								  Settings.ExitArguments.FirstOrDefault());
				string input = Console.ReadLine();

				if (Settings.ExitArguments.Contains(input.ToLower(Settings.Culture))) {
					Settings.DirectShutDown = true;
					break;
				} else if (meshes.Names.Contains(input)) {
					RunWindow(meshes[input].Mesh);
				} else {
					Console.WriteLine(I_UnknownCommand, input);
				}
			}
		}

		/// <summary>
		/// Mode for extracting featurevectors of the shapes, or reading them from a csv file.
		/// </summary>
		/// <param name="dirs">The directories containing shapes.</param>
		static FeatureManager ExtractFeatures(IEnumerable<string> _)
		{
			//LoadNewFiles(dirs, false);

			string filename = Settings.FeatureVectorFile;
			string directory = Settings.FeatureVectorDir;

			Directory.CreateDirectory(directory);
			string location = Path.Combine(directory, filename);

			FeatureManager manager;

			if (!Settings.ReadVectorFile)
			{
				Console.WriteLine(I_StartProc_Feature);

				manager = new FMBuilder().Build();
				manager.CalculateVectors(Settings.MeshLibrary.ToArray());

				Console.WriteLine(I_EndProc_Feature);

				ShowShapeCount();
				FMWriter.Instance.WriteFile(manager, location);

				Console.WriteLine(I_Feature_Exp, location);
			}
			else
			{
				using (StreamReader reader = new StreamReader(location))
				{
					manager = FMReader.Instance.ConvertFile(reader);
				}

				Console.WriteLine(I_Feature_Imp, location);
			}

			return manager;
		}

		/// <summary>
		/// Mode for comparing query shapes to the database shapes.
		/// </summary>
		/// <param name="featuremanager">The featuremanager of the complete database</param>
		static void QueryShapes(FeatureManager DatabaseFM)
		{
			Console.WriteLine(I_StartProc_Query);

			LoadQueryFiles();

			Tuple<string, IList<(string, double)>>[] QueryResults = new Tuple<string, IList<(string, double)>>[Settings.QueryLibrary.Meshes.Count];
			Parallel.For(0, Settings.QueryLibrary.Meshes.Count, i =>
			{
				string name = Settings.QueryLibrary.Names.ElementAt(i);
				IList<(string, double)> results = DatabaseFM.CalculateResults(Settings.QueryLibrary.Meshes.ElementAt(i));
				results = results.Take(Settings.KBestResults).ToArray();
				QueryResults[i] = new Tuple<string, IList<(string, double)>>(name, results);
			});

			Console.WriteLine(I_EndProc_Query);

			if (Settings.SaveQueryResults)
			{
				Console.WriteLine(I_Query_Exp);

				string location = Settings.QueryDir + "/" + Settings.QueryResultsFile;
				QueryWriter.Instance.WriteFile(QueryResults, location);
			}

			ShowQueryResults(QueryResults);
		}

		/// <summary>
		/// Shows the user the results of the queries.
		/// </summary>
		static void ShowQueryResults(Tuple<string, IList<(string, double)>>[] results)
		{
			Console.WriteLine($"{results.Length} Query Results:");
			foreach (Tuple<string, IList<(string, double)>> result in results)
			{
				Console.WriteLine($"\t- {result.Item1} : {string.Join(", ", result.Item2.Select(x => x.Item1 + "(" + x.Item2 + ")"))}");
			}
		}

		/// <summary>
		/// Tell the user how many shapes are currently loaded.
		/// </summary>
		static void ShowShapeCount() {
			MeshLibrary meshes = Settings.FileManager.ProcessedMeshes;
			Console.WriteLine(I_ShapeCount, meshes.Count);
			foreach (string name in meshes.Names)
				Console.WriteLine($"\t- {name}");
		}

		/// <summary>
		/// Visualises the specified mesh on the window.
		/// </summary>
		/// <param name="mesh"></param>
		static void RunWindow(IMesh mesh) {
			using (Window window = new Window(800, 600, A_WindowName, mesh)) {
				window.Run(60.0);
			}
		}

		/// <summary>
		/// Let the application process the given directories for shapes.
		/// </summary>
		/// <param name="dirs">The directories containing shapes.</param>
		/// <param name="refine">If the shapes should be automatically refined.</param>
		static void LoadNewFiles(IEnumerable<string> dirs, bool refine = false) {
			Console.WriteLine(I_StartLoad_Mesh);
			if (refine)
				foreach (string dir in dirs)
					Settings.FileManager.AddDirectory(dir);
			else
				foreach (string dir in dirs)
					Settings.FileManager.AddDirectoryDirect(dir);
			Console.WriteLine(I_EndLoad_Mesh);

		}

		/// <summary>
		/// Restores previously refined and processed files for
		/// visualisation or measurements.
		/// </summary>
		static void LoadFinalFiles() {
			Directory.CreateDirectory(Settings.ShapeFinalDir);
			Settings.FileManager.AddDirectoryDirect(Settings.ShapeFinalDir);
		}

		/// <summary>
		/// Processes the query shapes (and loads them in memory).
		/// </summary>
		static void LoadQueryFiles()
		{
			Settings.FileManager.AddQueryDirectory(Settings.QueryDir);
		}
	}
}
