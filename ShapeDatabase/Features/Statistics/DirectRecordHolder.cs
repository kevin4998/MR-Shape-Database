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
	public class DirectRecordHolder : IRecordHolder {

		#region --- Properties ---

		private ConcurrentBag<string> measureNames = new ConcurrentBag<string>();
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

		public DirectRecordHolder AddMeasureName(string measureName) {
			this.measureNames.Add(measureName);
			return this;
		}
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
