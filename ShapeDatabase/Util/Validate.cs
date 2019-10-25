using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Util {
	
	/// <summary>
	/// A class containing methods for validating input data or arguments.
	/// </summary>
	public static class Validate {

		/// <summary>
		/// Checks if the given argument is <see langword="null"/> or not.
		/// If this is the case then a specific exception will be thrown.
		/// </summary>
		/// <typeparam name="T">The type of object to verify for <see langword="null"/>
		/// condition. </typeparam>
		/// <param name="value">The argument which might be <see langword="null"/>
		/// or not.</param>
		/// <returns>The same object if it is not <see langword="null"/>.</returns>
		/// <exception cref="ArgumentNullException">If the given argument
		/// is <see langword="null"/>.</exception>
		public static T NonNull<T>(T value) {
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			return value;
		}

		/// <summary>
		/// Checks if the given arguments are <see langword="null"/> or not.
		/// If this is the case then a specific exception will be thrown.
		/// </summary>
		/// <typeparam name="T">The type of objects to verify for <see langword="null"/>
		/// condition. </typeparam>
		/// <param name="values">The arguments which might be <see langword="null"/>
		/// or not.</param>
		/// <returns>The same objects if it is not <see langword="null"/>.</returns>
		/// <exception cref="ArgumentNullException">If any of the given arguments
		/// is <see langword="null"/>.</exception>
		public static T[] NonNull<T>(params T[] values) {
			foreach (T value in values)
				if (value == null)
					throw new ArgumentNullException(nameof(values));
			return values;
		}

		/// <summary>
		/// Checks if the given arguments are <see langword="null"/> or not.
		/// If this is the case then a specific exception will be thrown.
		/// </summary>
		/// <typeparam name="T">The type of objects to verify for <see langword="null"/>
		/// condition. </typeparam>
		/// <param name="values">The collection of arguments which
		/// might be <see langword="null"/> or not.</param>
		/// <returns>The same objects if it is not <see langword="null"/>.</returns>
		/// <exception cref="ArgumentNullException">If any of the given arguments
		/// is <see langword="null"/>.</exception>
		public static IEnumerable<T>[] NonNull<T>(params IEnumerable<T>[] values) {
			foreach(IEnumerable<T> enums in values) { 
				if (enums == null)
					throw new ArgumentNullException(nameof(values));
				foreach(T value in enums)
					if (value == null)
						throw new ArgumentNullException(nameof(values));
			}
			return values;
		}

	}

}
