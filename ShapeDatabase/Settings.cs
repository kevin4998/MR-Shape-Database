using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;

using static ShapeDatabase.Properties.Resources;

namespace ShapeDatabase {

	/// <summary>
	/// A class with the purpose of defining application global variables.
	/// </summary>
	public static class Settings {

		// Responsible for IO operations.
		// Knowing where to save files and retrive them.
		#region --- File Structure ---

		/// <summary>
		/// The location where all the shapes are definined.
		/// These are the unmodified files as originally retrieved online.
		/// </summary>
		public static string ShapeLibraryDir { get; set; } =
			Path.Combine(D_ContentDir, D_ShapesDir, D_InitialDir);

		/// <summary>
		/// The location where the shapes are stored which require refinement.
		/// </summary>
		public static string ShapeTempDir { get; set; } =
			Path.Combine(D_ContentDir, D_ShapesDir, D_TempDir);

		/// <summary>
		/// The location where the shapes are stored which could not be refined.
		/// </summary>
		public static string ShapeFailedDir { get; set; } =
			Path.Combine(D_ContentDir, D_ShapesDir, D_FailedDir);

		/// <summary>
		/// The location where all the processed and normalised files are stored.
		/// </summary>
		public static string ShapeFinalDir { get; set; } =
			Path.Combine(D_ContentDir, D_ShapesDir, D_FinalDir);

		/// <summary>
		/// The location where the feature vectors will be stored in.
		/// </summary>
		public static string FeatureVectorDir { get; set; } =
			Path.Combine(D_ContentDir, D_FeatureDir);

		/// <summary>
		/// The location where the measurements/statistics will be stored in.
		/// </summary>
		public static string MeasurementsDir { get; set; } =
			Path.Combine(D_ContentDir, D_MeasureDir);


		/// <summary>
		/// States whether the featuremanager should be created by reading a vectorfile.
		/// </summary>
		public static bool ReadVectorFile { get; set; } = false;

		/// <summary>
		/// The file name of the feature vectors/descriptors.
		/// </summary>
		public static string FeatureVectorFile { get; set; } = D_FeatureFile;

		/// <summary>
		/// The file name of the measurement/statistics file.
		/// </summary>
		public static string MeasurementsFile { get; set; } = D_MeasureFile;

		#endregion

		// Responsible for determining application executions.
		// Which action to take and if data should be showno rn ot.
		#region --- Flow Properties ---

		/// <summary>
		/// The current processes which needs to be executed.
		/// </summary>
		public static OperationModes Mode { get; set; } = OperationModes.VIEW;


		/// <summary>
		/// If Debug messages should be visible in the console.
		/// </summary>
		public static bool ShowDebug { get; set; } = false;

		/// <summary>
		/// Describes if the application should be active.
		/// </summary>
		public static bool Active { get; set; } = true;

		/// <summary>
		/// Describes if all the functionality of the application should cease at once.
		/// </summary>
		public static bool DirectShutDown { get; set; } = false;

		#endregion

		// Responsible for allow access throughout the whole application.
		// Variabls which needs to be used everywhere, or important static methods.
		#region --- Global Properties ---

		/// <summary>
		/// The culture which is required for converting the shape files.
		/// </summary>
		public static CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

		/// <summary>
		/// A global manager for all the files in this application.
		/// </summary>
		public static FileManager FileManager { get; set; } = new FileManager();

		/// <summary>
		/// A collection of all the loaded shapes by this application.
		/// </summary>
		public static MeshLibrary MeshLibrary => FileManager.ProcessedMeshes;

		/// <summary>
		/// A collection of all the query meshes sturctured inside a library.
		/// </summary>
		public static MeshLibrary QueryMeshes { get; } = new MeshLibrary();

		/// <summary>
		/// A collection of all the loaded shapes by this application.
		/// </summary>
		public static MeshLibrary QueryLibrary => Settings.FileManager.QueryMeshes;

		// <summary>
		/// The location where the query shapes are located
		/// </summary>
		public static string QueryDir { get; set; } = "Content/Query";

		/// <summary>
		/// The file name of the query results file (also stored in QueryDir)
		/// </summary>
		public static string QueryResultsFile { get; set; } = "queryresults.csv";

		/// <summary>
		/// Stating the number the number of best matching results that should be shown/saved.
		/// </summary>
		public static int KBestResults = 5;

		/// <summary>
		/// States whether the query results should be saved (in QueryDir)
		/// </summary>
		public static bool SaveQueryResults = true;

		/// <summary>
		/// A collection of strings which will force the program to stop.
		/// </summary>
		public static ICollection<string> ExitArguments => A_ExitArgs.Split(A_ExitSep.ToCharArray());

		#endregion

	}
}
