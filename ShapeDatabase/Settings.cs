using System.Globalization;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;

namespace ShapeDatabase {
	/// <summary>
	/// A class with the purpose of defining application global variables.
	/// </summary>
	public static class Settings {

		/// <summary>
		/// The location where all the shapes are definined.
		/// These are the unmodified files as originally retrieved online.
		/// </summary>
		public static string ShapeLibraryDir { get; set; } = "Content/Shapes/Initial";

		/// <summary>
		/// The location where all the processed and normalised files are stored.
		/// </summary>
		public static string ShapeFinalDir { get; set; } = "Content/Shapes/Final";

		/// <summary>
		/// The location where the shapes are stored which require refinement.
		/// </summary>
		public static string ShapeTempDir { get; set; } = "Content/Shapes/Temp";

		/// <summary>
		/// The location where the shapes are stored which could not be refined.
		/// </summary>
		public static string ShapeFailedDir { get; set; } = "Content/Shapes/Failed";

		/// <summary>
		/// The location where all the Java refinement scripts are stored.
		/// </summary>
		public static string JavaScriptsDir { get; set; } = "Content/Scripts";

		/// <summary>
		/// The location where Java is installed on the computer.
		/// </summary>
		public static string JavaDir { get; set; } = "C:/Program Files (x86)/Common Files/Oracle/Java/javapath/java.exe";
		

		/// <summary>
		/// The culture which is required for converting the shape files.
		/// </summary>
		public static CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

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

		/// <summary>
		/// A global manager for all the files in this application.
		/// </summary>
		public static FileManager FileManager { get; set; } = new FileManager();

		/// <summary>
		/// A collection of all the loaded shapes by this application.
		/// </summary>
		public static MeshLibrary MeshLibrary => Settings.FileManager.ProcessedMeshes;

		/// <summary>
		/// A collection of strings which will force the program to stop.
		/// </summary>
		public static string[] ExitArguments => new string[] { "exit", "quit", "stop" };

	}
}
