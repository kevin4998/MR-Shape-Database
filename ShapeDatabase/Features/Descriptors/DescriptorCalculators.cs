using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Runtime.CompilerServices;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;
using System.Threading;
using ShapeDatabase.Refine;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for defining all descriptor calculator methods
	/// </summary>
	public static class DescriptorCalculators
	{
		/// <summary>
		/// Elementary descriptor for calculating the surface area of a mesh
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The elementary descriptor with the calculated value</returns>
		public static ElemDescriptor SurfaceArea(IMesh mesh)
		{
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
		public static ElemDescriptor BoundingBoxVolume(IMesh mesh)
		{
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			return new ElemDescriptor("BoundingBoxVolume", mesh.GetBoundingBox().Volume);
		}

		/// <summary>
		/// Elementary descriptor for calculating the ratio between largest and smallest eigenvalues
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The elementary descriptor with the calculated value</returns>
		public static ElemDescriptor Eccentricity(IMesh mesh)
		{
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
			MultivariateLinearRegression regression = pca.Learn(matrix);
			// Find the collection of eigenVectors.
			double[] eigenvalues = pca.Eigenvalues;

			return new ElemDescriptor("Eccentricity", eigenvalues[0] / eigenvalues[2]);
		}

		/// <summary>
		/// Elementary descriptor for calculating the largest distance between any two contour points
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The elementary descriptor with the calculated value</returns>
		public static ElemDescriptor Diameter(IMesh mesh)
		{
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			float biggestDiameter = 0;

			Parallel.For(0, mesh.VertexCount - 1, i =>
			{
				Parallel.For(i + 1, mesh.VertexCount, j =>
				{
				float distance = Vector3.Distance(mesh.GetVertex((uint)i), mesh.GetVertex((uint)j));
				float tempDiameter;

				do
				{
					tempDiameter = biggestDiameter;
					if (distance <= biggestDiameter)
						break;
				}
				while (Interlocked.CompareExchange(ref biggestDiameter, distance, tempDiameter) != tempDiameter);
				});
			});

			return new ElemDescriptor("Diameter", biggestDiameter);
		}

		/// <summary>
		/// Histogram descriptor for calculating the distance to the barycenter of a random vertex
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The histogram descriptor with the calculated histogram</returns>
		public static HistDescriptor DistanceBarycenter(IMesh mesh)
		{
			double binSize = 0.15;
			int[] binValues = new int[10];
			int numberOfValues = 5000;	
			Vector3 baryCenter = NormalisationRefiner.FindBaryCenter(mesh);
			ThreadSafeRandom random = new ThreadSafeRandom();

			Parallel.For(0, numberOfValues, i =>
			{
				Vector3 randomVector = mesh.GetVertex((uint)random.Next(0, (int)mesh.VertexCount));
				float distance = Vector3.Distance(randomVector, baryCenter);
				int bin = Math.Min((int)(distance / binSize), 9);
				AddOneToBin(ref binValues, bin);
			});
					   
			return new HistDescriptor("DistanceBarycenter", binSize, binValues);
		}

		/// <summary>
		/// Histogram descriptor for calculating the distance between two random vertices
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The histogram descriptor with the calculated histogram</returns>
		public static HistDescriptor DistanceVertices(IMesh mesh)
		{
			double binSize = 0.25;
			int[] binValues = new int[10];
			int numberOfValues = 5000;
			ThreadSafeRandom random = new ThreadSafeRandom();

			Parallel.For(0, numberOfValues, i =>
			{
				Vector3 randomVector1 = mesh.GetVertex((uint)random.Next(0, (int)mesh.VertexCount));
				Vector3 randomVector2 = mesh.GetVertex((uint)random.Next(0, (int)mesh.VertexCount));

				while (randomVector1 == randomVector2)
				{
					randomVector2 = mesh.GetVertex((uint)random.Next(0, (int)mesh.VertexCount));
				}

				float distance = Vector3.Distance(randomVector1, randomVector2);
				int bin = Math.Min((int)(distance / binSize), 9);
				AddOneToBin(ref binValues, bin);
			});

			return new HistDescriptor("DistanceVertices", binSize, binValues);
		}

		/// <summary>
		/// Histogram descriptor for calculating the square root of an triangle given by three random vertices
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The histogram descriptor with the calculated histogram</returns>
		public static HistDescriptor SquareRootTriangles(IMesh mesh)
		{
			double binSize = 0.125;
			int[] binValues = new int[10];
			int numberOfValues = 5000;
			ThreadSafeRandom random = new ThreadSafeRandom();

			Parallel.For(0, numberOfValues, i =>
			{
				Vector3 randomVector1 = mesh.GetVertex((uint)random.Next(0, (int)mesh.VertexCount));
				Vector3 randomVector2 = mesh.GetVertex((uint)random.Next(0, (int)mesh.VertexCount));
				Vector3 randomVector3 = mesh.GetVertex((uint)random.Next(0, (int)mesh.VertexCount));

				while (randomVector1 == randomVector2)
				{
					randomVector2 = mesh.GetVertex((uint)random.Next(0, (int)mesh.VertexCount));
				}

				while (randomVector1 == randomVector3)
				{
					randomVector3 = mesh.GetVertex((uint)random.Next(0, (int)mesh.VertexCount));
				}
				
				double area = Math.Sqrt(Functions.GetTriArea(new Vector3[] { randomVector1, randomVector2, randomVector3 }));
				int bin = Math.Min((int)(area / binSize), 9);
				AddOneToBin(ref binValues, bin);
			});

			return new HistDescriptor("SquareRootTriangles", binSize, binValues);
		}

		/// <summary>
		/// Atomically adds one to a histogram bin
		/// </summary>
		/// <param name="binValues">The histogram</param>
		/// <param name="bin">The bin index</param>
		private static void AddOneToBin(ref int[] binValues, int bin)
		{
			int tempBinValue;

			do
			{
				tempBinValue = binValues[bin];
			}
			while (Interlocked.CompareExchange(ref binValues[bin], tempBinValue + 1, tempBinValue) != tempBinValue);
		}
	}
}
