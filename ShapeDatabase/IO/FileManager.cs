using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ShapeDatabase.Refine;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A class with the purpose of managing different files containing shapes.
	/// This is the main manager of all the different types.
	/// </summary>
	public class FileManager {

		private const string EX_DIR_NE = "The provided directory does not exist \'{0}\'.";
		private const string EX_FORMAT = "Could not load file '{0}' because of an Exception.";
		/// <summary>
		/// The maximum amount of attempts that a shape may be refined
		/// before seeing it as unfixable.
		/// </summary>
		private const byte REFINEMENT_THRESHOLD = 4;

		private static readonly Lazy<IReader <UnstructuredMesh>[]> LocalReaders =
			new Lazy<IReader<UnstructuredMesh>[]>(ProduceReaders);
		private static readonly Lazy<IRefiner<UnstructuredMesh>[]> LocalRefiners =
			new Lazy<IRefiner<UnstructuredMesh>[]>(ProduceRefiners);

		private static IReader<UnstructuredMesh>[] ProduceReaders() {
			return new IReader<UnstructuredMesh>[] {
				OFFReader.Instance
			};
		}
		private static IRefiner<UnstructuredMesh>[] ProduceRefiners() {
			return new IRefiner<UnstructuredMesh>[] {

			};
		}

		// Pre-processing phase
		private readonly ISet<string> formats = new HashSet<string>();
		private readonly IDictionary<string, IReader<UnstructuredMesh>> readers = new Dictionary<string, IReader<UnstructuredMesh>>();
		private readonly ICollection<IRefiner<UnstructuredMesh>> refiners = new List<IRefiner<UnstructuredMesh>>(LocalRefiners.Value);

		/// <summary>
		/// A collection of all the loaded meshes sturctured inside a library.
		/// </summary>
		public MeshLibrary ProcessedMeshes { get; } = new MeshLibrary();


		/// <summary>
		/// Creates a new manager responsible for loading files.
		/// </summary>
		public FileManager() {
			foreach (IReader<UnstructuredMesh> reader in LocalReaders.Value)
				AddReader(reader);
			// Refiners added automatically.
		}


		public void AddReader(params IReader<UnstructuredMesh>[] readers) {
			if (readers == null || readers.Length == 0)
				return;

			foreach (IReader<UnstructuredMesh> reader in readers)
				foreach (string format in reader.SupportedFormats) {
					if (format == null || format.Length == 0)
						throw new ArgumentException();
					string ext = format.ToLower();
					if (ext[0] != '.')
						ext = '.' + ext;

					formats.Add(ext);
					this.readers.Add(ext, reader);
				}

		}

		public void AddRefiner(params IRefiner<UnstructuredMesh>[] refiners) {
			if (refiners == null || refiners.Length == 0)
				return;

			foreach(IRefiner<UnstructuredMesh> refine in refiners)
				if (!this.refiners.Contains(refine))
					this.refiners.Add(refine);
		}


		/// <summary>
		/// Secure the specified location as a directory containing shapes for this application.
		/// </summary>
		/// <param name="filedir">The location on your device which will be used
		/// for shapes in this database.</param>
		public void AddDirectory(string filedir, bool async = true) {
			if (string.IsNullOrEmpty(filedir))
				throw new ArgumentNullException(nameof(filedir));

			// Phase 1: Discover files

			FileInfo[] files = DiscoverFiles(new DirectoryInfo(filedir));
			InfoMesh[] filemeshes;
			byte attempts = 0;

			while (files.Length != 0 && attempts++ < REFINEMENT_THRESHOLD) { 

				// Phase 2: Process files into meshes

				filemeshes = async ? ReadFilesAsync(files) : ReadFiles(files);

				// Phase 3: Refine meshes into better examples (repeatable)

				if (async)	files = RefineFilesAsync(filemeshes);
				else		files = RefineFiles(filemeshes);

			}

			// Phase 4: Bin the files which could not be refined.
			FailedShapes(files);

			// Phase 5: Read final meshes
			SuccessShapes(async);
		}


		private FileInfo[] DiscoverFiles(DirectoryInfo directory) {
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

		private InfoMesh ReadFile(FileInfo file) {
			if (file == null || !file.Exists)
				throw new ArgumentNullException(nameof(file));

			string ext  = file.Extension.ToLower();

			if (!readers.TryGetValue(ext, out IReader<UnstructuredMesh> reader))
				return InfoMesh.NULL;

			using (StreamReader stream = file.OpenText()) {
				try { 
					UnstructuredMesh mesh = reader.ConvertFile(stream);
					return new InfoMesh(file, mesh);
				} catch (InvalidFormatException ex) {
					Console.WriteLine(EX_FORMAT, file.FullName);
					Console.WriteLine(ex);
					return InfoMesh.NULL;
				}
			}
		}


		// Returns unrefineed files.
		private FileInfo[] RefineFiles(params InfoMesh[] filemeshes) {
			List<FileInfo> unrefinedFiles = new List<FileInfo>(filemeshes.Length);

			foreach(InfoMesh infomesh in filemeshes)
				if (!RefineFile(infomesh.Info, infomesh.Mesh))
					unrefinedFiles.Add(infomesh.Info);

			return unrefinedFiles.ToArray();
		}

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

		// Returns if the file was already refined.
		private bool RefineFile(FileInfo info, UnstructuredMesh mesh) {
			bool isRefined = true;
			foreach (IRefiner<UnstructuredMesh> refiner in refiners) {
				if (refiner.RequireRefinement(mesh)) {
					refiner.RefineMesh(info);
					isRefined = false;
					break;
				}
			}

			// If it needs refinement then we put it in temp.
			// If it does not need refinement then we put it in the final map.
			string dir = isRefined ? Settings.ShapeFinalDir : Settings.ShapeTempDir;
			string name = info.Name;
			string ext = info.Extension;

			Directory.CreateDirectory(dir);
			info.MoveTo($"{dir}/{name}.{ext}");

			return isRefined;
		}


		private void FailedShapes(params FileInfo[] files) {
			foreach (FileInfo info in files) { 
				string dir = Settings.ShapeFailedDir;
				string name = info.Name;
				string ext = info.Extension;

				Directory.CreateDirectory(dir);
				info.MoveTo($"{dir}/{name}.{ext}");
			}
		}

		private void SuccessShapes(bool async) {
			DirectoryInfo finalDir = new DirectoryInfo(Settings.ShapeFinalDir);
			if (!finalDir.Exists)
				finalDir.Create();
			FileInfo[] files = DiscoverFiles(finalDir);
			InfoMesh[] filemeshes = async ? ReadFilesAsync(files) : ReadFiles(files);
			foreach (InfoMesh infomesh in filemeshes) {
				MeshEntry entry = new MeshEntry(infomesh.Info.Name, infomesh.Mesh);
				ProcessedMeshes.Add(entry);
			}
		}

	}

	[DebuggerDisplay("{Info.Name} - Vertices:{Mesh.VerticesCount}")]
	internal struct InfoMesh : IEquatable<InfoMesh> {

		public static readonly InfoMesh NULL = new InfoMesh(null, UnstructuredMesh.NULL);

		public FileInfo Info { get; }
		public UnstructuredMesh Mesh { get; }


		public InfoMesh(FileInfo file, UnstructuredMesh mesh) : this() {
			this.Info = file;
			this.Mesh = mesh;
		}


		public override bool Equals(object obj) {
			return obj is InfoMesh && Equals((InfoMesh) obj);
		}

		public bool Equals(InfoMesh obj) {
			return Object.ReferenceEquals(NULL, this)
				? Object.ReferenceEquals(NULL, obj)
				: !Object.ReferenceEquals(NULL, obj)
					&& Info.Equals(obj.Info)
					&& Mesh.Equals(obj.Mesh);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			return $"{Info.ToString()} - {Mesh.ToString()}";
		}


		public static bool operator == (InfoMesh left, InfoMesh right) {
			return left.Equals(right);
		}

		public static bool operator != (InfoMesh left, InfoMesh right) {
			return !left.Equals(right);
		}

	}

}
