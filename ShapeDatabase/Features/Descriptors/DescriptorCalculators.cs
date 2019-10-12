using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for defining all descriptor calculator methods
	/// </summary>
	public static class DescriptorCalculators
	{
		/// <summary>
		/// Elementary descriptor for calculating the surface area of a mesh
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor value is calculated</param>
		/// <returns>The elementary descriptor with the calculated value</returns>
		public static ElemDescriptor SurfaceArea(IMesh mesh)
		{
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			double surfaceArea = 0;

			for (int i = 0; i < mesh.FaceCount; i++)
				surfaceArea += mesh.GetTriArea(i);

			return new ElemDescriptor("SurfaceArea", surfaceArea);
		}
	}
}
