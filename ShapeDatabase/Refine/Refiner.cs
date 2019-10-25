using System;
using System.IO;
using System.Runtime.CompilerServices;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;
using g3;
using gs;
using OpenTK;
using ShapeDatabase.IO;
using ShapeDatabase.Properties;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util;

namespace ShapeDatabase.Refine {

	/// <summary>
	/// Class for extending meshes with too few faces or vertices.
	/// </summary>
	public class ExtendRefiner : IRefiner<Shapes.IMesh> {

		#region --- Properties ---

		/// <summary>
		/// The number of desried vertices.
		/// </summary>
		private const int DESIRED_VERTICES = 5000;
		/// <summary>
		/// The maximum number of refinement iterations.
		/// </summary>
		private const int MAX_ITERATIONS = 20;

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
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (!file.Exists)
				throw new ArgumentException(Resources.EX_FileNotFound, file.FullName);

			DMesh3 meshDMesh3 = mesh as GeometryMesh;

			Remesher remesher = new Remesher(meshDMesh3) {
				PreventNormalFlips = true
			};

			int i = 0;
			while (meshDMesh3.VertexCount < 5000 && i < MAX_ITERATIONS)
			{
				remesher.SetTargetEdgeLength(0.0001F);
				remesher.BasicRemeshPass();
				i++;
			}

			Reducer reducer = new Reducer(meshDMesh3);
			reducer.ReduceToVertexCount(5000);

			Settings.FileManager.WriteObject(meshDMesh3, file.FullName);
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
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
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
		public void RefineMesh(Shapes.IMesh mesh, FileInfo file) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (!file.Exists)
				throw new ArgumentException(Resources.EX_FileNotLoad, file.FullName);

			DMesh3 meshDMesh3 = mesh as GeometryMesh;

			Reducer reducer = new Reducer(meshDMesh3);
			reducer.ReduceToVertexCount(5000);

			Settings.FileManager.WriteObject(meshDMesh3, file.FullName);
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
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			return !mesh.IsNormalised;
		}

