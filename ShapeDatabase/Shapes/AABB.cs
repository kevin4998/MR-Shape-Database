using System;
using System.Diagnostics;
using OpenTK;

namespace ShapeDatabase.Shapes {
	/// <summary>
	/// The axis aligned bounding box of the shape.
	/// This is a box which perfectly surrounds the whole shape.
	/// </summary>
	public struct AABB {

		#region --- Properties ---

		public float MinX { get; }
		public float MaxX { get; }
		public float MinY { get; }
		public float MaxY { get; }
		public float MinZ { get; }
		public float MaxZ { get; }

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

		#region -- Static Methods --

		/// <summary>
		/// Creates an axis aligned bounding box which perfectly surrounds this shape.
		/// </summary>
		/// <param name="mesh">The mesh whose bounding box needs to be found.</param>
		/// <returns>An <see cref="AABB"/> around this shape.</returns>
		public static AABB FromMesh(UnstructuredMesh mesh) {
			float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue,
				  maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;
			Vector3[] points = mesh.UnstructuredGrid;

			foreach (Vector3 point in points) {
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
