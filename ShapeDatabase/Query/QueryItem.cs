﻿using System;
using System.Collections;
using System.Collections.Generic;
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
	public struct QueryItem : IEquatable<QueryItem>, IComparable<QueryItem> {

		#region --- Properties ---

		private static readonly StringComparer comparer =
			StringComparer.InvariantCultureIgnoreCase;


		public string MeshName { get; }
		public double MeshDistance { get; }

		#endregion

		#region --- Constructor Methods ---

		public QueryItem(string meshName, double meshDistance) {
			if (meshDistance < 0)
				throw new ArgumentException(Resources.EX_ExpPosValue, nameof(meshDistance));

			this.MeshName = meshName ?? throw new ArgumentNullException(nameof(meshName));
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

}