		public void RefineMesh(Shapes.IMesh mesh, FileInfo file) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			if (!file.Exists)
				throw new ArgumentException(Resources.EX_FileNotLoad, file.FullName);
			Shapes.SimpleMesh transformed =
				ScaleShape(
					FlipShape(
						AlignShape(
							CenterShape(mesh)
						)
					)
				);
			transformed.IsNormalised = true;
			Settings.FileManager.WriteObject(transformed, file.FullName);
		}


		/// <summary>
		/// Attempts to find the centroid or barycenter point of a mesh
		/// by using the average of the center of all triangles divided
		/// by their mass for an accurate representation.
		/// </summary>
		/// <param name="mesh">The mesh to find the barycenter point.</param>
		/// <returns>A <see cref="Vector3"/> containing the center.</returns>
		private static Vector3 FindBaryCenter(Shapes.IMesh mesh) {

			OpenTK.Vector3d totalSum = new OpenTK.Vector3d();
			double totalArea = 0;

			foreach(Vector3 face in mesh.Faces) {

				Vector3[] vertices = mesh.GetVerticesFromFace(face);

				double area = mesh.GetTriArea(vertices);
				if (area == 0)
					continue;
				totalSum += OpenTK.Vector3d.Multiply(
					(OpenTK.Vector3d) CenterOfTriangle(vertices), area);

				totalArea += area;
			}

			return (Vector3) OpenTK.Vector3d.Divide(totalSum, totalArea);

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Vector3 CenterOfTriangle(Vector3[] points) {
			if (points == null || points.Length == 0)
				throw new ArgumentNullException(nameof(points));

			Vector3 result = new Vector3();
			for(int i = 2; i >= 0; i--)
				result += points[i];
			return result / 3;
		}


		private static Shapes.SimpleMesh CenterShape(Shapes.IMesh mesh) {
			uint vertices = mesh.VertexCount;
			Vector3 center = FindBaryCenter(mesh);

			Vector3[] points = new Vector3[vertices];
			for (uint i = 0; i < vertices; i++)
				points[i] = mesh.GetVertex(i) - center;

			Shapes.SimpleMesh modifiedMesh = Shapes.SimpleMesh.CreateFrom(mesh);
			modifiedMesh.Vertices = points;
			return modifiedMesh;
		}

		private static Shapes.SimpleMesh AlignShape(Shapes.SimpleMesh mesh) {
			// Prepare matrix of all the vectors to present to PCA.
			double[][] matrix = new double[mesh.VertexCount][];
			for (uint i = 0; i < mesh.VertexCount; i++)
				matrix[i] = mesh.GetVertex(i).AsArrayD();
			// Call PCA using The Accord library.
			PrincipalComponentAnalysis pca =
				new PrincipalComponentAnalysis(PrincipalComponentMethod.Center,
											   false, 3);
			MultivariateLinearRegression regression = pca.Learn(matrix);
			double[][] transformed = regression.Transform(matrix);
			Vector3[] vectors = transformed.Vectorize();

			/*
			// Find the collection of eigenVectors.
			double[][] eigenvectors = pca.ComponentVectors;
			// Convert the coordinate system to the eigenvector one.
			Vector3[] vectors = new Vector3[mesh.VertexCount];
			for (int i = (int) mesh.VertexCount - 1; i >= 0; i--) {
				double[] currentVector = matrix[i];
				double[] newVector = Accord.Math.Matrix.Dot(eigenvectors,
															currentVector);
				vectors[i] = newVector.AsVector();
			}*/
			

			// Provide the new positions into the mesh.
			Shapes.SimpleMesh simple = Shapes.SimpleMesh.CreateFrom(mesh);
			simple.Vertices = vectors;
			return simple;
		}

		private static Shapes.SimpleMesh FlipShape(Shapes.IMesh mesh) {
			Shapes.SimpleMesh modifiedMesh = Shapes.SimpleMesh.CreateFrom(mesh);

			uint[] positiveValues = new uint[3];
			foreach (Vector3 vector in modifiedMesh.Vertices)
				for (int i = 2; i >= 0; i--)
					if (vector[i] >= 0)
						positiveValues[i]++;

			// Most mass should be on the left/ in the negative space.
			uint half = modifiedMesh.VertexCount >> 1;
			Vector3 flip = new Vector3();
			for (int i = 2; i >= 0; i--)
				flip[i] = (positiveValues[i] > half) ? -1 : 1;
			// Flip the direction of vertices if more mass is on the positive side.
			if (flip != Vector3.One)
				for (uint i = 0; i < modifiedMesh.VertexCount; i++)
					modifiedMesh.SetVertex(i, modifiedMesh.GetVertex(i) * flip);

			return modifiedMesh;
		}

		private static Shapes.SimpleMesh ScaleShape(Shapes.IMesh mesh) {
			IBoundingBox bb = mesh.GetBoundingBox();


			float min = NumberUtil.Min(bb.MinX, bb.MinY, bb.MinZ);
			float max = NumberUtil.Max(bb.MaxX, bb.MaxY, bb.MaxZ);
			float dif = (MAX_VALUE - MIN_VALUE) / (max - min);

			Vector3 minVector = new Vector3(min, min, min);
			Vector3 minExpVector = new Vector3(MIN_VALUE, MIN_VALUE, MIN_VALUE);

			Vector3[] points = new Vector3[mesh.VertexCount];
			for (int i = points.Length - 1; i >= 0; i--) {
				points[i] = (mesh.GetVertex((uint) i) - minVector) * dif + minExpVector;
			}


			Shapes.SimpleMesh modifiedMesh = Shapes.SimpleMesh.CreateFrom(mesh);
			modifiedMesh.Vertices = points;
			return modifiedMesh;
		}


		#endregion

	}

}
