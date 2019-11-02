using System;
using System.Collections;
using System.Collections.Generic;

namespace ShapeDatabase.Util {

	/// <summary>
	/// A collection of utility methods with respect to different enumerations and
	/// collections.
	/// </summary>
	public static class Enumerators {

		public static IEnumerable<TReturn> ConvertTo<TCurrent, TReturn>(
			this IEnumerable<TCurrent> enumerable, Converter<TCurrent, TReturn> converter) {
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			if (converter == null)
				throw new ArgumentNullException(nameof(converter));

			foreach (TCurrent current in enumerable)
				yield return converter(current);
		}

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
		public static IEnumerator<TReturn> ConvertTo<TCurrent, TReturn>(
			this IEnumerator<TCurrent> enumerator, Func<TCurrent, TReturn> converter) {
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
		public static IEnumerator<TReturn> FromConvert<TCurrent, TReturn>(
			IEnumerator<TCurrent> enumerator, Func<TCurrent, TReturn> converter) {
			return new ConvertEnumerator<TCurrent, TReturn>(enumerator, converter);
		}

		public static IEnumerator<TReturn> CombineConvert<TCurrent, TReturn>(
			IEnumerator<TCurrent> enumerator,
			Func<TCurrent, TCurrent, TCurrent, TReturn> converter) {
			return new TripleCombineEnumerator<TCurrent, TReturn>(enumerator, converter);
		}

		public static IEnumerable<T> FromEnumerator<T>(
			IEnumerator<T> enumerator) {
			return new Enumerable<T>(enumerator);
		}

		public static IEnumerator<T> SpecifyType<T>(IEnumerator enumerator) {
			return new TypeEnumerator<T>(enumerator);
		}

		public static IEnumerator<T> Cast<T>(this IEnumerator enumerator) {
			return SpecifyType<T>(enumerator);
		}

		public static IEnumerable<T> Enumerate<T>(this IEnumerator<T> enumerator) {
			if (enumerator == null)
				throw new ArgumentNullException(nameof(enumerator));

			while (enumerator.MoveNext())
				yield return enumerator.Current;
		}

	}

	class TypeEnumerator<TGoal> : IEnumerator<TGoal> {

		private IEnumerator Base { get; }

		public TGoal Current {
			get {
				object current = Base.Current;
				return (current is TGoal) ? (TGoal) current : default;
			}
		}
		object IEnumerator.Current => Current;

		public TypeEnumerator(IEnumerator enumerator) {
			Base = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
		}

		public bool MoveNext() => Base.MoveNext();
		public void Reset() => Base.Reset();

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					// TODO: dispose managed state (managed objects).
					// Not present
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose() {
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}
		#endregion
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
			for (int i = 0; result && i < 3; i++)
				if (result = Enumerator.MoveNext())
					values[i] = Enumerator.Current;
			if (result)
				Current = Converter(values[0], values[1], values[2]);
			return result;
		}

	}

}
