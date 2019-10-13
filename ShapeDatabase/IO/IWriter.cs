using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ShapeDatabase.IO {

	/// <summary>
	/// Describes a single writer which can export certain types of object
	/// to a file to be read back later.
	/// </summary>
	/// <typeparam name="T">The type of object which can be exported.</typeparam>
	public interface IWriter<T> {

		/// <summary>
		/// A collection of formats which the current reader can export.
		/// </summary>
		ICollection<string> SupportedFormats { get; }


		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// </summary>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="location">The location where to store this object.</param>
		/// <exception cref="ArgumentNullException">If the given location is null.
		/// </exception>
		/// <exception cref="ArgumentException">If the path does not specify a location.
		/// </exception>
		void WriteFile(T type, string location);

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// </summary>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="writer">The stream to write the object out to.</param>
		/// <exception cref="ArgumentNullException">If the given writer is null.
		/// </exception>
		void WriteFile(T type, StreamWriter writer);


		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// This happens asynchronously.
		/// </summary>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="location">The location where to store this object.</param>
		/// <returns>A <see cref="Task"/> containing the asynchronous operation to see
		/// its progress.</returns>
		/// <exception cref="ArgumentNullException">If the given location is null.
		/// </exception>
		/// <exception cref="ArgumentException">If the path does not specify a location.
		/// </exception>
		Task WriteFileAsync(T type, string location);

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// This happens asynchronously.
		/// </summary>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="writer">The stream to write the object out to.</param>
		/// <returns>A <see cref="Task"/> containing the asynchronous operation to see
		/// its progress.</returns>
		/// <exception cref="ArgumentNullException">If the given writer is null.
		/// </exception>
		Task WriteFileAsync(T type, StreamWriter writer);

	}
}
