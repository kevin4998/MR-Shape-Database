﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ShapeDatabase.IO {

	/// <summary>
	/// Describes a single writer which can export certain types of object
	/// to a file to be read back later.
	/// </summary>
	public interface IWriter {

		/// <summary>
		/// A collection of formats which the current reader can export.
		/// </summary>
		ICollection<string> SupportedFormats { get; }

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// </summary>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="writer">The stream to write the object out to.</param>
		/// <exception cref="ArgumentNullException">If the given writer is null.
		/// </exception>
		void WriteFile(object type, StreamWriter writer);

	}

	/// <summary>
	/// Describes a single writer which can export certain types of an object
	/// to a file to be read back later.
	/// </summary>
	/// <typeparam name="T">The type of object which can be exported.</typeparam>
	public interface IWriter<T> : IWriter {

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// </summary>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="writer">The stream to write the object out to.</param>
		/// <exception cref="ArgumentNullException">If the given writer is null.
		/// </exception>
		void WriteFile(T type, StreamWriter writer);

	}

	/// <summary>
	/// A class containing extension and helper methods for writers.
	/// </summary>
	public static class WriterEx {

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// </summary>
		/// <param name="writer">The writer object which knows to which file
		/// to transport the given data.</param>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="location">The location where to store this object.</param>
		/// <exception cref="ArgumentNullException">If any given parameter is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the path does not specify a location.
		/// </exception>
		public static void WriteFile(this IWriter writer, object type, string location) {
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException(nameof(location));

			using (StreamWriter stream = new StreamWriter(location)) {
				writer.WriteFile(type, stream);
			}
		}

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// This happens asynchronously.
		/// </summary>
		/// <param name="writer">The writer object which knows to which file
		/// to transport the given data.</param>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="location">The location where to store this object.</param>
		/// <returns>A <see cref="Task"/> containing the asynchronous operation to see
		/// its progress.</returns>
		/// <exception cref="ArgumentNullException">If any given parameter is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the path does not specify a location.
		/// </exception>
		public static Task WriteFileAsync(this IWriter writer, object type,
										  string location) {
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException(nameof(location));

			return Task.Run(() => writer.WriteFile(type, location));
		}

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// This happens asynchronously.
		/// </summary>
		/// <param name="writer">The writer object which knows to which file
		/// to transport the given data.</param>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="stream">The stream to write the object out to.</param>
		/// <returns>A <see cref="Task"/> containing the asynchronous operation to see
		/// its progress.</returns>
		/// <exception cref="ArgumentNullException">If any given parameter is
		/// <see langword="null"/>.</exception>
		public static Task WriteFileAsync(this IWriter writer, object type,
										  StreamWriter stream) {
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			return Task.Run(() => writer.WriteFile(type, stream));
		}

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// </summary>
		/// <param name="writer">The writer object which knows to which file
		/// to transport the given data.</param>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="location">The location where to store this object.</param>
		/// <exception cref="ArgumentNullException">If any given parameter is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the path does not specify a location.
		/// </exception>
		public static void WriteFile<T>(this IWriter<T> writer, T type, string location) {
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException(nameof(location));

			using (StreamWriter stream = new StreamWriter(location)) {
				writer.WriteFile(type, stream);
			}
		}

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// This happens asynchronously.
		/// </summary>
		/// <param name="writer">The writer object which knows to which file
		/// to transport the given data.</param>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="location">The location where to store this object.</param>
		/// <returns>A <see cref="Task"/> containing the asynchronous operation to see
		/// its progress.</returns>
		/// <exception cref="ArgumentNullException">If any given parameter is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the path does not specify a location.
		/// </exception>
		public static Task WriteFileAsync<T>(this IWriter<T> writer, T type,
											 string location) {
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (string.IsNullOrEmpty(location))
				throw new ArgumentNullException(nameof(location));

			return Task.Run(() => writer.WriteFile(type, location));
		}

		/// <summary>
		/// Transforms the given object and exports it to a file at the specified location.
		/// This happens asynchronously.
		/// </summary>
		/// <param name="writer">The writer object which knows to which file
		/// to transport the given data.</param>
		/// <param name="type">The object which needs to be exported.</param>
		/// <param name="stream">The stream to write the object out to.</param>
		/// <returns>A <see cref="Task"/> containing the asynchronous operation to see
		/// its progress.</returns>
		/// <exception cref="ArgumentNullException">If any given parameter is
		/// <see langword="null"/>.</exception>
		public static Task WriteFileAsync<T>(this IWriter<T> writer, T type,
											 StreamWriter stream) {
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			return Task.Run(() => writer.WriteFile(type, stream));
		}
	}
}
