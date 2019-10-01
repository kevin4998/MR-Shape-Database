using System;
using System.Collections;
using System.Collections.Generic;

namespace ShapeDatabase.Util {

	public static class Enumerators {

		public static IEnumerator<B> FromConvert<A, B>(IEnumerator<A> enumerator,
													   Func<A, B> converter) {
			return new ConvertEnumerator<A, B>(enumerator, converter);
		}

	}

	class ConvertEnumerator<A, B> : IEnumerator<B> {

		private Func<A, B> Converter { get; }
		private IEnumerator<A> Enumerator { get; }

		public B Current => Converter(Enumerator.Current);

		object IEnumerator.Current => Current;


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
