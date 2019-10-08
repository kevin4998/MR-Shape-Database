using ShapeDatabase.Features.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	public class FeatureVector
	{
		public IEnumerable<IDescriptor> Descriptors { get; }

		public FeatureVector(IEnumerable<IDescriptor> descriptors)
		{
			Descriptors = descriptors;
		}

		public double Compare(FeatureVector vector)
		{
			throw new NotImplementedException();
		}
	}
}
