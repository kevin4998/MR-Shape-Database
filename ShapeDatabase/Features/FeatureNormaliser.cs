using Accord.Math;
using ShapeDatabase.Features.Descriptors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for normalising featurevectors
	/// </summary>
	public static class FeatureNormaliser {

		/// <summary>
		/// Function for normalising featurevectors.
		/// </summary>
		/// <param name="vectors">The vectors to be normalised.</param>
		/// <returns>The normlised vectors.</returns>
		public static IDictionary<string, FeatureVector> NormaliseVectors(
										IDictionary<string, FeatureVector> vectors) {
			if (vectors == null) return null;

			IDictionary<string, FeatureVector> newVectors
				= new Dictionary<string, FeatureVector>(vectors);
			NormaliseVectors(ref newVectors);
			return newVectors;
		}

		/// <summary>
		/// Function for normalising featurevectors.
		/// </summary>
		/// <param name="vectors">The vectors to be normalised.</param>
		public static void NormaliseVectors(
										ref IDictionary<string, FeatureVector> vectors)
		{
			if (vectors == null) throw new ArgumentNullException(nameof(vectors));
			if (vectors.Count == 0) return;

			IDictionary<string, (double, double)> MinMaxValues = GetMinMax(vectors);

			foreach(KeyValuePair<string, FeatureVector> pair in vectors.ToArray()) { 
				string name = pair.Key;
				FeatureVector vector = pair.Value;
				IDescriptor[] normalisedDescriptors = new IDescriptor[vector.DescriptorCount];

				int descCount = 0;
				foreach (IDescriptor desc in vector.Descriptors) {
					//Normalisation for Elementary Descriptor.
					if(desc is ElemDescriptor elemDesc) {
						string elemName = elemDesc.Name;
						double elemValue = elemDesc.Value;
						double range = MinMaxValues[desc.Name].Item2 - MinMaxValues[desc.Name].Item1;

						double value = (range == 0) ? elemValue : (elemValue - MinMaxValues[desc.Name].Item1) / range;

						normalisedDescriptors[descCount] = new ElemDescriptor(
							elemName,
							value
						);

					//Normalisation for Histogram Descriptor.
					} else if (desc is HistDescriptor histDesc)
						normalisedDescriptors[descCount] = histDesc;

					//Normalisation for unsupported Descriptors.
					else
						throw new NotImplementedException();

					descCount++;
				}

				vectors[name] = new FeatureVector(normalisedDescriptors);
			}
		}

		/// <summary>
		/// Gets the minimum and maximum value of each descriptor of all given vectors
		/// </summary>
		/// <param name="vectors">The vectors</param>
		/// <returns>Dictionary with dscriptor name as key, and (min, max) as value.</returns>
		private static IDictionary<string, (double, double)> GetMinMax(IDictionary<string, FeatureVector> vectors)
		{
			if (vectors == null)
				throw new ArgumentNullException(nameof(vectors));
			if (vectors.Count == 0)
				return new Dictionary<string, (double, double)>();

			IDictionary<string, (double, double)> MinMaxValues = new Dictionary<string, (double, double)>();

			// Initialise the values for all the elementary descriptors.
			FeatureVector exampleVector = vectors.First().Value;
			foreach (ElemDescriptor desc in exampleVector.GetDescriptors<ElemDescriptor>())
				MinMaxValues[desc.Name] = (desc.Value, desc.Value);

			// Iterate over all vectors and update the min and max of each descriptor.
			foreach (KeyValuePair<string, FeatureVector> vector in vectors)
				foreach (ElemDescriptor desc in vector.Value.GetDescriptors<ElemDescriptor>())
				{
					double Min = (desc.Value < MinMaxValues[desc.Name].Item1) ? desc.Value : MinMaxValues[desc.Name].Item1;
					double Max = (desc.Value > MinMaxValues[desc.Name].Item2) ? desc.Value : MinMaxValues[desc.Name].Item2;
					MinMaxValues[desc.Name] = (Min, Max); 
				}

			// Return the found min and max values per descriptor.
			return MinMaxValues;
		}
	}
}
