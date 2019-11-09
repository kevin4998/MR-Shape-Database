using System;
using System.Diagnostics;
using OpenTK;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// The axis aligned bounding box of the shape.
	/// This is a box which perfectly surrounds the whole shape.
	/// </summary>
	[DebuggerDisplay("({MinX}, {MinY}, {MinZ});({MaxX}, {MaxY}, {MaxZ})")]
	public struct AABB : IBoundingBox, IEquatable<AABB> {

		#region --- Properties ---

		public float MinX { get; }
		public float MaxX { get; }
		public float MinY { get; }
		public float MaxY { get; }
		public float MinZ { get; }
		public float MaxZ { get; }

		public float Width => MaxX - MinX;
		public float Height => MaxY - MinY;
		public float Depth => MaxZ - MinZ;
		public float Volume => Width * Height * Depth;

		public Vector3 Min {
			get {
				return new Vector3(MinX, MinY, MinZ);
			}
		}
		public Vector3 Max {
			get {
				return new Vector3(MaxX, MaxY, MaxZ);
			}
		}
		public Vector3 Size {
			get {
				return new Vector3(MaxX - MinX,
								   MaxY - MinY,
								   MaxZ - MinZ);
			}
		}
		public Vector3 Center {
			get {
				return new Vector3((MaxX + MinX) / 2,
								   (MaxY + MinY) / 2,
								   (MaxZ + MinZ) / 2);
			}
		}
		public Vector2 Cube {
			get {
				return new Vector2(
					Math.Min(Math.Min(MinX, MinY), MinZ),
					Math.Max(Math.Max(MaxX, MaxY), MaxZ)
				);
			}
		}

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Creates a new box with the given points as its boundary.
		/// </summary>
		/// <param name="minx">The smallest x position.</param>
		/// <param name="maxx">The largest x position</param>
		/// <param name="miny">The smallest y position.</param>
		/// <param name="maxy">The largest Y position</param>
		/// <param name="minz">The smallest z position.</param>
		/// <param name="maxz">The largest z position</param>
		public AABB(float minx, float maxx,
					float miny, float maxy,
					float minz, float maxz) {
			Debug.Assert(minx <= maxx);
			Debug.Assert(miny <= maxy);
			Debug.Assert(minz <= maxz);

			MinX = minx;
			MaxX = maxx;
			MinY = miny;
			MaxY = maxy;
			MinZ = minz;
			MaxZ = maxz;
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods ---

		public override bool Equals(object obj) {
			return obj is AABB aABB && Equals(aABB);
		}

		public bool Equals(AABB other) {
			return this.MinX == other.MinX &&
				   this.MaxX == other.MaxX &&
				   this.MinY == other.MinY &&
				   this.MaxY == other.MaxY &&
				   this.MinZ == other.MinZ &&
				   this.MaxZ == other.MaxZ;
		}

		public override int GetHashCode() {
			// Automaticall generated code
			int hashCode = 1957011258;
			hashCode = hashCode * -1521134295 + this.MinX.GetHashCode();
			hashCode = hashCode * -1521134295 + this.MaxX.GetHashCode();
			hashCode = hashCode * -1521134295 + this.MinY.GetHashCode();
			hashCode = hashCode * -1521134295 + this.MaxY.GetHashCode();
			hashCode = hashCode * -1521134295 + this.MinZ.GetHashCode();
			hashCode = hashCode * -1521134295 + this.MaxZ.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(AABB left, AABB right) {
			return left.Equals(right);
		}

		public static bool operator !=(AABB left, AABB right) {
			return !(left == right);
		}


		#endregion

		#region -- Static Methods --

		/// <summary>
		/// Creates an axis aligned bounding box which perfectly surrounds this shape.
		/// </summary>
		/// <param name="mesh">The mesh whose bounding box needs to be found.</param>
		/// <returns>An <see cref="AABB"/> around this shape.</returns>
		public static AABB FromMesh(IMesh mesh) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue,
				  maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;

			foreach (Vector3 point in mesh.Vertices) {
				minx = minx < point.X ? minx : point.X;
				maxx = maxx > point.X ? maxx : point.X;
				miny = miny < point.Y ? miny : point.Y;
				maxy = maxy > point.Y ? maxy : point.Y;
				minz = minz < point.Z ? minz : point.Z;
				maxz = maxz > point.Z ? maxz : point.Z;
			}

			return new AABB(minx, maxx, miny, maxy, minz, maxz);
		}

		#endregion

		#endregion

	}
}
