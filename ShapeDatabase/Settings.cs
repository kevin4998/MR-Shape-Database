using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ShapeDatabase.Features;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util.Attributes;
using static ShapeDatabase.Properties.Resources;

namespace ShapeDatabase {

	/// <summary>
	/// A class with the purpose of defining application global variables.
	/// </summary>
	public static class Settings {

		// Responsible for IO operations.
		// Knowing where to save files and retrieve them.
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
		/// The location where the query shapes are located
		/// </summary>
		public static string QueryDir { get; set; } =
			Path.Combine(D_ContentDir, D_QueryDir);


		/// <summary>
		/// The file name of the feature vectors/descriptors.
		/// </summary>
		public static string FeatureVectorFile { get; set; } = D_FeatureFile;

		/// <summary>
		/// The file name of the measurement/statistics file.
		/// </summary>
		public static string MeasurementsFile { get; set; } = D_MeasureFile;

		/// <summary>
		/// The file name of the query results file.
		/// </summary>
		public static string QueryResultsFile { get; set; } = D_QueryFile;

		/// <summary>
		/// The file name of the application settings file.
		/// </summary>
		public static string SettingsFile { get; set; } = D_SettingsFile;

		/// <summary>
		/// The file name of the query evaluation results.
		/// </summary>
		public static string EvaluationFile { get; set; } = D_EvaluationFile;

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
		[Ignore]
		public static bool Active { get; set; } = true;

		/// <summary>
		/// Describes if all the functionality of the application should cease at once.
		/// </summary>
		[Ignore]
		public static bool DirectShutDown { get; set; } = false;

		/// <summary>
		/// States whether the featuremanager should be created by reading a vectorfile.
		/// </summary>
		public static bool ReadVectorFile { get; set; } = false;

		/// <summary>
		/// States whether the query results should be saved (in QueryDir)
		/// </summary>
		public static bool SaveQueryResults { get; set; } = true;


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
		/// A collection of all the loaded shapes by this application.
		/// </summary>
		public static MeshLibrary QueryLibrary => FileManager.QueryMeshes;

		/// <summary>
		/// A collection of all the weights for the different Descriptors.
		/// </summary>
		public static WeightManager Weights => WeightManager.Instance;

		/// <summary>
		/// Size of the weighted vertex array.
		/// </summary>
		public static int WeightedVertexArraySize { get; set; } = 100000;

		/// <summary>
		/// Refine shapes to this number of vertices.
		/// </summary>
		public static int RefineVertexNumber { get; set; } = 5250;

		/// <summary>
		/// Maximum number of refinement steps per shape.
		/// </summary>
		public static int MaxRefineIterations { get; set; } = 20;

		/// <summary>
		/// The maximum number of times that any form of refinement will
		/// be performed on a mesh before concluding that it can't be fixed.
		/// </summary>
		public static int RefinementThreshold { get; set; } = 16;

		/// <summary>
		/// Number of values per histogram descriptor.
		/// </summary>
		public static int ValuesPerHistogram { get; set; } = 5000;

		/// <summary>
		/// Number of bins per histogram descriptor.
		/// </summary>
		public static int BinsPerHistogram { get; set; } = 10;

		/// <summary>
		/// Stating the number the number of best matching results that
		/// should be shown/saved.
		/// </summary>
		public static int KBestResults { get; set; } = 5;


		/// <summary>
		/// A collection of strings which will force the program to stop.
		/// </summary>
		public static ICollection<string> ExitArguments => A_ExitArgs.Split(A_ExitSep.ToCharArray());

		#endregion

	}
}
