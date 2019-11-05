using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.IO;
using ShapeDatabase.Refine;
using ShapeDatabase.Util;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A manager with the purpose of storing and sorting all different
	/// meshes which can be loaded by the application.
	/// </summary>
	public class LibraryManager {

		#region --- Properties ---

		/// <summary>
		/// The one responsible for all refinement operations.
		/// </summary>
		public RefineManager Refiner { get; } = new RefineManager();

		/// <summary>
		/// A collection of all the loaded meshes structured inside a library.
		/// </summary>
		public MeshLibrary ProcessedMeshes { get; set; } = new MeshLibrary();

		/// <summary>
		/// A collection of all the query meshes structured inside a library.
		/// </summary>
		public MeshLibrary QueryMeshes { get; set; } = new MeshLibrary();

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initiates a new manager responsible for storing meshes in the right place
		/// and making them fit the programs norm.
		/// </summary>
		public LibraryManager() { }

		#endregion

		#region --- Instance Methods --- 

		#region -- Normal Meshes --

		public void AddAndRefine(string file) => AddAndRefine(new FileInfo(file));
		public void AddAndRefine(FileInfo file) {
			if (ProcessedMeshes.Contains(file.Name)) return;
			AddDirect(Refine(file, Settings.ShapeFinalDir, Settings.ShapeFailedDir));
		}

		public void AddDirect(string file) => AddDirect(new FileInfo(file));
		public void AddDirect(FileInfo file) {
			if (ProcessedMeshes.Contains(file.Name)) return;
			Add(file, ProcessedMeshes);
		}

		#endregion

		#region -- Query Meshes --

		public void AddQueryAndRefine(string file) => AddQueryAndRefine(new FileInfo(file));
		public void AddQueryAndRefine(FileInfo file) {
			if (QueryMeshes.Contains(file.Name)) return;
			AddQueryDirect(Refine(file, null, Settings.ShapeFailedDir));
		}

		public void AddQueryDirect(string file) => AddQueryDirect(new FileInfo(file));
		public void AddQueryDirect(FileInfo file) {
			if (QueryMeshes.Contains(file.Name)) return;
			Add(file, QueryMeshes);
		}

		#endregion

		#region -- Retrieval Operations --

		public int ShapesInClass(string className) {
			if (string.IsNullOrEmpty(className))
				throw new ArgumentNullException(nameof(className));

			return ProcessedMeshes.Count(
				mesh => className.Equals(
					mesh.Class,
					StringComparison.InvariantCultureIgnoreCase
				)
			);
		}

		public string ClassByShapeName(string shapeName, bool queryShape = true) {
			if (string.IsNullOrEmpty(shapeName))
				throw new ArgumentNullException(nameof(shapeName));

			MeshLibrary library = queryShape ? QueryMeshes : ProcessedMeshes;
			foreach (MeshEntry mesh in library)
				if (string.Equals(mesh.Name, shapeName,
									StringComparison.InvariantCultureIgnoreCase))
					return mesh.Class;
			MeshLibrary other = queryShape ? ProcessedMeshes : QueryMeshes;
			foreach (MeshEntry mesh in other)
				if (string.Equals(mesh.Name, shapeName,
									StringComparison.InvariantCultureIgnoreCase))
					return mesh.Class;
			return null;
		}

		#endregion

		#region -- Mesh Operations --

		private void Add(FileInfo file, MeshLibrary library) =>
			library.Add(new MeshEntry(file));


		private FileInfo Refine(FileInfo file,
								string successMap = null,
								string failureMap = null) {
			Refine(ref file, successMap, failureMap);
			return file;
		}

		private void Refine(ref FileInfo file,
							string successMap = null,
							string failureMap = null) {
			if (file == null || !file.Exists)
				throw new ArgumentNullException();

			// Remember the original map for later.
			string originalMap = file.Directory.Parent.FullName;
			// All modifications happen in the temp map.
			MoveFile(ref file, Settings.ShapeTempDir);

			// Refine it a maximum of X amounts of time.
			int attempts = Settings.MaxRefineIterations;
			while (!Refiner.RefineFile(file) && --attempts >= 0);

			// If it is refined then we add it to the final map.
			// If it is not refined then we add it to the failed map.
			if (attempts < 0 && failureMap != null)
				MoveFile(ref file, failureMap);
			else if (attempts >= 0 && successMap != null)
				MoveFile(ref file, successMap);
			else
				MoveFile(ref file, originalMap);
		}


		private void MoveFile(ref FileInfo file, string directory) {
			// Combine with the previous top directory to preserve class name.
			string newDir = Path.Combine(directory, file.Directory.Name);
			string newPath = Path.Combine(newDir, file.Name);
			// Move the file to its new home.
			if (!Directory.Exists(newDir))
				Directory.CreateDirectory(newDir);
			file.MoveAndOverwrite(newPath);
		}

		#endregion

		#endregion

	}
}
