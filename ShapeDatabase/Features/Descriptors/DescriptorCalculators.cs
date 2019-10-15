﻿using ShapeDatabase.Features.Descriptors;
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

			Parallel.For(0, mesh.VertexCount, i =>
			{
				Parallel.For(i, mesh.VertexCount, j =>
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
			Vector3 BaryCenter = NormalisationRefiner.FindBaryCenter(mesh);

			double BinSize = 0.2;
			int[] BinValues = new int[10];

			Parallel.For(0, mesh.VertexCount, i =>
			{
				float Distance = Vector3.Distance(mesh.GetVertex((uint)i), BaryCenter);
				int Bin = Math.Min((int)(Distance / BinSize), 9);
				int TempBinValue;

				do
				{
					TempBinValue = BinValues[Bin];
				}
				while (Interlocked.CompareExchange(ref BinValues[Bin], TempBinValue + 1, TempBinValue) != TempBinValue);
			});
					   
			return new HistDescriptor("DistanceBarycenter", BinSize, BinValues);
		}
	}
}
