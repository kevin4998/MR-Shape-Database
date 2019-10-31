using System;
using System.IO;
using ShapeDatabase.Features;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.UI.Properties.Resources;

namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which will calculate features for all the existing entries.
	/// </summary>
	public static class FeatureHandler {

		/// <summary>
		/// Safely provides a location where the file can be saved without errors.
		/// </summary>
		private static string FeatureFile {
			get {
				string filename = Settings.FeatureVectorFile;
				string directory = Settings.FeatureVectorDir;

				Directory.CreateDirectory(directory);
				return Path.Combine(directory, filename);
			}
		}


		/// <summary>
		/// The operation to refine new shapes and add them to the repository.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(FeatureOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			WriteLine(I_StartProc_Feature);
			FeatureManager manager = LoadFeatures(false);
			if (options.ShouldExport)
				SaveFeatures(manager);
			WriteLine(I_EndProc_Feature);
		}


		/// <summary>
		/// Either read the cached file or calculate new feature values.
		/// </summary>
		/// <returns>A <see cref="FeatureManager"/> containing all the features
		/// for the shapes in the database.</returns>
		public static FeatureManager LoadFeatures(bool import) {
			string filename = Settings.FeatureVectorFile;
			string directory = Settings.FeatureVectorDir;

			Directory.CreateDirectory(directory);
			string location = Path.Combine(directory, filename);
			// Load from cached data if possible.
			if (import
				&& Settings.FileManager.TryRead(location, out FeatureManager manager)
				&& manager.FeatureCount != 0) {
				WriteLine(I_Feature_Imp, location);
				return manager;
			}
			// Load new data.
			return ExtractFeatures();
		}

		/// <summary>
		/// Performs the feature extraction.
		/// </summary>
		private static FeatureManager ExtractFeatures() {
			// Calculate the different vectors.
			FeatureManager manager = new FMBuilder().Build();
			manager.CalculateVectors(Settings.MeshLibrary);
			return manager;
		}

		/// <summary>
		/// Serialises the features to their own file.
		/// </summary>
		/// <param name="manager">The object containing all the features.</param>
		private static void SaveFeatures(FeatureManager manager) {
			// Serialise it to a new file.
			string location = FeatureFile;
			Settings.FileManager.WriteObject(manager, location);
			// Notify that it got exported.
			WriteLine(I_Feature_Exp, location);
		}

	}

}
