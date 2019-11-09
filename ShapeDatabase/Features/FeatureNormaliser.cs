using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Util;

namespace ShapeDatabase.Features {
	
	/// <summary>
	/// Class for normalising featurevectors
	/// </summary>
	public class FeatureNormaliser {

		private static readonly Lazy<FeatureNormaliser> lazy =
			new Lazy<FeatureNormaliser>(() => new FeatureNormaliser());

		/// <summary>
		/// Creates and instance of the featurenormaliser.
		/// </summary>
		public static FeatureNormaliser Instance => lazy.Value;

		/// <summary>
		/// The minimum and maximum values of all descriptors <descriptorname, (min, max)>.
		/// </summary>
		private readonly IDictionary<string, (double, double)> MinMaxValues;

		/// <summary>
		/// Initialization of the featurenormaliser.
		/// </summary>
		public FeatureNormaliser() {
			MinMaxValues = new Dictionary<string, (double, double)>();
		}

		/// <summary>
		/// Class used to normalise one single featurevector.
		/// </summary>
		/// <param name="vector">The featurevector.</param>
		/// <returns>The normalised featurevector.</returns>
		public FeatureVector NormaliseVector(FeatureVector vector) {
			IDictionary<string, FeatureVector> singleDic = new Dictionary<string, FeatureVector>() { { "singleVec", vector} };
			NormaliseVectors(ref singleDic);
			return singleDic["singleVec"];
		}

		/// <summary>
		/// Function for normalising featurevectors.
		/// </summary>
		/// <param name="vectors">The vectors to be normalised.</param>
		public void NormaliseVectors(ref IDictionary<string, FeatureVector> vectors) {
			if (vectors == null)
				throw new ArgumentNullException(nameof(vectors));
			if (vectors.Count == 0)
				return;
			if (MinMaxValues.Count == 0)
				UpdateMinMaxDictionary(vectors);

			using (ProgressBar progress = new ProgressBar(vectors.Count)) {
				foreach (KeyValuePair<string, FeatureVector> pair in vectors.ToArray()) {
					string name = pair.Key;
					FeatureVector vector = pair.Value;
					IDescriptor[] normalisedDescriptors = new IDescriptor[vector.DescriptorCount];

					int descCount = 0;
					foreach (IDescriptor desc in vector.Descriptors) {
						//Normalisation for Elementary Descriptor.
						if (desc is ElemDescriptor elemDesc) {
							string elemName = elemDesc.Name;
							double elemValue = elemDesc.Value;
							(double min, double max) = MinMaxValues[desc.Name];
							double range = max - min;

							if(elemValue == double.PositiveInfinity)
							{
								normalisedDescriptors[descCount] = new ElemDescriptor(
								elemName, 1);
							}
							else
							{
								if (range != 0)
								{
									elemValue = Math.Max(min, elemValue);
									elemValue = Math.Min(max, elemValue);
									elemValue = (elemValue - min) / range;
								}
								else
								{
									elemValue = Math.Max(0, elemValue);
									elemValue = Math.Min(1, elemValue);
								}

								normalisedDescriptors[descCount] = new ElemDescriptor(
									elemName,
									elemValue
								);
							}

							//Normalisation for Histogram Descriptor.
						} else if (desc is HistDescriptor histDesc)
							normalisedDescriptors[descCount] = histDesc;

						//Normalisation for unsupported Descriptors.
						else
							throw new NotImplementedException();

						descCount++;
					}

					vectors[name] = new FeatureVector(normalisedDescriptors);
					progress.CompleteTask();
				}
			}
		}

		/// <summary>
		/// Updates the MinMax dictionary, used for normalisation.
		/// </summary>
		/// <param name="vectors">The vectors</param>
		public void UpdateMinMaxDictionary(IDictionary<string, FeatureVector> vectors) {
			if (vectors == null)
				throw new ArgumentNullException(nameof(vectors));
			if (vectors.Count == 0)
				return;

			// Initialise descriptors if they had not been added before.
			FeatureVector exampleVector = vectors.First().Value;
			foreach (ElemDescriptor desc in exampleVector.GetDescriptors<ElemDescriptor>()) {
				if (!MinMaxValues.ContainsKey(desc.Name))
					MinMaxValues[desc.Name] = (desc.Value, desc.Value);
			}

			// Iterate over all vectors and update the min and max of each descriptor.
			foreach (KeyValuePair<string, FeatureVector> vector in vectors)
				foreach (ElemDescriptor desc in vector.Value.GetDescriptors<ElemDescriptor>()) {
					double Min = (desc.Value != double.PositiveInfinity && desc.Value < MinMaxValues[desc.Name].Item1) ? desc.Value : MinMaxValues[desc.Name].Item1;
					double Max = (desc.Value != double.PositiveInfinity && desc.Value > MinMaxValues[desc.Name].Item2) ? desc.Value : MinMaxValues[desc.Name].Item2;
					MinMaxValues[desc.Name] = (Min, Max);
				}
		}
	}
}
