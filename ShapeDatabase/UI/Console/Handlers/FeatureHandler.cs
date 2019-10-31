using System;
using System.IO;
using ShapeDatabase.Features;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which will calculate features for all the existing entries.
	/// </summary>
	public static class FeatureHandler {

		/// <summary>
		/// The operation to refine new shapes and add them to the repository.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static int Start(FeatureOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			WriteLine(I_StartProc_Feature);
			ExtractFeatures();
			WriteLine(I_EndProc_Feature);
			return 0;
		}

		/// <summary>
		/// Performs the feature extraction.
		/// </summary>
		private static void ExtractFeatures() {
			// Calculate the different vectors.
			FeatureManager manager = new FMBuilder().Build();
			manager.CalculateVectors(Settings.MeshLibrary);
			// Serialise it to a new file.
			string filename = Settings.FeatureVectorFile;
			string directory = Settings.FeatureVectorDir;

			Directory.CreateDirectory(directory);
			string location = Path.Combine(directory, filename);
			Settings.FileManager.WriteObject(manager, location);
			// Notify that it got exported.
			WriteLine(I_Feature_Exp, location);
		}

	}

}
