using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ShapeDatabase.IO {

	/// <summary>
	/// Describes a simple reader which given a file converts the data
	/// into a specified object format.
	/// </summary>
	public interface IReader {

		/// <summary>
		/// A collection of formats which the given reader can convert.
		/// </summary>
		ICollection<string> SupportedFormats { get; }

		/// <summary>
		/// Transforms the given file into an object on the current thread.
		/// </summary>
		/// <param name="reader">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.</exception>
		object ConvertFile(StreamReader reader);

	}

	/// <summary>
	/// Describes a simple reader which given a file converts the data
	/// into a specified object format.
	/// </summary>
	/// <typeparam name="T">The object which will be converted from the file.
	/// </typeparam>
	public interface IReader<T> : IReader {

		/// <summary>
		/// Transforms the given file into an object on the current thread.
		/// </summary>
		/// <param name="reader">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.</exception>
		new T ConvertFile(StreamReader reader);

	}

	/// <summary>
	/// A class containing extension and helper methods for readers.
	/// </summary>
	public static class ReaderEx {

		/// <summary>
		/// Checks if the current supports the provided file format.
		/// </summary>
		/// <param name="reader">The reader object which converts the file.</param>
		/// <param name="extension">The extension of the file.</param>
		/// <returns>If the reader can convert the provided file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader or extension
		/// does not exist.</exception>
		public static bool Supports(this IReader reader, string extension) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (string.IsNullOrEmpty(extension))
				throw new ArgumentNullException(nameof(extension));

			if (extension.Length != 0 || extension[0] == '.')
				extension = extension.Substring(1);
			ICollection<string> formats = reader.SupportedFormats;
			foreach (string format in formats)
				if (string.Equals(format, extension, StringComparison.InvariantCultureIgnoreCase))
					return true;
			return false;
		}


		/// <summary>
		/// Transforms the given file into an object on the current thread.
		/// </summary>
		/// <param name="reader">The reader object which converts the file.</param>
		/// <param name="location">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader or stream
		/// does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.
		/// </exception>
		public static object ConvertFile(this IReader reader, string location) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException(nameof(location));

			using (StreamReader stream = new StreamReader(location)) {
				return reader.ConvertFile(stream);
			}
		}

		/// <summary>
		/// Transforms the given file into an object.
		/// This is done asynchronously.
		/// </summary>
		/// <param name="reader">The reader object which converts the file.</param>
		/// <param name="location">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader or stream
		/// does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.
		/// <returns></returns>
		public static Task<object> ConvertFileAsync(this IReader reader, string location) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException(nameof(location));

			using (StreamReader stream = new StreamReader(location)) {
				return reader.ConvertFileAsync(stream);
			}
		}

		/// <summary>
		/// Transforms the given file into an object.
		/// This is done asynchronously.
		/// </summary>
		/// <param name="reader">The reader object which converts the file.</param>
		/// <param name="stream">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader or stream
		/// does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.
		/// </exception>
		public static Task<object> ConvertFileAsync(this IReader reader,
													StreamReader stream) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			return Task.Run(() => reader.ConvertFile(stream));
		}


		/// <summary>
		/// Transforms the given file into an object on the current thread.
		/// </summary>
		/// <typeparam name="T">The type of object to deserialise.</typeparam>
		/// <param name="reader">The reader object which converts the file.</param>
		/// <param name="location">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader or stream
		/// does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.
		/// </exception>
		public static T ConvertFile<T>(this IReader<T> reader, string location) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException(nameof(location));

			using (StreamReader stream = new StreamReader(location)) {
				return reader.ConvertFile(stream);
			}
		}

		/// <summary>
		/// Transforms the given file into an object.
		/// This is done asynchronously.
		/// </summary>
		/// <typeparam name="T">The type of object to deserialise.</typeparam>
		/// <param name="reader">The reader object which converts the file.</param>
		/// <param name="location">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader or stream
		/// does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.
		/// </exception>
		public static Task<T> ConvertFileAsync<T>(this IReader<T> reader, string location) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException(nameof(location));

			using (StreamReader stream = new StreamReader(location)) {
				return reader.ConvertFileAsync(stream);
			}
		}

		/// <summary>
		/// Transforms the given file into an object.
		/// This is done asynchronously.
		/// </summary>
		/// <typeparam name="T">The type of object to deserialise.</typeparam>
		/// <param name="reader">The reader object which converts the file.</param>
		/// <param name="stream">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader or stream
		/// does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.
		/// </exception>

		public static Task<T> ConvertFileAsync<T>(this IReader<T> reader,
												  StreamReader stream) {
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			return Task.Run(() => reader.ConvertFile(stream));
		}

	}

}
