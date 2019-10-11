using ShapeDatabase.Features.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for building the featuremanager
	/// </summary>
	public class FMBuilder
	{
		/// <summary>
		/// The featurevectors that will be stored in the featuremanager.
		/// </summary>
		protected IDictionary<string, FeatureVector> Values;

		/// <summary>
		/// The descriptor calculator delegates that will be stored in the featuremanager.
		/// </summary>
		protected IList<FeatureManager.DescriptorCalculator> Calculators;

		/// <summary>
		/// Method for adding more descriptor calculator delegates
		/// </summary>
		/// <param name="calculators">The descriptor calculators to be added</param>
		public void AddCalculators(params FeatureManager.DescriptorCalculator[] calculators)
		{
			foreach(FeatureManager.DescriptorCalculator calculator in calculators)
			{
				Calculators.Add(calculator);
			}
		}

		/// <summary>
		/// Method for adding more featurevectors
		/// </summary>
		/// <param name="vectors">The featurevectors to be added</param>
		public void AddFeatures(Dictionary<string, FeatureVector> vectors)
		{
			foreach(KeyValuePair<string, FeatureVector> vector in vectors)
			{
				Values.Add(vector);
			}
		}

		/// <summary>
		/// Constructor of the FMBuilder, given only descriptor calculator delegates.
		/// </summary>
		/// <param name="calculators">The descriptor calculator delegates to be added to the FMBuilder</param>
		public FMBuilder(params FeatureManager.DescriptorCalculator[] calculators)
		{
			Calculators = calculators.ToList();
			Values = new Dictionary< string, FeatureVector>();
		}

		/// <summary>
		/// Constructor of the FMBuilder, given both featurevectors and descriptor calculator delegates.
		/// </summary>
		/// <param name="vectors">The featurevectors to be added to the FMBuilder</param>
		/// <param name="calculators">The descriptor calculator delegates to be added to the FMBuilder</param>
		public FMBuilder(Dictionary<string, FeatureVector> vectors, params FeatureManager.DescriptorCalculator[] calculators)
		{
			Calculators = calculators.ToList();
			Values = vectors;
		}

		/// <summary>
		/// Method for building a featuremanager containing the featurevectors and descriptorcalculators of the FMBuilder.
		/// </summary>
		/// <returns>The featuremanager containing the featurevectors and descriptorcalculators of the FMBuilder</returns>
		public FeatureManager Build()
		{
			return new FeatureManager(Values, Calculators.ToArray());
		}
	}
}
