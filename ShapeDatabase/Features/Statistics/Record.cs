using System;
using System.Collections.Generic;
using System.IO;
using ShapeDatabase.Util;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// A class containing all the different statistics/measures for a single shape.
	/// </summary>
	public class Record : IEquatable<Record> {

		#region --- Properties ---

		public DateTime Time { get; }
		public string Name { get; }

		public ICollection<(string, object)> Measures { get; } =
			new List<(string, object)>();

		#endregion

		#region --- Constructor Methods ---

		public Record(DateTime time, string name) {
			Time = time;
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		public Record AddMeasure(string name, object value) {
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			Measures.Add((name, value));
			return this;
		}

		public bool TryGetValue<T>(string name, out T value) {
			value = default;
			if (TryGetValue(name, out object result)) {
				if (result is T) {
					value = (T) result;
					return true;
				} else {
					throw new InvalidCastException();
				}
			}
			return false;
		}

		public bool TryGetValue(string name, out object value) {
			value = default;
			foreach ((string title, object measure) in Measures)
				if (name.Equals(title, StringComparison.OrdinalIgnoreCase)) {
					value = measure;
					return true;
				}
			return false;
		}

		public object this[string name] {
			get {
				if (TryGetValue(name, out object result))
					return result;
				throw new KeyNotFoundException();
			}
		}

		#endregion

		#region -- Object Methods --

		public override bool Equals(object obj) {
			return obj is Record && Equals((Record) obj);
		}

		public bool Equals(Record other) {
			return other != null
				&& Time == other.Time
				&& Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode() {
			return NumberUtil.Hash(Time, Name);
		}

		#endregion

		#endregion

	}

}
