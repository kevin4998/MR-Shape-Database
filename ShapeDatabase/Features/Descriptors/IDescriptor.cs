using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors
{
	/// <summary>
	/// Interface for descriptors
	/// </summary>
	public interface IDescriptor
	{
		/// <summary>
		/// Name of the descriptor
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Method for comparison with other descriptors
		/// </summary>
		/// <param name="desc">The other descriptor</param>
		/// <returns>Double indicating the resemblance</returns>
		double Compare(object desc);
		/// <summary>
		/// Method for serializing the descriptor
		/// </summary>
		/// <returns>The serialized descriptor value</returns>
		string Serialize();
	}
	/// <summary>
	/// Interface for comparison between different descriptors
	/// </summary>
	/// <typeparam name="T">Type of descriptor</typeparam>
	public interface IDescriptor<T> : IDescriptor where T : IDescriptor<T>
	{
		/// <summary>
		/// Method for comparison with other descriptors
		/// </summary>
		/// <param name="desc">The other descriptor</param>
		/// <returns>Double indicating the resemblance</returns>
		double Compare(T desc);
	}
}
