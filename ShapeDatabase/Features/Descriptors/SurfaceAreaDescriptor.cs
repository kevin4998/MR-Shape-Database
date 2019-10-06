using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Features.Descriptors
{
	public class SurfaceAreaDescriptor : IDescriptor<GeometryMesh>
	{
		public double CalculateDescriptor(GeometryMesh mesh)
		{
			return 0;
		}
	}
}
