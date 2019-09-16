using System;
using System.IO;
using System.Threading.Tasks;

namespace ShapeDatabase.IO {

	/// <summary>
	/// Describes a simple reader which given a file converts the data
	/// into a mesh using an unstructured grid.
	/// </summary>
	public interface IReader {

		/// <summary>
		/// A collection of formats which the given reader can convert.
		/// </summary>
		string[] SupportedFormats { get; }

		/// <summary>
		/// Transforms the given file into a mesh with an unstructured grid.
		/// </summary>
		/// <param name="reader">The file containing the mesh information.</param>
		/// <returns>A mesh with an Unstructured Grid or null if it could not
		/// be converted.</returns>
		/// <exception cref="ArgumentNullException">If the given reader does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.</exception>
		object ConvertFile(StreamReader reader);

		/// <summary>
		/// Transforms the given file into a mesh with an unstructured grid.
		/// This is done asynchronously.
		/// </summary>
		/// <param name="reader">The file containing the mesh information.</param>
		/// <returns>A mesh with an Unstructured Grid or null if it could not
		/// be converted.</returns>
		/// <exception cref="ArgumentNullException">If the given reader does not exist.</exception>
		/// <exception cref="InvalidFormatException">If a file was provided
		/// which does not use one of the <see cref="SupportedFormats"/>-formats.</exception>
		Task<object> ConvertFileAsync(StreamReader reader);

	}

}
