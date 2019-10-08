using ShapeDatabase.Features.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	public class FMBuilder
	{
		protected Dictionary<string, FeatureVector> Values;

		protected FeatureManager.DescriptorCalculator[] Calculators;

		public void AddCalculators(params FeatureManager.DescriptorCalculator[] calculators)
		{
			Calculators = calculators;
		}
		public void ImportFeatures(Dictionary<string, FeatureVector> vectors)
		{
			Values = vectors;
		}

		public FMBuilder(params FeatureManager.DescriptorCalculator[] calculators)
		{
			Calculators = calculators;
			Values = null;
		}

		public FMBuilder(Dictionary<string, FeatureVector> vectors, params FeatureManager.DescriptorCalculator[] calculators)
		{
			Calculators = calculators;
			Values = vectors;
		}

		public FeatureManager Build()
		{
			return new FeatureManager(Values, Calculators);
		}
	}
}
