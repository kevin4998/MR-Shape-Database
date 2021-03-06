﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Util;

namespace ShapeDatabase.Features.Descriptors {

	/// <summary>
	/// A descriptor that describes a property of the shape with one single value.
	/// </summary>
	[DebuggerDisplay("{Name}: {Value}")]
	public class ElemDescriptor : BaseDescriptor<ElemDescriptor> {

		#region --- Properties ---

		/// <summary>
		/// Value of the elementary descriptor.
		/// </summary>
		public double Value { get; }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Constructor of the elementary descriptor.
		/// </summary>
		/// <param name="name">Name of the descriptor.</param>
		/// <param name="value">Value of the descritor.</param>
		/// <exception cref="ArgumentNullException">If the name is <see langword="null"/>.
		/// </exception>
		public ElemDescriptor(string name, double value)
			: base(name) {
			Value = value;
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		/// <summary>
		/// Returns the distance with another elemtary descriptor.
		/// </summary>
		/// <param name="desc">The other elementary descriptor.</param>
		/// <returns>The distance (0 = Equal Descriptors, 1 = Completely Different).</returns>
		public override double Compare(ElemDescriptor desc) {
			if (desc == null)
				throw new ArgumentNullException(nameof(desc));
			return Math.Abs(desc.Value - Value);
		}

		/// Serializes the elementary descriptor.
		/// </summary>
		/// <returns>The serialized elementary descriptor.</returns>
		public override string Serialize() {
			IFormatProvider format = Settings.Culture;
			return Value.ToString(format);
		}

		#endregion

		#region -- Static Methods --

		/// <summary>
		/// (Tries to) deserialize a serialization string into an elementary descriptor.
		/// </summary>
		/// <param name="name">The name of the elementary descriptor.</param>
		/// <param name="serialised">The serialization string.</param>
		/// <param name="desc">The elementary descriptor (null if not successful).</param>
		/// <returns>Bool indicating whether the serialization was successful.</returns>
		public static bool TryParse(string name, string serialised,
									out ElemDescriptor desc) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrEmpty(serialised))
				throw new ArgumentNullException(nameof(serialised));

			IFormatProvider format = Settings.Culture;
			NumberStyles style = NumberStyles.Any;
			if (double.TryParse(serialised, style, format, out double value)) {
				desc = new ElemDescriptor(name, value);
				return true;
			}

			desc = null;
			return false;
		}

		#endregion

		#endregion

	}
}
