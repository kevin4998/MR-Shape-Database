using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Features.Descriptors
{
	/// <summary>
	/// Class for calculating and/or importing featurevectors.
	/// </summary>
	public class FeatureManager
	{
		/// <summary>
		/// IDictionary with mesh names as key, and their featurevector as value.
		/// </summary>
		public IDictionary<string, FeatureVector> featureVectors = new Dictionary<string, FeatureVector>();

		/// <summary>
		/// Delegate for calculating a descriptor.
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor is calculated</param>
		/// <returns>The calculated descriptor</returns>
		public delegate IDescriptor DescriptorCalculator(IMesh mesh);

		/// <summary>
		/// IList of all descriptor calculater delegates.
		/// </summary>
		public IList<DescriptorCalculator> descriptorCalculators = new List<DescriptorCalculator>();

		/// <summary>
		/// Constructor method of the featuremanager
		/// </summary>
		/// <param name="descriptorcalculators">The descriptor calculators that can be used to calculate the featurevectors</param>
		public FeatureManager(params DescriptorCalculator[] descriptorcalculators) 
		{
			foreach(DescriptorCalculator calculator in descriptorcalculators)
			{
				descriptorCalculators.Add(calculator);
			}
		}

		/// <summary>
		/// Constructor method of the featuremanager
		/// </summary>
		/// <param name="featurevectors">The featuresvectors, e.g. imported by the FMReader.</param>
		/// <param name="descriptorcalculators">The descriptor calculators that can be used to calculate the featurevectors</param>
		public FeatureManager(IDictionary<string, FeatureVector> featurevectors, params DescriptorCalculator[] descriptorcalculators)
		{
			featureVectors = featurevectors ?? new Dictionary<string, FeatureVector>();
			foreach (DescriptorCalculator calculator in descriptorcalculators)
			{
				descriptorCalculators.Add(calculator);
			}
		}

		/// <summary>
		/// Add a descriptor calculator to the featuremanager
		/// </summary>
		/// <param name="calculator">The descriptor calculator delegate to add</param>
		public void AddCalculator(DescriptorCalculator calculator)
		{
			descriptorCalculators.Add(calculator);
		}

		/// <summary>
		/// Calculate the featurevectores of all meshes in a library using all descriptor calculators of the featuremanager
		/// </summary>
		/// <param name="library">The library of which the featurevectors should be calculated</param>
		public void CalculateVectors(params MeshEntry[] library)
		{
			foreach(MeshEntry entry in library)
			{
				featureVectors.Add(entry.Name, CalculateVector(entry));
			}
		}

		/// <summary>
		/// Calculate the featurevector of one mesh using all descriptor calculators of the featuremanager
		/// </summary>
		/// <param name="entry">The mesh of which the featurevector should be calculated</param>
		/// <returns></returns>
		public FeatureVector CalculateVector(MeshEntry entry)
		{
			featureVectors.Remove(entry.Name);

			List<IDescriptor> descriptors = new List<IDescriptor>();

			foreach (DescriptorCalculator calculator in descriptorCalculators)
			{
				descriptors.Add(calculator(entry.Mesh));
			}

			return new FeatureVector(descriptors);
		}

		/// <summary>
		/// Calculates the resemblance of a mesh with all meshes (featurevectors) of the featuremanager.
		/// </summary>
		/// <param name="mesh">The mesh with which the meshes (featurevectors) should be compared</param>
		/// <returns></returns>
		public List<(string, double)> CalculateResults(MeshEntry mesh)
		{
			throw new NotImplementedException();
		}
	}
}
