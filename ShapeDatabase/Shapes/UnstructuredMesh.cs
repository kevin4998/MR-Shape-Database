using OpenTK;
using System;
using System.Diagnostics;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// An implementation of a shape which uses a collection of triangles
	/// in space without any form of positional restrictions.
	/// </summary>
	[DebuggerDisplay("Grid Size: {UnstructuredGrid.Length}; Element Size: {Elements.Length}")]
	public struct UnstructuredMesh {

		/// <summary>
		/// An array defining all the points of the cells for this grid.
		/// </summary>
		public Vector3[] UnstructuredGrid { get; }
		/// <summary>
		/// An array specifying which point in the grid should be use to
		/// define a shape. These shapes commonly consist of triangles.
		/// </summary>
		public uint[] Elements { get; }

		public uint VerticesCount => Convert.ToUInt32(UnstructuredGrid.Length);

		public uint FacesCount => Convert.ToUInt32(Elements.Length / 3);

		/// <summary>
		/// Initialises a new mesh with the specified containing grid and triangles on it.
		/// </summary>
		/// <param name="grid">A collection of points which can be used
		/// for shapes.</param>
		/// <param name="indices">A collection of references to points
		/// on the grid to form triangles.</param>
		public UnstructuredMesh(Vector3[] grid, uint[] indices) {
			Debug.Assert(GridCondition(grid));
			Debug.Assert(IndiceCondition(grid, indices));

			UnstructuredGrid = grid;
			Elements = indices;
		}

#if DEBUG

		private static bool GridCondition(Vector3[] grid) {
			// A grid contains the X, Y and Z coords to must be a multiple of 3.
			if (grid.Length == 0)
				return false;
			// All points need to be in a -1,1 cube.
			foreach (Vector3 variable in grid)
				if (variable.X < -1 || variable.X > 1
					|| variable.Y < -1 || variable.Y > 1
					|| variable.Z < -1 || variable.Z > 1)
					return false;
			return true;
		}

		private static bool IndiceCondition(Vector3[] grid, uint[] indices) {
			// Extention of 3 as it contains triangles.
			if (indices.Length % 3 != 0 || indices.Length == 0)
				return false;
			// Can only use points specified in the grid.
			foreach (uint variable in indices)
				if (variable >= grid.Length)
					return false;
			return true;
		}

#endif

	}
}
