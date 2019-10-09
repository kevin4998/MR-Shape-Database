using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors
{
	public class ElemDescriptor : IDescriptor<ElemDescriptor>
	{
		public string Name { get; }
		public double Value { get; }

		public ElemDescriptor(string name, double value)
		{
			Name = name;
			Value = value;
		}

		public double Compare(ElemDescriptor desc)
		{
			throw new NotImplementedException();
		}

		double IDescriptor.Compare(object desc)
		{
			return Compare(desc as ElemDescriptor);
		}
	}
}
