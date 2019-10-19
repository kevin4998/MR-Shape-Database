using ShapeDatabase.Features.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features {

	/// <summary>
	/// Class for building the featuremanager
	/// </summary>
	public class FMBuilder {

		#region --- Properties ---

		/// <summary>
		/// The featurevectors that will be stored in the featuremanager.
		/// </summary>
		private IDictionary<string, FeatureVector> Values { get; }

		/// <summary>
		/// The descriptor calculator delegates that will be stored in the featuremanager.
		/// </summary>
		private IList<FeatureManager.DescriptorCalculator> Calculators { get; }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Constructor of the FMBuilder, given only descriptor calculator delegates.
		/// </summary>
		/// <param name="calculators">The descriptor calculator delegates to be added to the FMBuilder</param>
		public FMBuilder(params FeatureManager.DescriptorCalculator[] calculators) 
			: this(new Dictionary<string, FeatureVector>(), calculators) { }

		/// <summary>
		/// Constructor of the FMBuilder, given both featurevectors and descriptor calculator delegates.
		/// </summary>
		/// <param name="vectors">The featurevectors to be added to the FMBuilder</param>
		/// <param name="calculators">The descriptor calculator delegates to be added to the FMBuilder</param>
		public FMBuilder(Dictionary<string, FeatureVector> vectors,
						 params FeatureManager.DescriptorCalculator[] calculators) {
			Values = vectors ?? throw new ArgumentNullException(nameof(vectors));
			Calculators = new List<FeatureManager.DescriptorCalculator>();

			AddLocalCalculators();
			AddCalculators(calculators);
		}

		#endregion

		#region --- Instance Methods ---

		// Add the calculators which are already defined here so it should not be
		// repeated in all the builder constructors.
		private void AddLocalCalculators() {
			foreach(FeatureManager.DescriptorCalculator calculator
					in DescriptorCalculators.Descriptors)
				Calculators.Add(calculator);
		}


		/// <summary>
		/// Method for adding more descriptor calculator delegates
		/// </summary>
		/// <param name="calculators">The descriptor calculators to be added</param>
		public void AddCalculators(params FeatureManager.DescriptorCalculator[] calculators) {
			if (calculators != null)
				foreach(FeatureManager.DescriptorCalculator calculator in calculators)
					if (calculator != null)
						Calculators.Add(calculator);
		}

		/// <summary>
		/// Method for adding more featurevectors
		/// </summary>
		/// <param name="vectors">The featurevectors to be added</param>
		public void AddFeatures(IDictionary<string, FeatureVector> vectors) {
			if (vectors == null) throw new ArgumentNullException(nameof(vectors));

			foreach(KeyValuePair<string, FeatureVector> vector in vectors)
				Values.Add(vector);
		}

		/// <summary>
		/// Method for adding more featurevectors
		/// </summary>
		/// <param name="vectors">The featurevectors to be added</param>
		public void AddFeatures(params (string, FeatureVector)[] vectors) {
			if (vectors == null || vectors.Length == 0)
				return;

			foreach((string meshName, FeatureVector values) in vectors)
				if (!string.IsNullOrEmpty(meshName) && values != null)
					Values.Add(meshName, values);
		}


		/// <summary>
		/// Method for building a featuremanager containing the featurevectors and descriptorcalculators of the FMBuilder.
		/// </summary>
		/// <returns>The featuremanager containing the featurevectors and descriptorcalculators of the FMBuilder</returns>
		public FeatureManager Build() {
			return new FeatureManager(Values, Calculators.ToArray());
		}

		#endregion

	}
}
