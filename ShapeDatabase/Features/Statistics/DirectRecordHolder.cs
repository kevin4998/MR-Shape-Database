using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// An implementation of <see cref="IRecordHolder"/> which does not calculate
	/// <see cref="Record"/>s but insteads accepts them directly.
	/// </summary>
	/// <remarks>
	/// Threadsafe implementation.
	/// </remarks>
	public sealed class DirectRecordHolder : IRecordHolder {

		#region --- Properties ---

		private readonly ConcurrentBag<string> measureNames = new ConcurrentBag<string>();
		private ConcurrentBag<Record> records = new ConcurrentBag<Record>();

		public bool IsActive => true;
		public bool IsEmpty => records.Count == 0;

		public DateTime SnapshotTime {
			get {
				if (records.TryPeek(out Record result))
					return result.Time;
				return DateTime.MinValue;
			}
		}
		public IEnumerable<string> MeasureNames {
			get {
				string[] names = measureNames.ToArray();
				Array.Reverse(names);
				return names;
			}
		}
		public ICollection<Record> Records => records.ToArray();

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new empty record holder which will receive
		/// records in its lifetime.
		/// </summary>
		public DirectRecordHolder() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Provides a new measurement which all the records share together.
		/// Adding names is order dependent.
		/// </summary>
		/// <param name="measureName">The name of the measurement which will be
		/// contained in the records.</param>
		/// <returns>The current <see cref="DirectRecordHolder"/> for chaining
		/// purposes.</returns>
		public DirectRecordHolder AddMeasureName(string measureName) {
			this.measureNames.Add(measureName);
			return this;
		}
		/// <summary>
		/// Provides a new record which should be held by this record holder.
		/// </summary>
		/// <param name="record">The record with its calculated metrics.</param>
		/// <returns>The current <see cref="DirectRecordHolder"/> for chaining
		/// purposes.</returns>
		public DirectRecordHolder AddRecord(Record record) {
			records.Add(record);
			return this;
		}

		public IEnumerator<Record> GetEnumerator() => Records.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Records.GetEnumerator();
		IRecordHolder IRecordHolder.Reset() {
			Interlocked.Exchange(ref records, new ConcurrentBag<Record>());
			return this;
		}

		#endregion

	}
}
