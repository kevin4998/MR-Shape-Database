using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;
using OpenTK;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Refine;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util;

namespace ShapeDatabase.Features {
	/// <summary>
	/// Class for defining all descriptor calculator methods
	/// </summary>
	public static class DescriptorCalculators {

		#region --- Properties ---

		/// <summary>
		/// The multiplier needed for calculating compactness.
		/// </summary>
		private const double COMPACTNESS_CONS = 1 / (36 * Math.PI);

		/// <summary>
		/// A collection of all the locally defined descriptors.
		/// </summary>
		public static IEnumerable<FeatureManager.DescriptorCalculator> Descriptors {
			get {
				yield return SurfaceArea;
				yield return BoundingBoxVolume;
				yield return Diameter;
				yield return Eccentricity;
				yield return Compactness;
				yield return DistanceBarycenter;
				yield return DistanceVertices;
				yield return SquareRootTriangles;
				yield return CubeRootTetrahedron;
				yield return AngleVertices;
			}
		}
		/// <summary>
		/// A collection of all the names of each descriptor.
		/// </summary>
		public static IEnumerable<string> DescriptorNames {
			get {
				yield return "SurfaceArea";
				yield return "BoundingBoxVolume";
				yield return "Diameter";
				yield return "Eccentricity";
				yield return "Compactness";
				yield return "DistanceBarycenter";
				yield return "DistanceVertices";
				yield return "SquareRootTriangles";
				yield return "CubeRootTetrahedron";
				yield return "AngleVertices";
			}
		}

		#endregion

		#region --- Elementary Descriptors ---

		/// <summary>
		/// Elementary descriptor for calculating the surface area of a mesh
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The elementary descriptor with the calculated value</returns>
		public static ElemDescriptor SurfaceArea(IMesh mesh) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			double surfaceArea = 0;

			for (uint i = 0; i < mesh.FaceCount; i++)
				surfaceArea += mesh.GetTriArea(i);

			return new ElemDescriptor("SurfaceArea", surfaceArea);
		}

