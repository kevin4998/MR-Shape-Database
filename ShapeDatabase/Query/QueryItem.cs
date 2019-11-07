using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ShapeDatabase.IO;
using ShapeDatabase.Properties;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Query {

	/// <summary>
	/// The results of a single comparison between a reference item and
	/// a comparison item from the library.
	/// </summary>
	[DebuggerDisplay("{MeshName}: {MeshDistance}")]
	public struct QueryItem : IEquatable<QueryItem>, IComparable<QueryItem> {

		#region --- Properties ---

		private static readonly StringComparer comparer =
			StringComparer.InvariantCultureIgnoreCase;

		public bool IsNull => string.IsNullOrEmpty(MeshName);
		public string MeshName { get; }
		public double MeshDistance { get; }

		#endregion

		#region --- Constructor Methods ---

		public QueryItem(string meshName, double meshDistance) {
			if (meshDistance < 0)
				throw new ArgumentException(Resources.EX_ExpPosValue, nameof(meshDistance));
			if (string.IsNullOrEmpty(meshName))
				throw new ArgumentNullException(nameof(meshName));

			this.MeshName = meshName;
			this.MeshDistance = meshDistance;
		}

		#endregion

		#region --- Instance Methods ---

		public int CompareTo(QueryItem other) {
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			return MeshDistance.CompareTo(other.MeshDistance);
		}

		public override bool Equals(object obj) {
			return obj != null
				&& obj is QueryItem
				&& Equals((QueryItem) obj);
		}

		public bool Equals(QueryItem other) {
			return other != null && comparer.Equals(MeshName, other.MeshName);
		}

		public override int GetHashCode() {
			return MeshName.GetHashCode();
		}

		public override string ToString() {
			return string.Format(
				Settings.Culture,
				"{0} ({1})",
				MeshName,
				MeshDistance.ToString(Settings.Culture)
			);
		}

		#endregion

		#region --- Static Methods ---

		/// <summary>
		/// Converts the string representation into QueryItem with the distance
		/// from its original reference item. And returns a boolean to identify
		/// if this conversion process has succeeded.
		/// </summary>
		/// <param name="serialised">The string representation of this item.</param>
		/// <param name="item">The reconstructed query item if it was successful,
		/// otherwise a default NULL query item.</param>
		/// <returns>If the conversion process was successful.</returns>
		public static bool TryParse(string serialised, out QueryItem item)
			=> TryParse(serialised, NumberStyles.Any, Settings.Culture, out item);

		/// <summary>
		/// Converts the string representation into QueryItem with the distance
		/// from its original reference item. And returns a boolean to identify
		/// if this conversion process has succeeded.
		/// </summary>
		/// <param name="serialised">The string representation of this item.</param>
		/// <param name="style">The style for which numbers are allowed.</param>
		/// <param name="format">The format to recognise numbers.</param>
		/// <param name="item">The reconstructed query item if it was successful,
		/// otherwise a default NULL query item.</param>
		/// <returns>If the conversion process was successful.</returns>
		public static bool TryParse(string serialised, NumberStyles style,
									IFormatProvider format, out QueryItem item) {
			try {
				item = FromString(serialised, style, format);
				return true;
			} catch (ArgumentException ex) {
				item = default;
				return false;
			}
		}


		/// <summary>
		/// Converts the string representation into QueryItem with the distance
		/// from its original reference item.
		/// </summary>
		/// <param name="serialised">The string representation of this item.</param>
		/// <returns>The reconstructed query item if it was successful.</returns>
		public static QueryItem FromString(string serialised)
			=> FromString(serialised, NumberStyles.Any, Settings.Culture);

		/// <summary>
		/// Converts the string representation into QueryItem with the distance
		/// from its original reference item.
		/// </summary>
		/// <param name="serialised">The string representation of this item.</param>
		/// <param name="style">The style for which numbers are allowed.</param>
		/// <param name="format">The format to recognise numbers.</param>
		/// <returns>The reconstructed query item if it was successful.</returns>
		public static QueryItem FromString(string serialised, NumberStyles style,
											IFormatProvider format) {
			if (string.IsNullOrEmpty(serialised))
				throw new ArgumentNullException(nameof(serialised));
			const string formatRegex = "^\\w+ \\((0|([1-9][0-9]*))((\\.|\\,)[0-9]+)?\\)$";
			//if (!Regex.IsMatch(serialised, formatRegex))
				//throw new InvalidFormatException(serialised, formatRegex);

			string[] args = serialised.Split(' ');
			if (args.Length != 2)
				throw new ArgumentException(
					string.Format(
						Settings.Culture,
						Resources.EX_Array_Size,
						args.Length,
						2
					)
				);

			string name = args[0];
			string number = args[1].Substring(1, args[1].Length - 2);

			if (!double.TryParse(number, style, format, out double distance))
				throw new InvalidFormatException(number, "decimal number");

			return new QueryItem(name, distance);
		}

		#endregion

		#region --- Operators ---

		public static bool operator ==(QueryItem left, QueryItem right) {
			return Equals(left, right);
		}

		public static bool operator !=(QueryItem left, QueryItem right) {
			return !(left == right);
		}

		public static bool operator <(QueryItem left, QueryItem right) {
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(QueryItem left, QueryItem right) {
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >(QueryItem left, QueryItem right) {
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(QueryItem left, QueryItem right) {
			return left.CompareTo(right) >= 0;
		}

		#endregion

	}

	public class QueryItemComparer : IComparer<QueryItem> {
		public int Compare(QueryItem x, QueryItem y) {
			if (x == y)
				return 0;
			if (x.IsNull)
				return int.MaxValue;
			if (y.IsNull)
				return int.MinValue;
			else
				return x.CompareTo(y);
		}

	}

}
