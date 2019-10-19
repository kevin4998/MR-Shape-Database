using ShapeDatabase.Features.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for normalising featurevectors
	/// </summary>
	public class FeatureNormaliser
	{
		private static readonly Lazy<FeatureNormaliser> lazy
			= new Lazy<FeatureNormaliser>();

		/// <summary>
		/// Gives a refiner to normalise meshes.
		/// </summary>
		public static FeatureNormaliser Instance => lazy.Value;

		/// <summary>
		/// Function for normalising featurevectors
		/// </summary>
		/// <param name="vectors">The vectors to be normalised</param>
		/// <returns>The normlised vectors</returns>
		public IDictionary<string, FeatureVector> NormaliseVectors(IDictionary<string, FeatureVector> vectors)
		{
			IDictionary<string, FeatureVector> normalisedVectors = vectors;

			IDictionary<string, double> averages = GetAverages(vectors);
			IDictionary<string, double> deviations = GetStandardDeviations(vectors, averages);

			foreach(KeyValuePair<string, FeatureVector> vector in vectors)
			{
				IDescriptor[] normalisedDescriptors = new IDescriptor[vector.Value.Descriptors.Count()];

				int j = 0;
				foreach (IDescriptor desc in vector.Value.Descriptors)
				{
					//Normalisation for Elementary Descriptor
					if(desc is ElemDescriptor)
					{
						ElemDescriptor unnormalisedDescriptor = (ElemDescriptor)vector.Value.Descriptors.ElementAt(j);

						normalisedDescriptors[j] = new ElemDescriptor(unnormalisedDescriptor.Name, (unnormalisedDescriptor.Value - averages[unnormalisedDescriptor.Name]) / deviations[unnormalisedDescriptor.Name]);
					}
					//Normalisation for Histogram Descriptor
					else
					{
						//TODO
						normalisedDescriptors[j] = desc;
					}
					j++;
				}

				normalisedVectors[vector.Key] = new FeatureVector(normalisedDescriptors);
			}

			return normalisedVectors;
		}

		/// <summary>
		/// Calculates the averages of all elementary descriptors.
		/// </summary>
		/// <param name="vectors">The featurevectors</param>
		/// <returns>IDictionary containing all (and only) elementary descriptor names, including their averages.</returns>
		private IDictionary<string, double> GetAverages(IDictionary<string, FeatureVector> vectors)
		{
			Dictionary<string, double> totalSums = new Dictionary<string, double>();

			foreach (IDescriptor desc in vectors.ElementAt(0).Value.Descriptors.Where(x => x is ElemDescriptor))
			{
				totalSums[desc.Name] = 0;
			}

			foreach (KeyValuePair<string, FeatureVector> vector in vectors)
			{
				foreach (IDescriptor desc in vector.Value.Descriptors.Where(x => x is ElemDescriptor))
				{
					totalSums[desc.Name] += ((ElemDescriptor)desc).Value;
				}
			}

			Dictionary<string, double> averages = new Dictionary<string, double>();

			foreach(KeyValuePair<string, double> sum in totalSums)
			{
				averages[sum.Key] = sum.Value / vectors.Count;
			}

			return averages;
		}

		/// <summary>
		/// Calculates the standard deviation of all elementary descriptors.
		/// </summary>
		/// <param name="vectors">The featurevectors</param>
		/// <param name="averages">IDictionary containing all elementary descriptor names, and their averages</param>
		/// <returns>IDictionary containing all (and only) elementary descriptor names, including their averages.</returns>
		public IDictionary<string, double> GetStandardDeviations(IDictionary<string, FeatureVector> vectors, IDictionary<string, double> averages)
		{
			Dictionary<string, double> squaredDifferenceSums = new Dictionary<string, double>();

			foreach (IDescriptor desc in vectors.ElementAt(0).Value.Descriptors.Where(x => x is ElemDescriptor))
			{
				squaredDifferenceSums[desc.Name] = 0;
			}

			foreach (KeyValuePair<string, FeatureVector> vector in vectors)
			{
				foreach (IDescriptor desc in vector.Value.Descriptors.Where(x => x is ElemDescriptor))
				{
					squaredDifferenceSums[desc.Name] += Math.Pow(((ElemDescriptor)desc).Value - averages[desc.Name], 2);
				}
			}

			Dictionary<string, double> deviations = new Dictionary<string, double>();

			foreach (KeyValuePair<string, double> sum in squaredDifferenceSums)
			{
				deviations[sum.Key] = Math.Sqrt(sum.Value / vectors.Count);
			}

			return deviations;
		}
	}
}
