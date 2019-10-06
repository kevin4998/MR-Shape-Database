using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Features.Descriptors
{
	public class DescriptorManager
	{

		IEnumerable<IDescriptor<GeometryMesh>> descriptors;

		public DescriptorManager(IEnumerable<IDescriptor<GeometryMesh>> desc)
		{
			descriptors = desc;
		}

		public void CalulcateDescriptors(IEnumerable<MeshEntry> library)
		{
			;
		}

	}
}
