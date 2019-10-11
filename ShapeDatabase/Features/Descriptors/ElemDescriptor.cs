using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors
{
	/// <summary>
	/// Class for elementary descriptors
	/// </summary>
	public class ElemDescriptor : IDescriptor<ElemDescriptor>
	{
		/// <summary>
		/// Name of the descriptor
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// Value of the elementary descriptor
		/// </summary>
		public double Value { get; }

		/// <summary>
		/// Constructor of the elementary descriptor
		/// </summary>
		/// <param name="name">Name of the descriptor</param>
		/// <param name="value">Value of the descritor</param>
		public ElemDescriptor(string name, double value)
		{
			Name = name;
			Value = value;
		}

		/// <summary>
		/// Method for comparison with another elementary descriptor
		/// </summary>
		/// <param name="desc">The other elementary descriptor</param>
		/// <returns>Double indicating the resemblance</returns>
		public double Compare(ElemDescriptor desc)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Serializes the descriptor
		/// </summary>
		/// <returns>The serialized descriptor value</returns>
		public string Serialize()
		{
			return Value.ToString();
		}

		/// <summary>
		/// Method for comparison with another elementary descriptor
		/// </summary>
		/// <param name="desc">The other elementary descriptor</param>
		/// <returns>Double indicating the resemblance</returns>
		double IDescriptor.Compare(object desc)
		{
			return Compare(desc as ElemDescriptor);
		}
	}
}
