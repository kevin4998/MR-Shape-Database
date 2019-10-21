using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features {

	/// <summary>
	/// A class to represent a collection of descriptor values for a shape.
	/// </summary>
	public class FeatureVector : Util.IComparable<FeatureVector> {

		/// <summary>
		/// A collection of descriptors which have been saved in this
		/// <see cref="FeatureVector"/>.
		/// </summary>
		public IEnumerable<IDescriptor> Descriptors { get; }

		/// <summary>
		/// Constructor for the featurevector, given all of its descriptors
		/// </summary>
		/// <param name="descriptors">A collection of specifications for a specific shape.
		/// </param>
		/// <exception cref="ArgumentNullException">if the given descriptors are
		/// <see langword="null"/>.</exception>
		public FeatureVector(IEnumerable<IDescriptor> descriptors) {
			Descriptors = descriptors
				?? throw new ArgumentNullException(nameof(descriptors));
		}

		/// <summary>
		/// Compare with another using the Proportional Transportation Distance.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The PTD</returns>
		public double Compare(FeatureVector vector) 
		{
			(double[] X_values, double[] X_weights) = CreatePTDArray(Descriptors);
			(double[] Y_values, double[] Y_weights) = CreatePTDArray(vector.Descriptors);

			return Functions.CalculatePTD(X_values, X_weights, Y_values, Y_weights);
		}

		/// <summary>
		/// Creates value and weight arrays that can be used for calculating the PTD.
		/// </summary>
		/// <param name="descriptors">The descriptors</param>
		/// <returns>The value (Item1) and weight (Item2) arrays</returns>
		private (double[], double[]) CreatePTDArray(IEnumerable<IDescriptor> descriptors)
		{
			List<double> values = new List<double>();
			List<double> weights = new List<double>();

			foreach(IDescriptor desc in descriptors)
			{
				if(desc is ElemDescriptor)
				{
					values.Add(((ElemDescriptor)desc).Value);
					weights.Add(1);
				}
				else
				{
					HistDescriptor hist_desc = (HistDescriptor)desc;
					values.AddRange(hist_desc.BinValues.Select(y => (double)y).ToArray());
					weights.AddRange(Enumerable.Repeat((double)1 / hist_desc.BinValues.Length, hist_desc.BinValues.Length));
				}
			}

			return (values.ToArray(), weights.ToArray());
		}
	}
}
