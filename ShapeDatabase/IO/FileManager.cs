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
		/// The one responsible for all the reading and writing with files.
		/// </summary>
		private IOManager IOManager { get; } = new IOManager();

		/// <summary>
		/// The one responsible for ordering all the meshes into the correct
		/// repositories.
		/// </summary>
		private LibraryManager Library { get; } = new LibraryManager();


		/// <summary>
		/// A collection of all the loaded meshes structured inside a library.
		/// </summary>
		public MeshLibrary ProcessedMeshes => Library.ProcessedMeshes;

		/// <summary>
		/// A collection of all the query meshes structured inside a library.
		/// </summary>
		public MeshLibrary QueryMeshes => Library.QueryMeshes;

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
			Library.Refiner.AddRefiners(refiners);

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

			// Discover files.
			FileInfo[] files = DiscoverFiles(new DirectoryInfo(filedir));
			// Add files into the library (+ refinement).
			if (async)
				Parallel.ForEach(files, file => Library.AddAndRefine(file) );
			else
				foreach (FileInfo file in files)
					Library.AddAndRefine(file);
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

			// Discover files.
			FileInfo[] files = DiscoverFiles(new DirectoryInfo(filedir));
			// Add files into the library (+ refinement).
			if (async)
				Parallel.ForEach(files, file => Library.AddDirect(file));
			else
				foreach (FileInfo file in files)
					Library.AddDirect(file);
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

			// Discover files.
			FileInfo[] files = DiscoverFiles(new DirectoryInfo(filedir));
			// Add files into the library (+ refinement).
			if (async)
				Parallel.ForEach(files, file => Library.AddQueryAndRefine(file));
			else
				foreach (FileInfo file in files)
					Library.AddQueryAndRefine(file);
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

			ISet<string> formats = IOManager.SupportedReaderFormats(typeof(IMesh));
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

		#endregion

		#endregion

	}

}
