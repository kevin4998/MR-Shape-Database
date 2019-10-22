using ShapeDatabase.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors {

	/// <summary>
	/// A class for complex descriptors which hold multiple values.
	/// These values are stored by making use if histograms where the values
	/// are grouped in buckets or bins.
	/// </summary>
	public class HistDescriptor : BaseDescriptor<HistDescriptor> {

		#region --- Properties ---

		/// <summary>
		/// The character which is used to seperate values of
		/// a single histogram descriptor.
		/// </summary>
		public static string HistSeperator => ";";

		/// <summary>
		/// The width of one bin
		/// </summary>
		private double BinSize { get; }
		/// <summary>
		/// The number of values in each of the bins.
		/// </summary>
		public float[] BinValues { get; }

		public int BinCount => BinValues.Length;

		/// <summary>
		/// Weight of the histogram descriptor
		/// </summary>
		public double Weight { get; } = 1;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Constructor of the histogram descriptor
		/// </summary>
		/// <param name="name">Name of the descriptor</param>
		/// <param name="binsize">Bin width of the descriptor</param>
		/// <param name="binvalues">Bin values of the descriptor</param>
		public HistDescriptor(string name, double binsize, float[] binvalues) 
			: base(name) {
			BinSize = binsize;
			BinValues = binvalues;
		}

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Calculates the EMD distance with another histogram descriptor.
		/// </summary>
		/// <param name="desc">The other histogram descriptor</param>
		/// <returns>The distance (0 = Equal Descriptors, 1 = Completely Different)</returns>
		public override double Compare(HistDescriptor desc) {
			if (desc == null)
				throw new ArgumentNullException(nameof(desc));

			double[] weights = Enumerable.Repeat(1d, BinValues.Length).ToArray();
			return Functions.CalculatePTD(
				BinValues.Cast<float, double>(x => x),
				weights,
				desc.BinValues.Cast<float, double>(x => x),
				weights);
		}

		/// <summary>
		/// Serializes the histogram descriptor.
		/// </summary>
		/// <returns>Serialized histogram descriptor</returns>
		public override string Serialize() {
			IFormatProvider format = Settings.Culture;

			string[] histValues = new string[BinValues.Length + 1];
			histValues[0] = BinSize.ToString(format);

			for (int i = 1; i < BinValues.Length + 1; i++)
				histValues[i] = BinValues[i - 1].ToString(format);
			
			return string.Join(HistSeperator, histValues);
		}

		#endregion

		#region -- Static Methods --

		/// <summary>
		/// Converts the integer based histogram in a float based histogram
		/// where the float value is always between 0 and 1 representating
		/// the persentage of values that are within that range.
		/// </summary>
		/// <returns>The normalised HistDescriptor</returns>
		public HistDescriptor Normalise()
		{
			float[] normalised = new float[BinValues.Length];
			float total = 0;
			foreach (float binSize in BinValues)
				total += binSize;

			for (int i = BinValues.Length - 1; i >= 0; i--)
				normalised[i] = BinValues[i] / total;

			for(int i = 0; i < BinValues.Length - 1; i++)
			{
				normalised[i + 1] += normalised[i];
			}

			return new HistDescriptor(base.Name, BinSize, normalised);
		}

		#endregion
	}
}
