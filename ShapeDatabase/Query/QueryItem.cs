using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			return $"{MeshName} ({MeshDistance})";
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
