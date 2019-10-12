using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using g3;
using gs;
using OpenTK;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Refine {

	/// <summary>
	/// Class for extending meshes with too few faces or vertices.
	/// </summary>
	public class ExtendRefiner : IRefiner<Shapes.IMesh> {

		#region --- Properties ---

		private const int DESIRED_VERTICES = 5000;

		private static readonly Lazy<ExtendRefiner> lazy =
			new Lazy<ExtendRefiner> (() => new ExtendRefiner());

		/// <summary>
		/// Gives a refiner to extend meshes.
		/// </summary>
		public static ExtendRefiner Instance { get { return lazy.Value; } }

		#endregion

		#region --- Constructor Methods ---
		
		/// <summary>
		/// Initialises a new refiner to increase the amount of faces in a shape.
		/// </summary>
		private ExtendRefiner() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Checkes whether mesh has too few faces or vertices.
		/// </summary>
		/// <param name="mesh">The mesh to check for refinement.</param>
		/// <returns><see langword="true"/> if a refinement operation is needed
		/// for an optimal shape.</returns>
		public bool RequireRefinement(Shapes.IMesh mesh)
		{
			if (mesh == null) throw new ArgumentNullException(nameof(mesh));
			return mesh.VertexCount != DESIRED_VERTICES;
		}
		/// <summary>
		/// Extends mesh by applying one iteration of the Doo-Sabin algorithm.
		/// Overwrites the specified file.
		/// </summary>
		/// <param name="mesh">The mesh that needs to be refined.</param>
		/// <param name="file">The file containing the shape to refine.</param>
		/// <exception cref="ArgumentNullException">If the given file is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given file does not exist.
		/// </exception>
		public void RefineMesh(Shapes.IMesh mesh, FileInfo file) {
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (!file.Exists)
				throw new ArgumentException("File {0} does not exist.", file.FullName);

			DMesh3 meshDMesh3 = (DMesh3)((GeometryMesh)mesh).Base;

			Remesher remesher = new Remesher(meshDMesh3) {
				PreventNormalFlips = true
			};

			int i = 0;
			while (meshDMesh3.VertexCount < 5000 && i < 20)
			{
				remesher.SetTargetEdgeLength(0.0001F);
				remesher.BasicRemeshPass();
				i++;
			}

			Reducer reducer = new Reducer(meshDMesh3);
			reducer.ReduceToVertexCount(5000);

			IOWriteResult result = StandardMeshWriter.WriteFile(file.FullName,
			new List<WriteMesh>() { new WriteMesh(meshDMesh3) }, WriteOptions.Defaults);
		}

		#endregion

	}

	/// <summary>
	/// Class for simplifying meshes with too few faces or vertices.
	/// </summary>
	public class SimplifyRefiner : IRefiner<Shapes.IMesh> {

		#region --- Properties ---

		private const int DESIRED_VERTICES = 5000;

		private static readonly Lazy<SimplifyRefiner> lazy =
			new Lazy<SimplifyRefiner>(() => new SimplifyRefiner());

		/// <summary>
		/// Gives a refiner to simplify meshes.
		/// </summary>
		public static SimplifyRefiner Instance { get { return lazy.Value; } }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new refiner to reduce the amount of faces in a shape.
		/// </summary>
		private SimplifyRefiner() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Checks whether mesh has too many faces or vertices.
		/// </summary>
		/// <param name="mesh">The mesh to check for refinement.</param>
		/// <returns><see langword="true"/> if the given mesh has too many faces.
		/// </returns>
		public bool RequireRefinement(Shapes.IMesh mesh) {
			return mesh.VertexCount != DESIRED_VERTICES;
		}

		/// <summary>
		/// Simplifies mesh by reducing number of faces and triangles. Overwrites the .off file.
		/// </summary>
		/// /// <param name="mesh">The mesh that needs to be refined.</param>
		/// <param name="file">The file containing the shape to refine.</param>
		/// <exception cref="ArgumentNullException">If the given file is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given file does not exist.
		/// </exception>
		/// 
		public void RefineMesh(Shapes.IMesh mesh, FileInfo file)
		{
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (!file.Exists)
				throw new ArgumentException("File {0} does not exist.", file.FullName);

			DMesh3 meshDMesh3 = mesh as GeometryMesh;

			Reducer reducer = new Reducer(meshDMesh3);
			reducer.ReduceToVertexCount(5000);
			new MeshAutoRepair(meshDMesh3);

			IOWriteResult result = StandardMeshWriter.WriteFile(file.FullName,
			new List<WriteMesh>() { new WriteMesh(meshDMesh3) }, WriteOptions.Defaults);
		}

		#endregion

	}
}
