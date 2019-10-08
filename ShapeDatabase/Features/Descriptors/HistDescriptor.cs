using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors
{
	public class HistDescriptor : IDescriptor<HistDescriptor>
	{
		public string Name { get; }
		public double Weight { get; }
		public double Offset { get; }

		public double BinSize { get; }

		public int[] BinValues { get; }

		public HistDescriptor(string name, double weight, double offset, double binsize, int[] binvalues)
		{
			Name = name;
			Weight = weight;
			Offset = offset;
			BinSize = binsize;
			BinValues = binvalues;
		}

		public double Compare(HistDescriptor desc)
		{
			throw new NotImplementedException();
		}

		double IDescriptor.Compare(object desc)
		{
			return Compare(desc as HistDescriptor);
		}
	}
}
