using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Features.Statistics;
using ShapeDatabase.Properties;
using ShapeDatabase.Query;
using ShapeDatabase.Refine;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A class with the purpose of managing different files containing shapes.
	/// This is the main manager of all the different types.
	/// </summary>
	public class FileManager {

		#region --- Properties ---

		#region -- Instance Variables --

		/// <summary>
		/// The one responsible for all refinement operations.
		/// </summary>
		private RefineManager Refiner { get; } = new RefineManager();

		/// <summary>
		/// The one responsible for all the reading and writing with files.
		/// </summary>
		private IOManager IOManager { get; } = new IOManager();


		/// <summary>
		/// A collection of all the loaded meshes structured inside a library.
		/// </summary>
		public MeshLibrary ProcessedMeshes { get; } = new MeshLibrary();

		/// <summary>
		/// A collection of all the query meshes structured inside a library.
		/// </summary>
		public MeshLibrary QueryMeshes { get; } = new MeshLibrary();

		#endregion

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Creates a new manager responsible for loading files.
		/// </summary>
		public FileManager() { }

		#endregion

		#region --- Instance Methods ---

		#region -- Public Operations --

		#region - Add Functionality --

		/// <summary>
		/// Provides another reader which is able to convert files into meshes.
		/// This will have effect on the next provided directories.
		/// It will not try to recover the extra filess from previous directories.
		/// </summary>
		/// <param name="readers">The readers which can convert files into meshes.</param>
		/// <exception cref="ArgumentException">If a given reader does not contain
		/// any supported file extensions.</exception>
		public void AddReader<T>(params IReader<T>[] readers) =>
			IOManager.AddReaders<T>(readers);

		/// <summary>
		/// Provides another refiner which can normalise meshes for easier feature extraction.
		/// This will have effect on the next provided directories.
		/// It will not try to recover the extra filess from previous directories.
		/// </summary>
		/// <param name="refiners">The refiner which can normalise a shape in any way.</param>
		public void AddRefiner(params IRefiner<IMesh>[] refiners) =>
			Refiner.AddRefiners(refiners);

		/// <summary>
		/// Provides another writer which can change the format in which certain
		/// information is being saved.
		/// </summary>
		/// <typeparam name="T">The type of objects which can be serialised.</typeparam>
		/// <param name="writers">The collection of writers which can now be used
		/// for serialisation purposes.</param>
		public void AddWriter<T>(params IWriter<T>[] writers) =>
			IOManager.AddWriters<T>(writers);

		#endregion

		#region - Directory Functionality -

		/// <summary>
		/// Secure the specified location as a directory containing new shapes
		/// for this application. The program will first check and possibly
		/// refine/normalise the given shapes before execution.
		/// </summary>
		/// <param name="filedir">The location on your device which will be used
		/// for shapes in this database.</param>
		/// <param name="async">If the given method may make use of asynchronous
		/// operations.</param>
		/// <exception cref="ArgumentNullException">If the given directory is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given directory does not exist.
		/// </exception>
		public void AddDirectory(string filedir, bool async = false) {
			if (string.IsNullOrEmpty(filedir))
				throw new ArgumentNullException(nameof(filedir));

			// Phase 1: Discover files.

			FileInfo[] files = DiscoverFiles(new DirectoryInfo(filedir));
			InfoMesh[] filemeshes;
			byte attempts = 0;

			while (files.Length != 0 && attempts++ < Settings.RefinementThreshold) {

				// Phase 2: Process files into meshes. (repeatable)

				filemeshes = async ? ReadFilesAsync(files) : ReadFiles(files);

				// Phase 3: Refine meshes into better examples. (repeatable)

				files = async ? RefineFilesAsync(filemeshes) : RefineFiles(filemeshes);

			}

			// Phase 4: Bin the files which could not be refined.

			FailedShapes(files);

			// Phase 5: Read final meshes.

			DirectoryInfo finalDir = new DirectoryInfo(Settings.ShapeFinalDir);
			if (!finalDir.Exists)
				finalDir.Create();
			AddDirectoryDirect(finalDir.FullName, async);
		}

		/// <summary>
		/// Secure the specified location as a directory containing already
		/// processed shapes for this application. These space will override
		/// currently stored shapes in the case of a collision.
		/// </summary>
		/// <param name="filedir">The location on your device which will be used
		/// for shapes in this database.</param>
		/// <param name="async">>If the given method may make use of asynchronous
		/// operations.</param>
		/// <exception cref="ArgumentNullException">If the given directory is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given directory does not exist.
		/// </exception>
		public void AddDirectoryDirect(string filedir, bool async = true) {
			if (string.IsNullOrEmpty(filedir))
				throw new ArgumentNullException(nameof(filedir));

			// Phase 1: Discover files.
			FileInfo[] files = DiscoverFiles(new DirectoryInfo(filedir));
			// Phase 2: Process files into meshes.
			InfoMesh[] filemeshes = async ? ReadFilesAsync(files) : ReadFiles(files);
			// Phase 3: Store meshes into memory.
			foreach (InfoMesh infomesh in filemeshes) {
				MeshEntry entry =
					new MeshEntry(infomesh.Info.NameWithoutExtension(),
								  infomesh.Info.Directory.Name,
								  infomesh.Mesh);
				ProcessedMeshes.Add(entry, false);
			}
		}

		/// <summary>
		/// Secure the specified location as a directory containing query shapes
		/// for this application. The program will first check and possibly
		/// refine/normalise the given shapes before loading them in memory.
		/// </summary>
		/// <param name="filedir">The location on your device which will be used
		/// for shapes in this database.</param>
		/// <param name="async">If the given method may make use of asynchronous
		/// operations.</param>
		/// <exception cref="ArgumentNullException">If the given directory is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given directory does not exist.
		/// </exception>
		public void AddQueryDirectory(string filedir, bool async = false) {
			if (string.IsNullOrEmpty(filedir))
				throw new ArgumentNullException(nameof(filedir));

			// Phase 1: Discover query files.

			FileInfo[] Files = DiscoverFiles(new DirectoryInfo(filedir));

			// Phase 2: Process files into meshes.

			InfoMesh[] QueryMeshes = async ? ReadFilesAsync(Files) : ReadFiles(Files);

			InfoMesh[] RefinedQueryMeshes = QueryMeshes;

			// Phase 3: Refine query meshes in parallel.

			Parallel.For(0, QueryMeshes.Length, i => {
				InfoMesh TempMesh = QueryMeshes[i];
				int Attempt = 0;

				while (Attempt++ < Settings.RefinementThreshold)
					if (!Refiner.RefineFile(TempMesh.Info, TempMesh.Mesh))
						TempMesh = ReadFile(TempMesh.Info);

				RefinedQueryMeshes[i] = TempMesh;
			});

			// Phase 4: Add refined query meshes to library.
			foreach (InfoMesh mesh in RefinedQueryMeshes) {
				MeshEntry entry =
					new MeshEntry(mesh.Info.NameWithoutExtension(),
								  mesh.Info.Directory.Name,
								  mesh.Mesh);
				this.QueryMeshes.Add(entry, false);
			}
		}

		#endregion

		#region - Read and Writing -

		/// <summary>
		/// Serialises the given object to the specified path if possible.
		/// </summary>
		/// <param name="path">The location of the file where everything should be
		/// written to.</param>
		/// <param name="value">The value which should be in the path.</param>
		/// <param name="type">The type of object which to serialise.</param>
		/// <returns>If the value was successfully writen to the specified file.</returns>
		/// <exception cref="ArgumentNullException">If any of the given paramaters
		/// is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">If there is no writer
		/// which can deserialise the given type to the specified file type.</exception>
		public bool TryWrite(string path, object value, Type type) =>
			IOManager.TryWrite(path, value, type);

		/// <summary>
		/// Serialises the given object to the specified path if possible.
		/// </summary>
		/// <typeparam name="T">The type of object which to serialise.</typeparam>
		/// <param name="path">The location of the file where everything should be
		/// written to.</param>
		/// <param name="value">The value which should be in the path.</param>
		/// <returns>If the value was successfully writen to the specified file.</returns>
		/// <exception cref="ArgumentNullException">If any of the given paramaters
		/// is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">If there is no writer
		/// which can deserialise the given type to the specified file type.</exception>
		public bool TryWrite<T>(string path, T value) =>
			IOManager.TryWrite<T>(path, value);

		/// <summary>
		/// Serialises the given object to the specified path if possible.
		/// </summary>
		/// <param name="path">The location of the file where everything should be
		/// written to.</param>
		/// <param name="value">The value which should be in the path.</param>
		/// <param name="type">The type of object which to serialise.</param>
		/// <exception cref="ArgumentNullException">If any of the given paramaters
		/// is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">If there is no writer
		/// which can deserialise the given type to the specified file type.</exception>
		public void Write(string path, object value, Type type) =>
			IOManager.Write(path, value, type);

		/// <summary>
		/// Serialises the given object to the specified path if possible.
		/// </summary>
		/// <typeparam name="T">The type of object which to serialise.</typeparam>
		/// <param name="value">The value which should be in the path.</param>
		/// <param name="path">The location of the file where everything should be
		/// written to.</param>
		/// <exception cref="ArgumentNullException">If any of the given paramaters
		/// is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">If there is no writer
		/// which can deserialise the given type to the specified file type.</exception>
		public void Write<T>(string path, T value) =>
			IOManager.Write<T>(path, value);



		/// <summary>
		/// Deserialises the given file into the specified object type if possible.
		/// </summary>
		/// <typeparam name="T">The type of object which to deserialise.</typeparam>
		/// <param name="path">The location of the file where everything should be
		/// read from.</param>
		/// <param name="value">The deserialised object where all information
		/// comes from the specified file.</param>
		/// <returns>If the value was successfully deserialised.</returns>
		/// <exception cref="ArgumentNullException">If the specified path does
		/// not exist or is <see langword="null"/>.</exception>
		public bool TryRead<T>(string path, out T value) =>
			IOManager.TryRead<T>(path, out value);

		/// <summary>
		/// Deserialises the given file into the specified object type if possible.
		/// </summary>
		/// <param name="path">The location of the file where everything should be
		/// read from.</param>
		/// <param name="type">The type of object which to deserialise.</param>
		/// <param name="value">he deserialised object where all information
		/// comes from the specified file.</param>
		/// <returns>If the value was successfully deserialised.</returns>
		/// <exception cref="ArgumentNullException">If the specified path does
		/// not exist or is <see langword="null"/>.</exception>
		public bool TryRead(string path, Type type, out object value) =>
			IOManager.TryRead(path, type, out value);

		/// <summary>
		/// Deserialises the given file into the specified object type if possible.
		/// </summary>
		/// <param name="path">The location of the file where everything should be
		/// written to.</param>
		/// <param name="type">The type of object which to deserialise.</param>
		/// <returns>The value which should be in the path.</returns>
		/// <exception cref="ArgumentNullException">If any provided parameter
		/// is <see langword="null"/>.</exception>
		public object Read(string path, Type type) => IOManager.Read(path, type);

		/// <summary>
		/// Deserialises the given file into the specified object type if possible.
		/// </summary>
		/// <typeparam name="T">The type of object which to deserialise.</typeparam>
		/// <param name="path">The location of the file where everything should be
		/// written to.</param>
		/// <returns>The value which should be in the path.</returns>
		public T Read<T>(string path) => IOManager.Read<T>(path);

		#endregion

		#endregion

		#region -- Private Phases --

		/// <summary>
		/// Looks through the specified directory and subdirectories for all
		/// the different files which can be loaded by the current FileManager.
		/// </summary>
		/// <param name="directory">The main directory to search for files to convert.
		/// </param>
		/// <returns>An array of all the files in the directory which can be read.
		/// </returns>
		/// <exception cref="ArgumentNullException">if the given directory is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">if the given directry does not exist.
		/// </exception>
		private FileInfo[] DiscoverFiles(DirectoryInfo directory) {
			if (directory == null)
				throw new ArgumentNullException(nameof(directory));
			if (!directory.Exists)
				throw new ArgumentException(
					Resources.EX_Directoy_NotExist,
					directory.FullName);

			ISet<string> formats = IOManager.SupportedReaderFormats;
			Queue<DirectoryInfo> pdirs  = new Queue<DirectoryInfo>();
			Queue<FileInfo>      pfiles = new Queue<FileInfo>();

			pdirs.Enqueue(directory);
			while (pdirs.Count != 0) {
				DirectoryInfo dir = pdirs.Dequeue();

				DirectoryInfo[] dirs = dir.GetDirectories();
				foreach (DirectoryInfo subdir in dirs)
					pdirs.Enqueue(subdir);

				FileInfo[] files  = dir.GetFiles();
				foreach (FileInfo file in files) {
					string extension = file.Extension
											.Substring(1)
											.ToLower(Settings.Culture);
					if (formats.Contains(extension))
						pfiles.Enqueue(file);
				}

			}

			return pfiles.ToArray();
		}


		/// <summary>
		/// Reads all the given files and returns the files together with
		/// their converted mesh. This method will only return the files
		/// which could be successfully read. So the returned array will
		/// be smaller or of the same size as the input array.
		/// This method is performed synchronously.
		/// </summary>
		/// <param name="files">A collection of files to be converted.</param>
		/// <returns>An array containing all the successfully converted meshes.
		/// This array will be smaller or of the same size as the input array.</returns>
		private InfoMesh[] ReadFiles(params FileInfo[] files) {
			if (files == null || files.Length == 0)
				return Array.Empty<InfoMesh>();

			List<InfoMesh> infoMeshes = new List<InfoMesh>(files.Length);

			foreach (FileInfo file in files) {
				InfoMesh infoMesh = ReadFile(file);
				if (InfoMesh.NULL != infoMesh)
					infoMeshes.Add(infoMesh);
			}

			return infoMeshes.ToArray();
		}

		/// <summary>
		/// Reads all the given files and returns the files together with
		/// their converted mesh. This method will only return the files
		/// which could be successfully read. So the returned array will
		/// be smaller or of the same size as the input array.
		/// This method is performed asynchronously and will not preserve file order.
		/// </summary>
		/// <param name="files">A collection of files to be converted.</param>
		/// <returns>An array containing all the successfully converted meshes.
		/// This array will be smaller or of the same size as the input array.</returns>
		private InfoMesh[] ReadFilesAsync(params FileInfo[] files) {
			if (files == null || files.Length == 0)
				return Array.Empty<InfoMesh>();

			ConcurrentBag<InfoMesh> infoMeshes = new ConcurrentBag<InfoMesh>();

			Parallel.ForEach(files, (FileInfo file) => {
				InfoMesh infoMesh = ReadFile(file);
				if (InfoMesh.NULL != infoMesh)
					infoMeshes.Add(infoMesh);
			});

			return infoMeshes.ToArray();
		}

		/// <summary>
		/// Converts a single file safely into a mesh using the right reader
		/// for this particular file type.
		/// </summary>
		/// <param name="file">The file that needs to be read and converted.</param>
		/// <returns>A link between the file and the mesh created from it.
		/// If it could not load the mesh for some reason then this will produce
		/// the <see cref="InfoMesh.NULL"/> property.</returns>
		/// <exception cref="ArgumentNullException">If the given file does not
		/// exist or is <see langword="null"/>.</exception>
		private InfoMesh ReadFile(FileInfo file) {
			if (file == null || !file.Exists)
				throw new ArgumentNullException(nameof(file));

			if (TryRead(file.FullName, out GeometryMesh mesh))
				return new InfoMesh(file, mesh);
			else
				return InfoMesh.NULL;
		}


		/// <summary>
		/// Refines the given meshes if they do not follow the current refiners
		/// specifications. The provided files will be moved in the process of execution.
		/// 
		/// Files which do not need to be refined will be moved to the
		/// <see cref="Settings.ShapeFinalDir"/> directory, while files who have
		/// just been refined will be moved to the <see cref="Settings.ShapeTempDir"/>
		/// directory for further processing if needed.
		/// 
		/// This process happens synchronously and file order will be preserved.
		/// </summary>
		/// <param name="filemeshes">A collection of meshes which their files
		/// which possibly need to be refined.</param>
		/// <returns>An array containing all the files which need to be checked
		/// for more refinement operations.</returns>
		private FileInfo[] RefineFiles(params InfoMesh[] filemeshes) {
			List<FileInfo> unrefinedFiles = new List<FileInfo>(filemeshes.Length);

			foreach (InfoMesh infomesh in filemeshes)
				if (!RefineFile(infomesh.Info, infomesh.Mesh))
					unrefinedFiles.Add(infomesh.Info);

			return unrefinedFiles.ToArray();
		}

		/// <summary>
		/// Refines the given meshes if they do not follow the current refiners
		/// specifications. The provided files will be moved in the process of execution.
		/// 
		/// Files which do not need to be refined will be moved to the
		/// <see cref="Settings.ShapeFinalDir"/> directory, while files who have
		/// just been refined will be moved to the <see cref="Settings.ShapeTempDir"/>
		/// directory for further processing if needed.
		/// 
		/// This process happens asynchronously and file order will not be preserved.
		/// </summary>
		/// <param name="filemeshes">A collection of meshes which their files
		/// which possibly need to be refined.</param>
		/// <returns>An array containing all the files which need to be checked
		/// for more refinement operations.</returns>
		private FileInfo[] RefineFilesAsync(params InfoMesh[] filemeshes) {
			ConcurrentBag<FileInfo> unrefinedFiles = new ConcurrentBag<FileInfo>();

			Parallel.ForEach(filemeshes,
				(InfoMesh infomesh) => {
					if (!RefineFile(infomesh.Info, infomesh.Mesh))
						unrefinedFiles.Add(infomesh.Info);
				}
			);

			return unrefinedFiles.ToArray();
		}

		/// <summary>
		/// Refines the given mesh from the specified file if it does not follow
		/// the refiners specifications. The file will be moved to the right
		/// directory depending on the need for refinement.
		/// <para>
		/// Files which do not need to be refined will be moved to the specified
		/// <see cref="Settings.ShapeFinalDir"/> directory, while files who have
		/// just been refined will be moved to the <see cref="Settings.ShapeTempDir"/>
		/// directory for further processing if needed.
		/// </para>
		/// </summary>
		/// <param name="info">The file where the mesh was loaded from.</param>
		/// <param name="mesh">The mesh which needs to be refined or checked.</param>
		/// <returns><see langword="true"/> if this file was already refined
		/// from the start and needs no further improvements.</returns>
		/// <exception cref="ArgumentNullException">If the given file info
		/// does not exist or is <see langword="null"/>.</exception>
		private bool RefineFile(FileInfo info, IMesh mesh) {
			if (info == null || !info.Exists)
				throw new ArgumentNullException(nameof(info));

			bool isRefined = Refiner.RefineFile(info, mesh);

			// If it needs refinement then we put it in temp.
			// If it does not need refinement then we put it in the final map.
			string dir = isRefined ? Settings.ShapeFinalDir : Settings.ShapeTempDir;
			string name = info.Name;

			// Combine with the previous top directory to preserve class name.
			dir = Path.Combine(dir, info.Directory.Name);//.Replace('\\', '/');
			name = Path.Combine(dir, name);
			// Move the file to its new home.
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			info.MoveAndOverwrite(name);

			return isRefined;
		}


		/// <summary>
		/// Moves all the given <see cref="FileInfo"/> files into the
		/// <see cref="Settings.ShapeFailedDir"/> directory for possible
		/// later fixes.
		/// </summary>
		/// <param name="files">A collection of files which needs to be moved.</param>
		private static void FailedShapes(params FileInfo[] files) {
			foreach (FileInfo info in files) {
				if (info == null)
					continue;

				string dir = Settings.ShapeFailedDir;
				dir = Path.Combine(dir, info.Directory.Name);
				string name = info.Name;

				Directory.CreateDirectory(dir);
				info.MoveAndOverwrite($"{dir}/{name}");
			}
		}

		/// <summary>
		/// Prepares an exception for if a certain file could not be read or written to.
		/// </summary>
		/// <param name="path">The path where to save or restore from.</param>
		/// <returns>The exception to throw for this file.</returns>
		private static NotSupportedException MissingFormatProvider(string path) {
			return new NotSupportedException(
				string.Format(
					Settings.Culture,
					Resources.EX_Not_Supported,
					path
				)
			);
		}

		#endregion

		#endregion

	}

	/// <summary>
	/// A simple struct to combine a mesh with its origin file.
	/// </summary>
	[DebuggerDisplay("{Info.Name} - Vertices:{Mesh.VerticesCount}")]
	internal struct InfoMesh : IEquatable<InfoMesh> {

		#region --- Properties ---

		/// <summary>
		/// An InfoMesh to describe when no actual mesh/file could be delivered.
		/// This is a <see langword="null"/> variant for this struct.
		/// </summary>
		public static readonly InfoMesh NULL = new InfoMesh(null, null, true);

		/// <summary>
		/// Specifies if the this object represents the null variant for structs.
		/// </summary>
		private bool IsNull { get; }
		/// <summary>
		/// The file where the original mesh was stored.
		/// </summary>
		public FileInfo Info { get; }
		/// <summary>
		/// The mesh loaded from the specified file.
		/// </summary>
		public IMesh Mesh { get; }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Links the given file and mesh together.
		/// </summary>
		/// <param name="file">The location where the mesh is stored.</param>
		/// <param name="mesh">The shape which was loaded from the file.</param>
		public InfoMesh(FileInfo file, IMesh mesh) : this() {
			this.Info = file;
			this.Mesh = mesh;
			IsNull = false;
		}

		private InfoMesh(FileInfo file, IMesh mesh, bool isNull)
			: this(file, mesh) {
			IsNull = isNull;
		}

		#endregion

		#region --- Instance Methods ---

		public override bool Equals(object obj) {
			return obj is InfoMesh && Equals((InfoMesh) obj);
		}

		public bool Equals(InfoMesh obj) {
			return this.IsNull
				? obj.IsNull
				: !obj.IsNull
					&& Info.Equals(obj.Info)
					&& Mesh.Equals(obj.Mesh);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			return $"{Info.ToString()} - {Mesh.ToString()}";
		}

		#endregion

		#region --- Operators ---

		public static bool operator ==(InfoMesh left, InfoMesh right) {
			return left.Equals(right);
		}

		public static bool operator !=(InfoMesh left, InfoMesh right) {
			return !left.Equals(right);
		}

		#endregion

	}

}
