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

		/// <summary>
		/// Provides a new internal shapes specified at the given file location.
		/// This shape will be refined before it gets added into the library.
		/// </summary>
		/// <param name="file">The path on the current device to the file containing
		/// the shape which needs to be present in the library.</param>
		public void AddAndRefine(string file) => AddAndRefine(new FileInfo(file));
		/// <summary>
		/// Provides a new internal shapes specified at the given file location.
		/// This shape will be refined before it gets added into the library.
		/// </summary>
		/// <param name="file">The path on the current device to the file containing
		/// the shape which needs to be present in the library.</param>	
		public void AddAndRefine(FileInfo file) {
			if (ProcessedMeshes.Contains(file.Name)) return;
			AddDirect(Refine(file, Settings.ShapeFinalDir, Settings.ShapeFailedDir));
		}

		/// <summary>
		/// Provides a new internal shapes specified at the given file location.
		/// This shape will not be refined and gets added directly into the library.
		/// </summary>
		/// <param name="file">The path on the current device to the file containing
		/// the shape which needs to be present in the library.</param>
		public void AddDirect(string file) => AddDirect(new FileInfo(file));
		/// <summary>
		/// Provides a new internal shapes specified at the given file location.
		/// This shape will not be refined and gets added directly into the library.
		/// </summary>
		/// <param name="file">The path on the current device to the file containing
		/// the shape which needs to be present in the library.</param>
		public void AddDirect(FileInfo file) {
			if (ProcessedMeshes.Contains(file.Name)) return;
			Add(file, ProcessedMeshes);
		}

		#endregion

		#region -- Query Meshes --

		/// <summary>
		/// Provides a new query item specified at the given file location.
		/// This shape will be refined before it gets added into the query library.
		/// </summary>
		/// <param name="file">The path on the current device to the file containing
		/// the shape which needs to be present in the query library.</param>
		public void AddQueryAndRefine(string file) => AddQueryAndRefine(new FileInfo(file));
		/// <summary>
		/// Provides a new query item specified at the given file location.
		/// This shape will be refined before it gets added into the query library.
		/// </summary>
		/// <param name="file">The path on the current device to the file containing
		/// the shape which needs to be present in the query library.</param>
		public void AddQueryAndRefine(FileInfo file) {
			if (QueryMeshes.Contains(file.Name)) return;
			AddQueryDirect(Refine(file, null, Settings.ShapeFailedDir));
		}

		/// <summary>
		/// Provides a new query item specified at the given file location.
		/// This shape will not be refined and gets added directly into the query library.
		/// </summary>
		/// <param name="file">The path on the current device to the file containing
		/// the shape which needs to be present in the query library.</param>
		public void AddQueryDirect(string file) => AddQueryDirect(new FileInfo(file));
		/// <summary>
		/// Provides a new query item specified at the given file location.
		/// This shape will not be refined and gets added directly into the query library.
		/// </summary>
		/// <param name="file">The path on the current device to the file containing
		/// the shape which needs to be present in the query library.</param>
		public void AddQueryDirect(FileInfo file) {
			if (QueryMeshes.Contains(file.Name)) return;
			Add(file, QueryMeshes);
		}

		#endregion

		#region -- Retrieval Operations --

		/// <summary>
		/// Checks how many shapes there are with the specified class name.
		/// This will only check internal shapes and not query items.
		/// </summary>
		/// <param name="className">The name of the class whose shapes to retrieve.
		/// </param>
		/// <returns>The amount of shapes which are present in the specified class.
		/// If the class is not in the database then this will return 0.</returns>
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

		/// <summary>
		/// Attempts to retrieve the class of a specified shape.
		/// The shape will be searched in both internal and query directories
		/// starting in the specified directory for speed.
		/// </summary>
		/// <param name="shapeName">The name of the shape which needs to be retrieved.
		/// </param>
		/// <param name="queryShape">If the shape is present in the query directory.
		/// This is used to speed up retrieval by using extra knowledge.</param>
		/// <returns>The name of the class of the specified shape if it could be found,
		/// otherwise it returns <see langword="null"/>.</returns>
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

		/// <summary>
		/// Adds the given file to the specified library, creating the correct
		/// entry for easier access.
		/// </summary>
		/// <param name="file">The file, assumed not <see langword="null"/>, which
		/// will contain the shape to add to the library.</param>
		/// <param name="library">The collection of shapes which will get a new
		/// shape present in the previous file.</param>
		private void Add(FileInfo file, MeshLibrary library) =>
			library.Add(new MeshEntry(file));


		/// <summary>
		/// Performs refinement on the given file in order for same quality
		/// shapes present in the library.
		/// <para>
		/// The refinement process works as follows:
		/// <list type="number">
		///		<item><description>
		///			First of the shape will be moved to a temporary directory where
		///			all the refinement operations will be performed.
		///		</description></item>
		///		<item><description>
		///			Next the shape will be refined one step at a time for a maximum
		///			of <see cref="Settings.MaxRefineIterations"/> attempts. If the
		///			shapes is not refined at so many attempts then it is assumed
		///			that the shape can not be fixed.
		///		</description></item>
		///		<item><description>
		///			Lastly the shape will be moved to a specified directory depending
		///			on the success of the refinement operation. If the shape was
		///			successfully refined then it will be move to the
		///			<paramref name="successMap"/>, if it could not be refined then
		///			instead it will be moved to the <paramref name="failureMap"/>.
		///			If one of these maps is not specified then it will be placed
		///			back into its original folder and will stil be loaded in by the
		///			application.
		///		</description></item>
		/// </list>
		/// </para>
		/// </summary>
		/// <param name="file">Information about the file which needs to be refined.
		/// The file will be modified in the process of refining the shape and the
		/// original file will be lost.</param>
		/// <param name="successMap">The directory where the shape should be
		/// moved to after a successful refinement.</param>
		/// <param name="failureMap">The directory where the shape should be
		/// moved to after it could no longer be refined for further analysis.</param>
		/// <returns>The same file info for easier chaining and functional programming.
		/// </returns>
		private FileInfo Refine(FileInfo file,
								string successMap = null,
								string failureMap = null) {
			Refine(ref file, successMap, failureMap);
			return file;
		}
		
		/// <summary>
		/// Performs refinement on the given file in order for same quality
		/// shapes present in the library.
		/// <para>
		/// The refinement process works as follows:
		/// <list type="number">
		///		<item><description>
		///			First of the shape will be moved to a temporary directory where
		///			all the refinement operations will be performed.
		///		</description></item>
		///		<item><description>
		///			Next the shape will be refined one step at a time for a maximum
		///			of <see cref="Settings.MaxRefineIterations"/> attempts. If the
		///			shapes is not refined at so many attempts then it is assumed
		///			that the shape can not be fixed.
		///		</description></item>
		///		<item><description>
		///			Lastly the shape will be moved to a specified directory depending
		///			on the success of the refinement operation. If the shape was
		///			successfully refined then it will be move to the
		///			<paramref name="successMap"/>, if it could not be refined then
		///			instead it will be moved to the <paramref name="failureMap"/>.
		///			If one of these maps is not specified then it will be placed
		///			back into its original folder and will stil be loaded in by the
		///			application.
		///		</description></item>
		/// </list>
		/// </para>
		/// </summary>
		/// <param name="file">Information about the file which needs to be refined.
		/// The file will be modified in the process of refining the shape and the
		/// original file will be lost.</param>
		/// <param name="successMap">The directory where the shape should be
		/// moved to after a successful refinement.</param>
		/// <param name="failureMap">The directory where the shape should be
		/// moved to after it could no longer be refined for further analysis.</param>
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


		/// <summary>
		/// Moves the file to the specified directory overwriting files that
		/// are already there and preserving class structure.
		/// </summary>
		/// <param name="file">The file which will be moved to a new location.</param>
		/// <param name="directory">The directory where the file will end up in,
		/// if the file has a class name then it will be placed in this directory
		/// into a folder with the class name.</param>
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
