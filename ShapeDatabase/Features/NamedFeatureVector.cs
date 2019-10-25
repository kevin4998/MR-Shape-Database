using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	public class NamedFeatureVector
	{
		public string Name;

		public FeatureVector FeatureVector;

		public NamedFeatureVector(string name, FeatureVector featurevector)
		{
			Name = name;
			FeatureVector = featurevector;
		}
	}
}
