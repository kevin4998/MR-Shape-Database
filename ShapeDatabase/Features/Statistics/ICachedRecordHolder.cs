using System;
using System.Collections.Generic;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// An extension of <see cref="IRecordHolder{T}"/> to allow for caching of values
	/// which can be shared between Records.
	/// </summary>
	/// <typeparam name="T">The type of object which will be converted to
	/// <see cref="Record"/>s. This specific object will be used when calculating
	/// statistics.</typeparam>
	public interface ICachedRecordHolder<T> : IRecordHolder<T> {

		ICache Cache { get; }

	}
}
