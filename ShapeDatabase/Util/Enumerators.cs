using System;
using System.Collections;
using System.Collections.Generic;

namespace ShapeDatabase.Util {

	/// <summary>
	/// A collection of utility methods with respect to different enumerations and
	/// collections.
	/// </summary>
	public static class Enumerators {

		/// <summary>
		/// Changes the type provided from this enumerator during its usage.
		/// </summary>
		/// <typeparam name="A">The type of values which the original enumerator
		/// contains.</typeparam>
		/// <typeparam name="B">The type of values which the new enumerator provides.
		/// </typeparam>
		/// <param name="enumerator">The base class which actually provides value
		/// from a collection or other class.</param>
		/// <param name="converter">The formula to convert a value from type A to B.
		/// </param>
		/// <returns>A new enumerator over the same object but providing the given
		/// value.</returns>
		public static IEnumerator<B> ConvertTo<A,B>(this IEnumerator<A> enumerator,
														Func<A, B> converter) {
			return FromConvert(enumerator, converter);
		}

		/// <summary>
		/// A specific enumerator which converts the values of a given enumerator
		/// into another class.
		/// </summary>
		/// <typeparam name="A">The type of values which the original enumerator
		/// contains.</typeparam>
		/// <typeparam name="B">The type of values which the new enumerator provides.
		/// </typeparam>
		/// <param name="enumerator">The base class which actually provides value
		/// from a collection or other class.</param>
		/// <param name="converter">The formula to convert a value from type A to B.
		/// </param>
		/// <returns>A new enumerator over the same object but providing the given
		/// value.</returns>
		/// <exception cref="ArgumentNullException">If any of the provided parameters
		/// is equal to <see langword="null"/>.</exception>
		public static IEnumerator<B> FromConvert<A, B>(IEnumerator<A> enumerator,
													   Func<A, B> converter) {
			return new ConvertEnumerator<A, B>(enumerator, converter);
		}

	}

	/// <summary>
	/// A specific enumerator which converts the ouput value to a given type.
	/// </summary>
	/// <typeparam name="A">The type of values which the original enumerator
	/// contains.</typeparam>
	/// <typeparam name="B">The type of values which the new enumerator provides.
	/// </typeparam>
	class ConvertEnumerator<A, B> : IEnumerator<B> {

		private Func<A, B> Converter { get; }
		private IEnumerator<A> Enumerator { get; }

		public B Current => Converter(Enumerator.Current);
		object IEnumerator.Current => Current;


		/// <summary>
		/// Initialises a new enumerator which can convert output values.
		/// </summary>
		/// <param name="enumerator">The base class which actually provides value
		/// from a collection or other class.</param>
		/// <param name="converter">The formula to convert a value from type A to B.
		/// </param>
		/// <exception cref="ArgumentNullException">If any of the provided parameters
		/// is equal to <see langword="null"/>.</exception>
		public ConvertEnumerator(IEnumerator<A> enumerator, Func<A, B> converter) {
			Converter = converter ?? throw new ArgumentNullException(nameof(converter));
			Enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
		}


		public void Dispose() {
			Enumerator.Dispose();
		}

		public bool MoveNext() {
			return Enumerator.MoveNext();
		}

		public void Reset() {
			Enumerator.Reset();
		}

	}

}
