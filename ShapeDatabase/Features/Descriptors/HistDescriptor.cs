using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors
{
	/// <summary>
	/// Class for histogram descriptors
	/// </summary>
	public class HistDescriptor : IDescriptor<HistDescriptor>
	{
		/// <summary>
		/// Name of the descriptor
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The offset value of the first bin
		/// </summary>
		public double Offset { get; }
		/// <summary>
		/// The width of one bin
		/// </summary>
		public double BinSize { get; }
		/// <summary>
		/// The number of values in each of the bins.
		/// </summary>
		public int[] BinValues { get; }

		/// <summary>
		/// The character which is used to seperate values of a single histogram descriptor.
		/// </summary>
		public string HistSeperator => ";";

		/// <summary>
		/// Constructor of the histogram descriptor
		/// </summary>
		/// <param name="name">Name of the descriptor</param>
		/// <param name="offset">Offset value of the descriptor</param>
		/// <param name="binsize">Bin width of the descriptor</param>
		/// <param name="binvalues">Bin values of the descriptor</param>
		public HistDescriptor(string name, double offset, double binsize, int[] binvalues)
		{
			Name = name;
			Offset = offset;
			BinSize = binsize;
			BinValues = binvalues;
		}

		/// <summary>
		/// Method for comparison with another histogram descriptor
		/// </summary>
		/// <param name="desc">The other histogram descriptor</param>
		/// <returns>Double indicating the resemblance</returns>
		public double Compare(HistDescriptor desc)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Serializes the descriptor
		/// </summary>
		/// <returns>The serialized descriptor value</returns>
		public string Serialize()
		{
			string[] histValues = new string[12];
			histValues[0] = Offset.ToString();
			histValues[1] = BinSize.ToString();
			for (int i = 2; i < 12; i++)
			{
				histValues[i] = BinValues[i].ToString();
			}
			
			return string.Join(HistSeperator, histValues);
		}

		/// <summary>
		/// Method for comparison with another histogram descriptor
		/// </summary>
		/// <param name="desc">The other histogram descriptor</param>
		/// <returns>Double indicating the resemblance</returns>
		double IDescriptor.Compare(object desc)
		{
			return Compare(desc as HistDescriptor);
		}
	}
}
