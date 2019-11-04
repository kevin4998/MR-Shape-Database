using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Features.Statistics;
using ShapeDatabase.Properties;
using ShapeDatabase.Query;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.IO {

	/// <summary>
	/// A manager which is responsible for reading and writing values to files.
	/// </summary>
	class IOManager {

		#region --- Properties ---

		/// <summary>
		/// The different file formats which can be read by the readers.
		/// </summary>
		public ISet<string> AllReaderFormats {
			get {
				ISet<string> formats = new HashSet<string>();
				foreach (IDictionary<string, IReader> readers in Readers.Values)
					formats.UnionWith(readers.Keys);
				return formats;
			}
		}


		private IDictionary<Type, IDictionary<string, IWriter>> Writers { get; }
			= new Dictionary<Type, IDictionary<string, IWriter>>();
		private IDictionary<Type, IDictionary<string, IReader>> Readers { get; }
			= new Dictionary<Type, IDictionary<string, IReader>>();

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Creates a new manager for the IO reading/writing system.
		/// </summary>
		public IOManager() {
			PopulateReaders(this);
			PopulateWriters(this);
		}

		#endregion

		#region --- Instance Methods ---

		#region -- Add Functionality --

		/// <summary>
		/// Provides another reader which is able to convert files into meshes.
		/// This will have effect on the next provided directories.
		/// It will not try to recover the extra filess from previous directories.
		/// </summary>
		/// <param name="readers">The readers which can convert files into meshes.</param>
		/// <exception cref="ArgumentException">If a given reader does not contain
		/// any supported file extensions.</exception>
		public void AddReaders<T>(params IReader<T>[] readers) =>
			AddX(typeof(T), Readers, reader => reader.SupportedFormats, readers);

		/// <summary>
		/// Provides another writer which can change the format in which certain
		/// information is being saved.
		/// </summary>
		/// <typeparam name="T">The type of objects which can be serialised.</typeparam>
		/// <param name="writers">The collection of writers which can now be used
		/// for serialisation purposes.</param>
		public void AddWriters<T>(params IWriter<T>[] writers) =>
			AddX(typeof(T), Writers, writer => writer.SupportedFormats, writers);

		#endregion

		#region -- Write Functionality --

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
		public bool TryWrite(string path, object value, Type type) {
			if (type == null || value == null || string.IsNullOrEmpty(path))
				return false;

			string extension = new FileInfo(path).Extension.Substring(1);
			// Check if we have a supported Writer.
			if (!TryGetWriter(extension, type, out IWriter writer))
				// We don't have anything that can deserialise the class.
				throw MissingFormatProvider(type.FullName);

			// Safely exports the object to the specified file format.
			try { writer.WriteFile(value, path); return true; }
			catch (ArgumentException) { return false; }
			catch (NotSupportedException) { return false; }
		}

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
			TryWrite(path, value, typeof(T));

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
			TryWrite(path, value, type);

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
			Write(path, value, typeof(T));

		#endregion

		#region -- Read Functionality --

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
		public bool TryRead<T>(string path, out T value) {
			if (TryRead(path, typeof(T), out object obj)
				&& obj is T) { 
				value = (T) obj;
				return true;
			}
			value = default;
			return false;
		}

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
		public bool TryRead(string path, Type type, out object value) {
			value = default;
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
				return false;

			value = default;
			try {
				value = Read(path, type);
				return true;
			} catch (ArgumentException) {
				// We don't care about the exception since it didn't work.
				// So ignore and continue.
			} catch (NotSupportedException) {
				// We don't care about the exception since it didn't work.
				// So ignore and continue.
			}
			return false;
		}

		/// <summary>
		/// Deserialises the given file into the specified object type if possible.
		/// </summary>
		/// <param name="path">The location of the file where everything should be
		/// written to.</param>
		/// <param name="type">The type of object which to deserialise.</param>
		/// <returns>The value which should be in the path.</returns>
		/// <exception cref="ArgumentNullException">If any provided parameter
		/// is <see langword="null"/>.</exception>
		public object Read(string path, Type type) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));

			string extension = new FileInfo(path).Extension;
			// Check if we have a supported Reader.
			if (TryGetReader(extension, type, out IReader reader))
				return reader.ConvertFile(path);
			// We don't have anything that can deserialise the class.
			throw MissingFormatProvider(type.FullName);
		}

		/// <summary>
		/// Deserialises the given file into the specified object type if possible.
		/// </summary>
		/// <typeparam name="T">The type of object which to deserialise.</typeparam>
		/// <param name="path">The location of the file where everything should be
		/// written to.</param>
		/// <returns>The value which should be in the path.</returns>
		public T Read<T>(string path) {
			TryRead<T>(path, out T value);
			return value;
		}

		#endregion

		#region -- Format Functionality --

		/// <summary>
		/// A collection of all the file extensions which can be read to provide
		/// the given type of object.
		/// </summary>
		/// <typeparam name="T">The type of object to retrieve.</typeparam>
		/// <returns>An <see cref="ISet{T}"/> containing all the file extensions
		/// which can be read into the given type.</returns>
		public ISet<string> SupportedReaderFormats<T>() =>
			SupportedReaderFormats(typeof(T));
		/// <summary>
		/// A collection of all the file extensions which can be read to provide
		/// the given type of object.
		/// </summary>
		/// <param name="type">The type of object to retrieve.</param>
		/// <returns>An <see cref="ISet{T}"/> containing all the file extensions
		/// which can be read into the given type.</returns>
		public ISet<string> SupportedReaderFormats(Type type) =>
			SupportedFormats(type, Readers);

		/// <summary>
		/// A collection of all the file extensions which can be used for serialising
		/// the given type of object.
		/// </summary>
		/// <typeparam name="T">The type of object to serialise.</typeparam>
		/// <returns>An <see cref="ISet{T}"/> containing all the file extensions
		/// which can be used for the given type.</returns>
		public ISet<string> SupportedWriterFormats<T>() =>
			SupportedWriterFormats(typeof(T));
		/// <summary>
		/// A collection of all the file extensions which can be used for serialising
		/// the given type of object.
		/// </summary>
		/// <param name="type">The type of object to serialise.</param>
		/// <returns>An <see cref="ISet{T}"/> containing all the file extensions
		/// which can be used for the given type.</returns>
		public ISet<string> SupportedWriterFormats(Type type) =>
			SupportedFormats(type, Writers);

		#endregion

		#region -- Helper Functionality --

		/// <summary>
		/// Adds the provided Readers or Writers to the correct complex dictionary.
		/// This abstracts all the complex storage behaviour.
		/// </summary>
		/// <typeparam name="X">The type (either IReaders or IWriters) of the X objects.
		/// </typeparam>
		/// <param name="type">The type of new X objects to save.</param>
		/// <param name="dic">The dictionary containing all the readers or writers.</param>
		/// <param name="formatsFunc">The function to get the supported formats of
		/// the readers or writers.</param>
		/// <param name="values">The new variables to save in the dictionary.</param>
		private void AddX<X>(Type type,
							 IDictionary<Type, IDictionary<string, X>> dic,
							 Func<X, IEnumerable<string>> formatsFunc,
							 params X[] values) {
			foreach (X value in values) {
				// Silently ignore null values.
				if (value == null)
					continue;

				IDictionary<string, X> exDic;
				// When we already have existing values for this type.
				if (dic.TryGetValue(type, out exDic)) {
					// Submit the new value for the specified formats.
					foreach (string extension in formatsFunc(value))
						exDic[extension] = value;
				}
				// When we don't have existing values for this type.
				else {
					exDic = new Dictionary<string, X>();
					// Submit the new value for the specified formats.
					foreach (string extension in formatsFunc(value))
						exDic[extension] = value;
					dic[type] = exDic;
				}
			}
		}

		/// <summary>
		/// A collection of all the file extensions in a given dictionary which
		/// use the provided type.
		/// </summary>
		/// <typeparam name="X">The item stored in the dictionary, irrelevant.</typeparam>
		/// <param name="type">The type of object to retrieve/serialise.</param>
		/// <param name="dic">The dictionary containing all the readers or writers.</param>
		/// <returns>An <see cref="ISet{T}"/> containing all the file extensions
		/// which can be used for the given type.</returns>
		private ISet<string> SupportedFormats<X>(
				Type type, IDictionary<Type, IDictionary<string, X>> dic) {
			ISet<string> formats = new HashSet<string>();

			foreach (KeyValuePair<Type, IDictionary<string, X>> pair in dic)
				if (pair.Key.IsAssignableFrom(type))
					formats.UnionWith(pair.Value.Keys);

			return formats;
		}

		/// <summary>
		/// Abstraction level to get the correct reader or writer from the complex
		/// dictionaries.
		/// </summary>
		/// <typeparam name="T">The type (either IReaders or IWriters) of the objects.
		/// </typeparam>
		/// <param name="extension">The file format extension which should be supported.
		/// </param>
		/// <param name="type">The type of objects which the reader or writer can support.
		/// </param>
		/// <param name="dic">The dictionary containing all the readers or writers.</param>
		/// <param name="value">The outputed reader or writer found in the dictionary.
		/// </param>
		/// <returns><see langword="true"/> if the provided reader or writer could
		/// be found in the complex dictionary.</returns>
		private bool TryGet<T>(string extension, Type type,
								IDictionary<Type, IDictionary<string, T>> dic,
								out T value) {
			value = default;
			IDictionary<string, T> exDic = null;

			// Phase 1: Search for direct implementations.
			if (!dic.TryGetValue(type, out exDic)) {
				// Phase 2: Search for inherited types. 
				foreach (Type rType in dic.Keys) {
					if (rType.IsAssignableFrom(type)) {
						exDic = dic[rType];
					}
				}
			}

			return exDic != null && exDic.TryGetValue(extension, out value);
		}

		private bool TryGetReader(string extension, Type type, out IReader reader)
			=> TryGet<IReader>(extension, type, Readers, out reader);

		private bool TryGetWriter(string extension, Type type, out IWriter writer)
			=> TryGet<IWriter>(extension, type, Writers, out writer);

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

		#region --- Static Methods ---

		private static void PopulateReaders(IOManager manager) {
			manager.AddReaders<GeometryMesh>	(GeomOffReader	.Instance);
			manager.AddReaders<FeatureManager>	(FMReader		.Instance);
			manager.AddReaders<TempSettings>	(SettingsReader	.Instance);
			manager.AddReaders<QueryResult[]>	(QueryReader	.Instance);
		}
		private static void PopulateWriters(IOManager manager) {
			manager.AddWriters<GeometryMesh>  (GeomOffWriter .Instance);
			manager.AddWriters<IMesh>		  (OFFWriter	 .Instance);
			manager.AddWriters<IRecordHolder> (RecordsWriter .Instance);
			manager.AddWriters<FeatureManager>(FMWriter		 .Instance);
			manager.AddWriters<QueryResult[]> (QueryWriter	 .Instance);
			manager.AddWriters<TempSettings>  (SettingsWriter.Instance);
		}

		#endregion

	}
}
