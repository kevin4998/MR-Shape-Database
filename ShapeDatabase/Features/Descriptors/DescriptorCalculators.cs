using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	public static class DescriptorCalculators
	{
		public static ElemDescriptor SurfaceArea(IMesh mesh)
		{
			double surfaceArea = 0;

			for (int i = 0; i < mesh.FaceCount; i++)
			{
				surfaceArea += mesh.GetTriArea(i);
			}

			return new ElemDescriptor("SurfaceArea", 1, surfaceArea);
		}
	}
}
