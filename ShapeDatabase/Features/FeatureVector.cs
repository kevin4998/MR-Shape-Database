using ShapeDatabase.Features.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for a featurevector
	/// </summary>
	public class FeatureVector
	{
		/// <summary>
		/// The descriptors of the featurevector
		/// </summary>
		public IEnumerable<IDescriptor> Descriptors { get; }

		/// <summary>
		/// Constructor for the featurevector, given all of its descriptors
		/// </summary>
		/// <param name="descriptors">The descriptors that should be saved in the featurevector</param>
		public FeatureVector(IEnumerable<IDescriptor> descriptors)
		{
			Descriptors = descriptors;
		}

		/// <summary>
		/// Method for comparison with another featurevector
		/// </summary>
		/// <param name="vector">The other feature vector</param>
		/// <returns>Doubling representing its resemblance</returns>
		public double Compare(FeatureVector vector)
		{
			throw new NotImplementedException();
		}
	}
}
