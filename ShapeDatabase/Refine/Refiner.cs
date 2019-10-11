using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;
using g3;
using gs;
using OpenTK;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util;

namespace ShapeDatabase.Refine {

	/// <summary>
	/// Class for extending meshes with too few faces or vertices.
	/// </summary>
	public class ExtendRefiner : IRefiner<Shapes.IMesh> {

		#region --- Properties ---

		private static readonly int DESIRED_VERTICES = 5000;

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

		private static readonly int DESIRED_VERTICES = 5000;

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

	/// <summary>
	/// A class to normalise all shapes in the same style using
	/// the 4 step normalisation process.
	/// <para>
	/// The 4 steps of the normalisation process consists of:
	/// <list type="number">
	///		<item>
	///			<description>Centering to the shape to the bary center.</description>
	///		</item>
	///		<item>
	///			<description>Aligning the shape using the eigenvectors.</description>
	///		</item>
	///		<item>
	///			<description>Scaling the figure to fit the [-1,1] range.</description>
	///		</item>
	///		<item>
	///			<description>And finally flipping along all 3 axis using the
	///			momentum test.</description>
	///		</item>
	/// </list>
	/// </para>
	/// </summary>
	public class NormalisationRefiner : IRefiner<Shapes.IMesh> {

		#region --- Properties ---

		private const float MIN_VALUE = -1f;
		private const float MAX_VALUE = 1f;

		private static readonly Lazy<NormalisationRefiner> lazy
			= new Lazy<NormalisationRefiner>();

		/// <summary>
		/// Gives a refiner to normalise meshes.
		/// </summary>
		public static NormalisationRefiner Instance => lazy.Value;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new refiner which makes use of the 4 step normalisation
		/// process to make all meshes similar.
		/// </summary>
		public NormalisationRefiner() { }

		#endregion

		#region --- Instance Methods ---

		public bool RequireRefinement(Shapes.IMesh mesh) {
			return mesh.IsNormalised;
		}

		public void RefineMesh(Shapes.IMesh mesh, FileInfo file) {
			Shapes.IMesh transformed = FlipShape(ScaleShape(AlignShape(CenterShape(mesh))));
			IO.OFFWriter.Instance.WriteFile(transformed, file.FullName);

		}


		/// <summary>
		/// Attempts to find the centroid or barycenter point of a mesh
		/// by using the average of all points in a mesh.
		/// </summary>
		/// <param name="mesh">The mesh to find the barycenter point.</param>
		/// <returns>A <see cref="Vector3"/> containing the center.</returns>
		private static Vector3 FindBaryCenter(Shapes.IMesh mesh) {
			double[] center = new double[3];

			if (mesh != null) { 
				foreach (Vector3 vector in mesh.Vertices)
					for(int i = 2; i >= 0; i--)
						center[i] += vector[i];

				double inverse = 1 / mesh.VertexCount;
				for(int i = 2; i >= 0; i--)
					center[i] *= inverse;
			}

			return new Vector3(
				Convert.ToSingle(center[0]),
				Convert.ToSingle(center[1]),
				Convert.ToSingle(center[2]));
		}


		private static Shapes.IMesh CenterShape(Shapes.IMesh mesh) {
			uint vertices = mesh.VertexCount;
			Vector3 center = FindBaryCenter(mesh);

			Vector3[] points = new Vector3[vertices];
			for (uint i = vertices - 1; i >= 0; i--)
				points[i] = mesh.GetVertex(i) - center;

			Shapes.SimpleMesh modifiedMesh = Shapes.SimpleMesh.CreateFrom(mesh);
			modifiedMesh.Vertices = points;
			return modifiedMesh;
		}

		private static Shapes.IMesh AlignShape(Shapes.IMesh mesh) {
			double[][] matrix = new double[mesh.VertexCount][];
			for (uint i = mesh.VertexCount - 1; i >= 0; i--) {
				Vector3 point = mesh.GetVertex(i);
				matrix[i] = new double[] { point.X, point.Y, point.Z };
			}

			PrincipalComponentAnalysis pca =
				new PrincipalComponentAnalysis(PrincipalComponentMethod.Standardize,
											   true, 3);
			pca.Learn(matrix);
			double[][] modified = pca.Transform(matrix);
			Vector3[] vectors = NumberUtil.Vectorize(modified);

			Shapes.SimpleMesh simple = Shapes.SimpleMesh.CreateFrom(mesh);
			simple.Vertices = vectors;
			return simple;
		}

		private static Shapes.IMesh ScaleShape(Shapes.IMesh mesh) {
			IBoundingBox bb = mesh.GetBoundingBox();
			float min = NumberUtil.Min(bb.MinX, bb.MinY, bb.MinZ);
			float max = NumberUtil.Max(bb.MaxX, bb.MaxY, bb.MaxZ);
			float scale = (MAX_VALUE - MIN_VALUE) / (max - min);

			uint vertices = mesh.VertexCount;
			Vector3[] points = new Vector3[vertices];
			for (uint i = vertices - 1; i >= 0; i--)
				points[i] = mesh.GetVertex(i) * scale;

			Shapes.SimpleMesh modifiedMesh = Shapes.SimpleMesh.CreateFrom(mesh);
			modifiedMesh.Vertices = points;
			return modifiedMesh;
		}

		private static Shapes.IMesh FlipShape(Shapes.IMesh mesh) {

			Shapes.SimpleMesh modifiedMesh = Shapes.SimpleMesh.CreateFrom(mesh);
			modifiedMesh.IsNormalised = true;
			return modifiedMesh;
		}


		#endregion

	}

}
