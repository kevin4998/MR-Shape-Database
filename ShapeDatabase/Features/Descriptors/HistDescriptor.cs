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
		/// The offset value of the first bin
		/// </summary>
		private double Offset { get; }
		/// <summary>
		/// The width of one bin
		/// </summary>
		private double BinSize { get; }
		/// <summary>
		/// The number of values in each of the bins.
		/// </summary>
		private int[] BinValues { get; }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Constructor of the histogram descriptor
		/// </summary>
		/// <param name="name">Name of the descriptor</param>
		/// <param name="offset">Offset value of the descriptor</param>
		/// <param name="binsize">Bin width of the descriptor</param>
		/// <param name="binvalues">Bin values of the descriptor</param>
		public HistDescriptor(string name, double offset, double binsize, int[] binvalues) 
			: base(name) {
			Offset = offset;
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

			string[] histValues = new string[12];
			histValues[0] = Offset.ToString(format);
			histValues[1] = BinSize.ToString(format);

			for (int i = 2; i < 12; i++)
				histValues[i] = BinValues[i].ToString(format);
			
			return string.Join(HistSeperator, histValues);
		}

		#endregion

	}
}
