using System;
using System.Collections;
using System.Collections.Generic;
using ShapeDatabase.Properties;
using ShapeDatabase.Util;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// A class containing all the different statistics/measures for a single shape.
	/// </summary>
	public class Record : IEquatable<Record>, IEnumerable<object> {

		#region --- Properties ---

		/// <summary>
		/// The time when this record was created.
		/// </summary>
		public DateTime Time { get; }

		/// <summary>
		/// The unique name of the object to who this <see cref="Record"/> belongs.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// A collection of measures which has been extracted from the previous object.
		/// </summary>
		public ICollection<(string, object)> Measures { get; } =
			new List<(string, object)>();

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new <see cref="Record"/> which can contain values of
		/// a certain object.
		/// </summary>
		/// <param name="time">The moment in time when this record was created.</param>
		/// <param name="name">The unique name of the object to who this record belongs.</param>
		/// <exception cref="ArgumentNullException">If the provided name is <see langword="null"/>.</exception>
		public Record(DateTime time, string name) {
			Time = time;
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		/// <summary>
		/// Specifies a new measure which has been taken on this object.
		/// </summary>
		/// <param name="name">The name of the measurement which has been taken.</param>
		/// <param name="value">The result of the measurement.</param>
		/// <returns>The record.</returns>
		public Record AddMeasure(string name, object value) {
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			Measures.Add((name, value));
			return this;
		}

		/// <summary>
		/// Attempts to retrieve a previously taken measure of an object.
		/// </summary>
		/// <typeparam name="T">The type of result which was given from the measure.
		/// </typeparam>
		/// <param name="name">The name of the measurement to recover.</param>
		/// <param name="value">The returned value of the object if it is present
		/// in the <see cref="Record"/>.</param>
		/// <returns><see langword="true"/> if the value was successfully retrieved
		/// and could be converted.</returns>
		/// <exception cref="InvalidCastException">If a value was retrieved for this name
		/// but it could not be casted to the specified type.</exception>
		public bool TryGetValue<T>(string name, out T value) {
			value = default;
			if (TryGetValue(name, out object result)) {
				if (result is T) {
					value = (T) result;
					return true;
				} else {
					throw new InvalidCastException(
						string.Format(Settings.Culture,
							Resources.EX_Cast,
							typeof(T).Name,
							result == null ? "NULL" : result.GetType().Name)
					);
				}
			}
			return false;
		}

		/// <summary>
		/// Attempts to retrieve a previously taken measure of an object.
		/// </summary>
		/// <param name="name">The name of the measurement to recover.</param>
		/// <param name="value">The returned value of the object if it is present
		/// in the <see cref="Record"/>.</param>
		/// <returns><see langword="true"/> If the value was successfully retrieved.
		/// </returns>
		public bool TryGetValue(string name, out object value) {
			value = default;
			foreach ((string title, object measure) in Measures)
				if (name.Equals(title, StringComparison.OrdinalIgnoreCase)) {
					value = measure;
					return true;
				}
			return false;
		}

		/// <summary>
		/// Directly provides the measured value of a specific attribute.
		/// </summary>
		/// <param name="name">The name of the attribute to recover.</param>
		/// <returns>The value of the measurement for this attribute.</returns>
		/// <exception cref="KeyNotFoundException">If the specified attribute has not
		/// been measured in this <see cref="Record"/>.</exception>
		public object this[string name] {
			get {
				if (TryGetValue(name, out object result))
					return result;
				throw new KeyNotFoundException();
			}
		}

		#endregion

		#region -- Interface Methods --

		public IEnumerator<object> GetEnumerator() {
			return Enumerators.FromConvert(Measures.GetEnumerator(),
										   tuple => tuple.Item2);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		#endregion

		#region -- Object Methods --

		public override bool Equals(object obj) {
			return obj is Record && Equals((Record) obj);
		}

		/// <summary>
		/// Whether this record is equal to another record.
		/// </summary>
		/// <param name="other">The other record.</param>
		/// <returns>Bool stating whether they are equal.</returns>
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
