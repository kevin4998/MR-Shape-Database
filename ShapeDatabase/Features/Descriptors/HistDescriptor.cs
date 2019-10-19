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
		private float[] BinValues { get; }

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

		public override double Compare(HistDescriptor desc) {
			if (desc == null)
				throw new ArgumentNullException(nameof(desc));
			throw new NotImplementedException();
		}

		public override string Serialize() {
			IFormatProvider format = Settings.Culture;

			string[] histValues = new string[BinValues.Length + 1];
			histValues[0] = BinSize.ToString(format);

			for (int i = 1; i < 11; i++)
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

			return new HistDescriptor(base.Name, BinSize, normalised);
		}

		#endregion
	}
}
