using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors
{
	public interface IDescriptor<T>
	{
		double CalculateDescriptor(T mesh);
	}
}