		/// <summary>
		/// Elementary descriptor for calculating the volume of the axis-aligned bouding box
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The elementary descriptor with the calculated value</returns>
		public static ElemDescriptor BoundingBoxVolume(IMesh mesh) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			return new ElemDescriptor("BoundingBoxVolume", mesh.GetBoundingBox().Volume);
		}

		/// <summary>
		/// Elementary descriptor for calculating the largest distance between any two contour points
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The elementary descriptor with the calculated value</returns>
		public static ElemDescriptor Diameter(IMesh mesh) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			float biggestDiameter = 0;

			Parallel.For(0, mesh.VertexCount - 1, i => {
				Parallel.For(i + 1, mesh.VertexCount, j => {
					float distance = Vector3.Distance(mesh.GetVertex((uint) i), mesh.GetVertex((uint) j));
					float tempDiameter;

					do {
						tempDiameter = biggestDiameter;
						if (distance <= biggestDiameter)
							break;
					}
					while (Interlocked.CompareExchange(ref biggestDiameter, distance, tempDiameter) != tempDiameter);
				});
			});

			IBoundingBox bb = mesh.GetBoundingBox();
			biggestDiameter /= NumberUtil.Max(bb.MaxX, bb.MaxY, bb.MaxZ);
			return new ElemDescriptor("Diameter", biggestDiameter);
		}

		/// <summary>
		/// Elementary descriptor for calculating the ratio between largest and smallest eigenvalues
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The elementary descriptor with the calculated value</returns>
		public static ElemDescriptor Eccentricity(IMesh mesh) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			// Prepare matrix of all the vectors to present to PCA.
			double[][] matrix = new double[mesh.VertexCount][];
			for (uint i = 0; i < mesh.VertexCount; i++)
				matrix[i] = mesh.GetVertex(i).AsArrayD();
			// Call PCA using The Accord library.
			PrincipalComponentAnalysis pca =
				new PrincipalComponentAnalysis(PrincipalComponentMethod.Center,
											   false, 3);
			pca.Learn(matrix);
			// Find the collection of eigenVectors.
			double[] eigenvalues = pca.Eigenvalues;

			return new ElemDescriptor("Eccentricity", eigenvalues[0] / eigenvalues[2]);
		}

		/// <summary>
		/// Elementary descript for calculation how similar a shape is with respect
		/// to a sphere.
		/// 
		/// <seealso cref="https://github.com/gradientspace/geometry3Sharp/blob/master/mesh/MeshMeasurements.cs"/>
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The elementary descriptor with the calculated value</returns>
		public static ElemDescriptor Compactness(IMesh mesh) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			double area_sum = 0;
			double mass_integral = 0;

			for (uint i = 0; i < mesh.FaceCount; i++) {
				Vector3[] triangle = mesh.GetVerticesFromFace(i);

				Vector3 v0 = triangle[0];
				Vector3 v1 = triangle[1];
				Vector3 v2 = triangle[2];

				Vector3 V1mV0 = v1 - v0;
				Vector3 V2mV0 = v2 - v0;
				Vector3 N = Vector3.Cross(V1mV0, V2mV0);

				area_sum += 0.5 * N.Length;

				double tmp0 = v0.X + v1.X;
				double f1x = tmp0 + v2.X;
				mass_integral += N.X * f1x;
			}

			double volume = mass_integral * (1.0 / 6.0);

			// Formule c = (A^3)/(V^2)
			double compactness = Math.Pow(area_sum, 3) / Math.Pow(volume, 2);
			compactness *= COMPACTNESS_CONS;
			return new ElemDescriptor("Compactness", compactness);
		}

		#endregion

		#region --- Histogram Descriptors ---

		/// <summary>
		/// Histogram descriptor for calculating the distance to the barycenter of a random vertex
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The histogram descriptor with the calculated histogram</returns>
		public static HistDescriptor DistanceBarycenter(IMesh mesh) {
			double binSize = 0.15;
			int[] binValues = new int[Settings.BinsPerHistogram];

			Parallel.For(0, Settings.ValuesPerHistogram, i => {
				Random random = RandomUtil.ThreadSafeRandom;
				Vector3 randomVertice = GetRandomVertice(mesh, random);
				float distance = randomVertice.Length;
				int bin = Math.Min((int)(distance / binSize), binValues.Length - 1);
				Interlocked.Increment(ref binValues[bin]);
			});

			return HistDescriptor.FromIntHistogram("DistanceBarycenter", binSize, binValues);
		}

		/// <summary>
		/// Histogram descriptor for calculating the distance between two random vertices
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The histogram descriptor with the calculated histogram</returns>
		public static HistDescriptor DistanceVertices(IMesh mesh) {
			double binSize = 0.25;
			int[] binValues = new int[Settings.BinsPerHistogram];

			Parallel.For(0, Settings.ValuesPerHistogram, i => {
				Random random = RandomUtil.ThreadSafeRandom;
				Vector3[] randomVertices = GetRandomVertices(mesh, random, 2);
				float distance = Vector3.Distance(randomVertices[0], randomVertices[1]);
				int bin = Math.Min((int)(distance / binSize), binValues.Length - 1);
				Interlocked.Increment(ref binValues[bin]);
			});

			return HistDescriptor.FromIntHistogram("DistanceVertices", binSize, binValues);
		}

		/// <summary>
		/// Histogram descriptor for calculating the square root of an triangle given by three random vertices
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The histogram descriptor with the calculated histogram</returns>
		public static HistDescriptor SquareRootTriangles(IMesh mesh) {
			double binSize = 0.125;
			int[] binValues = new int[Settings.BinsPerHistogram];

			Parallel.For(0, Settings.ValuesPerHistogram, i => {
				Random random = RandomUtil.ThreadSafeRandom;
				Vector3[] randomVertices = GetRandomVertices(mesh, random, 3);
				double area = Math.Sqrt(Functions.GetTriArea(randomVertices));
				int bin = Math.Min((int)(area / binSize), binValues.Length - 1);
				Interlocked.Increment(ref binValues[bin]);
			});

			return HistDescriptor.FromIntHistogram("SquareRootTriangles", binSize, binValues);
		}

		/// <summary>
		/// Histogram descriptor for calculating the cube root of a tetrahedron given by four random vertices
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The histogram descriptor with the calculated histogram</returns>
		public static HistDescriptor CubeRootTetrahedron(IMesh mesh) {
			double binSize = 0.075;
			int[] binValues = new int[Settings.BinsPerHistogram];

			Parallel.For(0, Settings.ValuesPerHistogram, i => {
				Random random = RandomUtil.ThreadSafeRandom;
				Vector3[] randomVertices = GetRandomVertices(mesh, random, 4);
				double volume = Math.Pow(Functions.GetTetVolume(randomVertices), (1d / 3d));
				int bin = Math.Min((int)(volume / binSize), binValues.Length - 1);
				Interlocked.Increment(ref binValues[bin]);
			});

			return HistDescriptor.FromIntHistogram("CubeRootTetrahedron", binSize, binValues);
		}

		/// <summary>
		/// Histogram descriptor for calculating the angle between three random vertices
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The histogram descriptor with the calculated histogram</returns>
		public static HistDescriptor AngleVertices(IMesh mesh) {
			double binSize = 18;
			int[] binValues = new int[Settings.BinsPerHistogram];

			Parallel.For(0, Settings.ValuesPerHistogram, i => {
				Random random = RandomUtil.ThreadSafeRandom;
				Vector3[] randomVertices = GetRandomVertices(mesh, random, 3);
				double angle = Functions.GetAngleVertices(randomVertices);
				int bin = Math.Min((int)(angle / binSize), binValues.Length - 1);
				Interlocked.Increment(ref binValues[bin]);
			});

			return HistDescriptor.FromIntHistogram("AngleVertices", binSize, binValues);
		}

		#endregion

		#region --- Helper Functions ---

		/// <summary>
		/// Method for getting a random vertex out of a mesh.
		/// </summary>
		/// <param name="mesh">The mesh of which the random vertices will be taken</param>
		/// <param name="random">The (threadsafe) random generator</param>
		/// <returns>A Single Vector3 which is a random vertex position.</returns>
		private static Vector3 GetRandomVertice(IMesh mesh, Random random)
			=> GetRandomVertices(mesh, random, 1)[0];

		/// <summary>
		/// Method for getting a certain number of random vertices from a mesh
		/// </summary>
		/// <param name="mesh">The mesh of which the random vertices will be taken</param>
		/// <param name="random">The (threadsafe) random generator</param>
		/// <param name="numberOfVertices">The number of random vertices</param>
		/// <returns>An array containing the random vertices</returns>
		private static Vector3[] GetRandomVertices(IMesh mesh, Random random, int numberOfVertices) {
			return mesh.GetRandomVertices(numberOfVertices, random);
		}

		#endregion

	}

}
