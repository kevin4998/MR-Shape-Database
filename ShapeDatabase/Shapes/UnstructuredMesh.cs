﻿using OpenTK;
using System;
using System.Diagnostics;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// An implementation of a shape which uses a collection of triangles
	/// in space without any form of positional restrictions.
	/// </summary>
	[DebuggerDisplay("Grid Size: {UnstructuredGrid.Length}; Element Size: {Elements.Length}")]
	public struct UnstructuredMesh {

		#region --- Properties ---

		/// <summary>
		/// A Default object for an unspecified mesh.
		/// </summary>
		public static readonly UnstructuredMesh NULL = new UnstructuredMesh(Array.Empty<Vector3>(), Array.Empty<uint>());

		/// <summary>
		/// An array defining all the points of the cells for this grid.
		/// </summary>
		public Vector3[] UnstructuredGrid { get; }
		/// <summary>
		/// An array specifying which point in the grid should be use to
		/// define a shape. These shapes commonly consist of triangles.
		/// </summary>
		public uint[] Elements { get; }
		/// <summary>
		/// If the current shape is normalised and false in the range of [-1,1]
		/// located at the center of space.
		/// </summary>
		public bool IsNormalised { get; }
		/// <summary>
		/// The total amount of vertices in this shape.
		/// </summary>
		public uint VerticesCount => Convert.ToUInt32(UnstructuredGrid.Length);
		/// <summary>
		/// The total amount of faces in this shape.
		/// </summary>
		public uint FacesCount => Convert.ToUInt32(Elements.Length / 3);
		/// <summary>
		/// An axis aligned bounding box which surrounds this shape.
		/// </summary>
		public AABB AABB => AABB.FromMesh(this);
		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new mesh with the specified containing grid and triangles on it.
		/// </summary>
		/// <param name="grid">A collection of points which can be used
		/// for shapes.</param>
		/// <param name="indices">A collection of references to points
		/// on the grid to form triangles.</param>
		/// <param name="normalised">If the current shape is normalised and whose conditions
		/// should be checked.</param>
		public UnstructuredMesh(Vector3[] grid, uint[] indices, bool normalised = false) {
			#if DEBUG
			if (normalised) { 
				Debug.Assert(GridCondition(grid));
				Debug.Assert(IndiceCondition(grid, indices));
			}
			#endif

			UnstructuredGrid = grid;
			Elements = indices;
			IsNormalised = normalised;
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		/// <summary>
		/// Converts the current mesh to fall in the [-1,1] range and at the 
		/// center of the universe.
		/// </summary>
		/// <returns>A new mesh, different from the current one, which is normalised.
		/// </returns>
		public UnstructuredMesh Normalise() {
			AABB box = AABB;
			// The transformation to scale all equally.
			Vector2 cube = box.Cube;
			float min = cube.X;
			float max = cube.Y;
			float scale = 2 / (max - min);
			// The transformation to move to the origin center.
			Vector3 move = box.Center;
			for (int j = 2; j >= 0; j--)
				move[j] = (move[j] - min) * scale;
			// Perform transformations.
			Vector3[] current = UnstructuredGrid;
			Vector3[] next = new Vector3[current.Length];
			for (int i = current.Length - 1; i >= 0; i--)
				for (int j = 2; j >= 0; j--)
					next[i][j] = (current[i][j] - min) * scale - move[j];

			return new UnstructuredMesh(next, Elements, true);
		}

		#endregion

		#region -- Debug Conditions --

#if DEBUG

		private static bool GridCondition(Vector3[] grid) {
			// A grid contains the X, Y and Z coords to must be a multiple of 3.
			/*if (grid.Length == 0)
				return false;*/
			// All points need to be in a -1,1 cube.
			foreach (Vector3 variable in grid)
				if (variable.X <= -1.00001f || variable.X >= 1.00001f
					|| variable.Y <= -1.00001f || variable.Y >= 1.00001f
					|| variable.Z <= -1.00001f || variable.Z >= 1.00001f)
					return false;
			return true;
		}

		private static bool IndiceCondition(Vector3[] grid, uint[] indices) {
			// Extention of 3 as it contains triangles.
			if (indices.Length % 3 != 0)
				return false;
			// Can only use points specified in the grid.
			foreach (uint variable in indices)
				if (variable >= grid.Length)
					return false;
			return true;
		}

#endif

		#endregion

		#endregion

	}
}
