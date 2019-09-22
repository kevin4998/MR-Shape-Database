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
		public static MeshLibrary MeshLibrary => Settings.FileManager.ProcessedMeshes;

	}
}
