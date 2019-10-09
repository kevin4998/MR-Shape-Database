using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Features.Descriptors
{
	public class FeatureManager
	{
		public Dictionary<string, FeatureVector> featureVectors = new Dictionary<string, FeatureVector>();

		public delegate IDescriptor DescriptorCalculator(IMesh mesh);

		public List<DescriptorCalculator> descriptorCalculators = new List<DescriptorCalculator>();

		public FeatureManager(params DescriptorCalculator[] descriptorcalculators) 
		{
			descriptorCalculators.AddRange(descriptorcalculators);
		}

		public FeatureManager(Dictionary<string, FeatureVector> featurevectors, params DescriptorCalculator[] descriptorcalculators)
		{
			featureVectors = featurevectors != null ? featurevectors : new Dictionary<string, FeatureVector>();
			descriptorCalculators.AddRange(descriptorcalculators);
		}

		public void AddCalculator(DescriptorCalculator calculator)
		{
			descriptorCalculators.Add(calculator);
		}

		public void CalculateVectors(params MeshEntry[] library)
		{
			foreach(MeshEntry entry in library)
			{
				featureVectors.Add(entry.Name, CalculateVector(entry));
			}
		}

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

		public List<(string, double)> CalculateResults(MeshEntry mesh)
		{
			throw new NotImplementedException();
		}

		public double CalculateResult(MeshEntry mesh)
		{
			throw new NotImplementedException();
		}
	}
}
