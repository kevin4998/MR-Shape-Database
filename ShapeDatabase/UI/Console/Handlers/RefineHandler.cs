using System;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle refining new shapes and
	/// adding them into the repository.
	/// </summary>
	public static class RefineHandler {

		/// <summary>
		/// The operation to refine new shapes and add them to the repository.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static int Start(RefineOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			WriteLine(I_StartProc_Mesh);
			// Refine newly provided shapes.
			if (options.HasDirectories)
				foreach (string dir in options.ShapeDirectories)
					Settings.FileManager.AddDirectory(dir);
			// Notify the user of the refined shapes.
			WriteLine(I_ShapeCount, Settings.MeshLibrary.Count);
			WriteLine(I_EndProc_Mesh);
			return 0;
		}

	}

}
