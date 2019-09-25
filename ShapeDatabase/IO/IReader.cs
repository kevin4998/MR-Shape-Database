using System;
using System.IO;
using System.Threading.Tasks;

namespace ShapeDatabase.IO {

	/// <summary>
	/// Describes a simple reader which given a file converts the data
	/// into a specified object format.
	/// </summary>
	/// <typeparam name="T">The object which will be converted from the file.
	/// </typeparam>
	public interface IReader<T> {

		/// <summary>
		/// A collection of formats which the given reader can convert.
		/// </summary>
		string[] SupportedFormats { get; }

		/// <summary>
		/// Transforms the given file into an object on the current thread.
		/// </summary>
		/// <param name="reader">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.</exception>
		T ConvertFile(StreamReader reader);

		/// <summary>
		/// Transforms the given file into an object.
		/// This is done asynchronously.
		/// </summary>
		/// <param name="reader">The file containing the object data.</param>
		/// <returns>An object containing all the retrieved data from the file.</returns>
		/// <exception cref="ArgumentNullException">If the given reader does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.</exception>
		Task<T> ConvertFileAsync(StreamReader reader);

	}
}
