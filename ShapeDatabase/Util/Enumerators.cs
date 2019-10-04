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

		public static IEnumerator<B> CombineConvert<A, B>(IEnumerator<A> enumerator,
														Func<A, A, A, B> converter) {
			return new TripleCombineEnumerator<A, B>(enumerator, converter);
		}

		public static IEnumerable<A> FromEnumerator<A>(IEnumerator<A> enumerator) {
			return new Enumerable<A>(enumerator);
		}

	}

	class Enumerable<T> : IEnumerable<T> {

		private IEnumerator<T> Enumerator { get; }

		public IEnumerator<T> GetEnumerator() => Enumerator;
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public Enumerable(IEnumerator<T> enumerator) {
			Enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
		}

	}

	abstract class BaseEnumerator<TBase, TGoal> : IEnumerator<TGoal> {

		protected IEnumerator<TBase> Enumerator { get; }

		object IEnumerator.Current => Current;
		public abstract TGoal Current { get; protected set; }


		public BaseEnumerator(IEnumerator<TBase> enumerator) {
			Enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
		}


		public virtual void Dispose() {
			Enumerator.Dispose();
		}
		public virtual bool MoveNext() {
			return Enumerator.MoveNext();
		}
		public virtual void Reset() {
			Enumerator.Reset();
		}

	}

	/// <summary>
	/// A specific enumerator which converts the ouput value to a given type.
	/// </summary>
	/// <typeparam name="TBase">The type of values which the original enumerator
	/// contains.</typeparam>
	/// <typeparam name="TGoal">The type of values which the new enumerator provides.
	/// </typeparam>
	class ConvertEnumerator<TBase, TGoal> : BaseEnumerator<TBase, TGoal> {

		private Func<TBase, TGoal> Converter { get; }
		public override TGoal Current {
			get {
				return Converter(Enumerator.Current);
			}
			protected set {
				throw new InvalidOperationException();
			}
		}


		/// <summary>
		/// Initialises a new enumerator which can convert output values.
		/// </summary>
		/// <param name="enumerator">The base class which actually provides value
		/// from a collection or other class.</param>
		/// <param name="converter">The formula to convert a value from type A to B.
		/// </param>
		/// <exception cref="ArgumentNullException">If any of the provided parameters
		/// is equal to <see langword="null"/>.</exception>
		public ConvertEnumerator(IEnumerator<TBase> enumerator, Func<TBase, TGoal> converter) 
			: base(enumerator) {
			Converter = converter ?? throw new ArgumentNullException(nameof(converter));
		}

	}

	class TripleCombineEnumerator<TBase, TGoal> : BaseEnumerator<TBase, TGoal> {

		private Func<TBase, TBase, TBase, TGoal> Converter { get; }
		public override TGoal Current { get; protected set; } = default;

		public TripleCombineEnumerator(IEnumerator<TBase> enumerator,
									   Func<TBase, TBase, TBase, TGoal> converter) 
			: base(enumerator) {
			Converter = converter ?? throw new ArgumentNullException(nameof(converter));
		}

		public override bool MoveNext() {
			TBase[] values = new TBase[3];
			bool result = true;
			for(int i = 0; result && i < 3; i++)
				if (result = Enumerator.MoveNext())
					values[i] = Enumerator.Current;
			if (result)
				Current = Converter(values[0], values[1], values[2]);
			return result;
		}

	}

}
