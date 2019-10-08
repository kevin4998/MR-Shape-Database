using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors
{
	public interface IDescriptor
	{
		string Name { get; }

		double Weight { get; }

		double Compare(object desc);
	}

	public interface IDescriptor<T> : IDescriptor where T : IDescriptor<T>
	{
		double Compare(T desc);
	}
}
