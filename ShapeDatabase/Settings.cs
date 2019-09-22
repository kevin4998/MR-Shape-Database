using System.Globalization;

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

	}
}
