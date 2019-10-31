using System;
using System.Globalization;
using System.IO;
using ShapeDatabase.Features.Statistics;
using ShapeDatabase.Shapes;
using ShapeDatabase.UI.Console.Verbs;

using static System.Console;
using static ShapeDatabase.UI.Properties.Resources;


namespace ShapeDatabase.UI.Console.Handlers {

	/// <summary>
	/// The class which should handle measuring statistics for all shapes.
	/// </summary>

	public static class MeasureHandler {

		/// <summary>
		/// The operation to calculate metrics for saved and presented shapes.
		/// </summary>
		/// <param name="options">The options object which contains extra information
		/// which helps during the exeuction of this modus.</param>
		public static void Start(MeasureOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			WriteLine(I_StartProc_Measure);
			// Load in new shapes to measure.
			if (options.HasDirectories)
				foreach (string dir in options.ShapeDirectories)
					Settings.FileManager.AddDirectoryDirect(dir);
			// Perform metric calculations.
			IRecordHolder<MeshEntry> recordHolder = MetricCalculator;
			recordHolder.TakeSnapShot(Settings.MeshLibrary);
			// Save results to external file.
			SaveMetrics(recordHolder);
			// Notify the user of the refined shapes.
			WriteLine(I_ShapeCount, Settings.MeshLibrary.Count);
			WriteLine(I_EndProc_Measure);
		}


		/// <summary>
		/// Serialise the results to a file so it can be viewed outside the application.
		/// </summary>
		/// <param name="recordHolder">The object containing the new metrics.</param>
		private static void SaveMetrics(IRecordHolder<MeshEntry> recordHolder) {
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
			Settings.FileManager.WriteObject(recordHolder, location);
			WriteLine(I_Measure_Exp, filename);
		}

		/// <summary>
		/// The object to calculate different measures from the database.
		/// </summary>
		private static IRecordHolder<MeshEntry> MetricCalculator {
			get {
				return new RecordHolder<MeshEntry>(
					(MeshEntry entry) => entry.Name,
					("Name",     (MeshEntry entry) => entry.Name),
					("Class",    (MeshEntry entry) => entry.Class),
					("Vertices", (MeshEntry entry) => entry.Mesh.VertexCount),
					("Faces",    (MeshEntry entry) => entry.Mesh.FaceCount),
					("",                         _ => ""), // Empty line to seperate values.
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
					("Volume",   (MeshEntry entry) => entry.Mesh.GetBoundingBox().Volume)
				);
			}
		}

	}

}
