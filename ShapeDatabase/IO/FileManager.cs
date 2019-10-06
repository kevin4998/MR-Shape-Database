using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

		#region -- Error Messages --

		private const string EX_NO_EXT = "No extension where provided for reader \'{0}\'.";
		private const string EX_DIR_NE = "The provided directory does not exist \'{0}\'.";
		private const string EX_FORMAT = "Could not load file '{0}' because of an Exception.";

		#endregion

		#region -- Static Variables --

		/// <summary>
		/// The maximum amount of attempts that a shape may be refined
		/// before seeing it as unfixable.
		/// </summary>
		private const byte REFINEMENT_THRESHOLD = 16;

		private static readonly Lazy<IReader <GeometryMesh>[]> LocalReaders =
			new Lazy<IReader<GeometryMesh>[]>(ProduceReaders);
		private static readonly Lazy<IRefiner<IMesh>[]> LocalRefiners =
			new Lazy<IRefiner<IMesh>[]>(ProduceRefiners);
		private static readonly Lazy<IWriter<IMesh>> LocalWriter =
			new Lazy<IWriter<IMesh>>(ProduceWriter);

		private static IReader<GeometryMesh>[] ProduceReaders() {
			return new IReader<GeometryMesh>[] {
				GeomOffReader.Instance
			};
		}
		private static IRefiner<IMesh>[] ProduceRefiners() {
			return new IRefiner<IMesh>[] {
				ExtendRefiner.Instance,
				SimplifyRefiner.Instance
			};
		}

		private static IWriter<IMesh> ProduceWriter()
		{
			return OFFWriter.Instance;
		}

		#endregion

		#region -- Instance Variables --

		private readonly ISet<string> formats = new HashSet<string>();
		private readonly IDictionary<string, IReader<GeometryMesh>> readers =
			new Dictionary<string, IReader<GeometryMesh>>();
		private readonly ICollection<IRefiner<IMesh>> refiners =
			new List<IRefiner<IMesh>>(LocalRefiners.Value);
		private readonly IWriter<IMesh> writer = LocalWriter.Value;


		/// <summary>
		/// A collection of all the loaded meshes sturctured inside a library.
		/// </summary>
		public MeshLibrary ProcessedMeshes { get; } = new MeshLibrary();

		#endregion

		#endregion

		#region --- Constructor Methods ---


		/// <summary>
		/// Creates a new manager responsible for loading files.
		/// </summary>
		public FileManager() {
			foreach (IReader<GeometryMesh> reader in LocalReaders.Value)
				AddReader(reader);
			// Refiners added automatically.
		}

		#endregion

		#region --- Instance Methods ---

		#region -- Public Operations --

		/// <summary>
		/// Provides another reader which is able to convert files into meshes.
		/// This will have effect on the next provided directories.
		/// It will not try to recover the extra filess from previous directories.
		/// </summary>
		/// <param name="readers">The readers which can convert files into meshes.</param>
		/// <exception cref="ArgumentException">If a given reader does not contain
		/// any supported file extensions.</exception>
		public void AddReader(params IReader<GeometryMesh>[] readers) {
			if (readers == null || readers.Length == 0)
				return;

			foreach (IReader<GeometryMesh> reader in readers)
				if (reader != null)
					foreach (string format in reader.SupportedFormats) {
						if (format == null || format.Length == 0)
							throw new ArgumentException(EX_NO_EXT, reader.GetType().FullName);
						string ext = format.ToLower();
						if (ext[0] != '.')
							ext = '.' + ext;

						formats.Add(ext);
						this.readers.Add(ext, reader);
					}

		}

		/// <summary>
		/// Provides another refiner which can normalise meshes for easier feature extraction.
		/// This will have effect on the next provided directories.
		/// It will not try to recover the extra filess from previous directories.
		/// </summary>
		/// <param name="refiners">The refiner which can normalise a shape in any way.</param>
		public void AddRefiner(params IRefiner<IMesh>[] refiners) {
			if (refiners == null || refiners.Length == 0)
				return;

			foreach(IRefiner<IMesh> refine in refiners)
				if (refine != null && !this.refiners.Contains(refine))
					this.refiners.Add(refine);
		}


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
		public void AddDirectory(string filedir, bool async = true) {
			if (string.IsNullOrEmpty(filedir))
				throw new ArgumentNullException(nameof(filedir));

			// Phase 1: Discover files.

			FileInfo[] files = DiscoverFiles(new DirectoryInfo(filedir));
			InfoMesh[] filemeshes;
			byte attempts = 0;

			while (files.Length != 0 && attempts++ < REFINEMENT_THRESHOLD) { 

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
				ProcessedMeshes.Add(entry, true);
			}
		}

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
				throw new ArgumentException(string.Format(EX_DIR_NE, directory.FullName));

			Queue<DirectoryInfo> pdirs  = new Queue<DirectoryInfo>();
			Queue<FileInfo>      pfiles = new Queue<FileInfo>();

			pdirs.Enqueue(directory);
			while (pdirs.Count != 0) {
				DirectoryInfo dir = pdirs.Dequeue();

				DirectoryInfo[] dirs = dir.GetDirectories();
				foreach (DirectoryInfo subdir in dirs)
					pdirs.Enqueue(subdir);

				FileInfo[] files  = dir.GetFiles();
				foreach (FileInfo file in files)
					if (formats.Contains(file.Extension.ToLower()))
						pfiles.Enqueue(file);

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
				if (InfoMesh.NULL != infoMesh) infoMeshes.Add(infoMesh);
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
				else
				{
					;
				}
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

			if (!this.readers.TryGetValue(file.Extension.ToLower(),
										  out IReader<GeometryMesh> reader))
				return InfoMesh.NULL;

			using (StreamReader stream = file.OpenText()) {
				try {
					IMesh mesh = reader.ConvertFile(stream);
					return new InfoMesh(file, mesh);
				} catch (InvalidFormatException ex) {
					Console.WriteLine(EX_FORMAT, file.FullName);
					Console.WriteLine(ex);
					return InfoMesh.NULL;
				}
			}
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

			foreach(InfoMesh infomesh in filemeshes)
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
		/// 
		/// Files which do not need to be refined will be moved to the specified
		/// <see cref="Settings.ShapeFinalDir"/> directory, while files who have
		/// just been refined will be moved to the <see cref="Settings.ShapeTempDir"/>
		/// directory for further processing if needed.
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

			bool isRefined = true;	// Holds if no refinement has been made.
			foreach (IRefiner<IMesh> refiner in refiners) {
				if (refiner.RequireRefinement(mesh)) {
					refiner.RefineMesh(mesh, info);
					isRefined = false;
					break;
				}
			}

			// If it needs refinement then we put it in temp.
			// If it does not need refinement then we put it in the final map.
			string dir = isRefined ? Settings.ShapeFinalDir : Settings.ShapeTempDir;
			dir = Path.Combine(dir, info.Directory.Name);
			string name = info.Name;
			//string ext = info.Extension;

			string newDir = $"{dir}/{name}";
			Directory.CreateDirectory(dir);
			info.MoveAndOverwrite(newDir);

			return isRefined;
		}


		/// <summary>
		/// Moves all the given <see cref="FileInfo"/> files into the
		/// <see cref="Settings.ShapeFailedDir"/> directory for possible
		/// later fixes.
		/// </summary>
		/// <param name="files">A collection of files which needs to be moved.</param>
		private void FailedShapes(params FileInfo[] files) {
			foreach (FileInfo info in files) { 
				if (info == null) continue;

				string dir = Settings.ShapeFailedDir;
				dir = Path.Combine(dir, info.Directory.Name);
				string name = info.Name;

				Directory.CreateDirectory(dir);
				info.MoveAndOverwrite($"{dir}/{name}");
			}
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
			:this(file, mesh) {
			IsNull = isNull;
		}

		#endregion

		#region --- Instance Methods ---

		public override bool Equals(object obj) {
			return obj is InfoMesh && Equals((InfoMesh) obj);
		}

		public bool Equals(InfoMesh obj) {
			return this.IsNull
				?   obj.IsNull
				:  !obj.IsNull
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

		public static bool operator == (InfoMesh left, InfoMesh right) {
			return left.Equals(right);
		}

		public static bool operator != (InfoMesh left, InfoMesh right) {
			return !left.Equals(right);
		}

		#endregion

	}

}
