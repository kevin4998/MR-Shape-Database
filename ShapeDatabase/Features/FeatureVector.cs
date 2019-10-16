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

		public double Compare(FeatureVector vector) {
			//TODO; currently returns 0.5 for testing.
			return 0.5;
		}
	}
}
