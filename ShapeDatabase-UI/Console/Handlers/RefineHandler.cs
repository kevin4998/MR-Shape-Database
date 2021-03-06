﻿using System;
using ShapeDatabase.UI.Console.Verbs;
using ShapeDatabase.Util;
using static ShapeDatabase.UI.Properties.Resources;

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
		public static void Start(RefineOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Logger.Log(I_StartProc_Refine);
			// Refine newly provided shapes.
			if (options.HasDirectories)
				foreach (string dir in options.ShapeDirectories)
					Settings.FileManager.AddDirectory(dir);
			// Notify the user of the refined shapes.
			Logger.Log(I_ShapeCount, Settings.MeshLibrary.Count);
			Logger.Log(I_EndProc_Refine);
		}

	}

}
