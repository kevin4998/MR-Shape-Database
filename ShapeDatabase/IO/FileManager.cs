using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

		private IDictionary<FileInfo, UnstructuredMesh> FilleMesh { get; } = new Dictionary<FileInfo, UnstructuredMesh>();


		/// <summary>
		/// Creates a new manager responsible for loading files.
		/// </summary>
		public FileManager() {
			foreach (IReader<UnstructuredMesh> reader in LocalReaders.Value)
				AddReader(reader);
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

			DirectoryInfo rootInfo = new DirectoryInfo(filedir);
			if (!rootInfo.Exists)
				throw new ArgumentException(string.Format(EX_DIR_NE, filedir));

			Queue<DirectoryInfo> pdirs  = new Queue<DirectoryInfo>();
			Queue<FileInfo>		 pfiles = new Queue<FileInfo>();

			pdirs.Enqueue(rootInfo);
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

			if (async)
				ProcessFilesAsync(pfiles.ToArray());
			else
				ProcessFiles(pfiles.ToArray());



		}


		private void ProcessFiles(params FileInfo[] files) {
			if (files == null || files.Length == 0)
				return;

			foreach (FileInfo file in files) {
				string name = file.Name;
				string ext	= file.Extension.ToLower();

				if (!readers.TryGetValue(ext, out IReader<UnstructuredMesh> reader))
					continue;

				using (StreamReader stream = file.OpenText()) { 
					UnstructuredMesh mesh = reader.ConvertFile(stream);
					FileMesh[file] = mesh;
					ProcessedMeshes.Add(new MeshEntry(name, mesh));
				}

			}
		}

		private void ProcessFilesAsync(params FileInfo[] files) {
			if (files == null || files.Length == 0)
				return;

			IDictionary<string, UnstructuredMesh> meshes = new ConcurrentDictionary<string, UnstructuredMesh>();

			Parallel.ForEach(files, (FileInfo file) => {
				string name = file.Name;
				string ext  = file.Extension.ToLower();

				if (!readers.TryGetValue(ext, out IReader<UnstructuredMesh> reader))
					return;

				using (StreamReader stream = file.OpenText()) {
					UnstructuredMesh mesh = reader.ConvertFile(stream);
					meshes.Add(name, mesh);
				}

			});

			foreach (KeyValuePair<string, UnstructuredMesh> pair in meshes)
				ProcessedMeshes.Add(new MeshEntry(pair.Key, pair.Value));
		}


		private void RefineFiles(params Tuple<FileInfo, UnstructuredMesh>[] filemeshes) {
			foreach((FileInfo info, UnstructuredMesh mesh) in filemeshes)
				RefineFile(info, mesh);
		}

		private void RefineFile(FileInfo info, UnstructuredMesh mesh) {
			bool refined = false;
			foreach (IRefiner<UnstructuredMesh> refiner in refiners) {
				if (refiner.RequireRefinement(mesh)) {
					refiner.RefineMesh(info);
					refined = true;
					break;
				}
			}

			// If it is not refined, move it to our final map.
			if (!refined) {
				string dir = Settings.ShapeFinalDir;
				string name = info.Name;
				string ext = info.Extension;

				Directory.CreateDirectory(dir);
				info.MoveTo($"{dir}/{name}.{ext}");
			}
		}

		private void RefineFilesAsync();

	}
}
