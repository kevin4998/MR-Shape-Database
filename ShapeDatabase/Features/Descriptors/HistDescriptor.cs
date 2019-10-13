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
		public static char HistSeperator => ';';

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

		protected override StringBuilder SubSerialise(StringBuilder builder) {
			if (builder == null)
				throw new ArgumentNullException(nameof(builder));

			builder
				.Append(Offset)
				.Append(HistSeperator)
				.Append(BinSize);

			foreach (int value in BinValues)
				builder.Append(HistSeperator).Append(value);

			return builder;
		}

		public static HistDescriptor Deserialise(string serialised) {
			if (string.IsNullOrEmpty(serialised))
				throw new ArgumentNullException(nameof(serialised));

			string[] split = serialised.Split(NameSeperator);
			if (split.Length < 2)
				return null;

			string name = split[0];
			string histValue = string.Join(HistSeperator.ToString(Settings.Culture),
										   split, 1, split.Length - 1);

			string[] valueSplit = histValue.Split(HistSeperator);
			if (valueSplit.Length < 3)
				return null;

			if (!double.TryParse(valueSplit[0], out double offset)
				|| !double.TryParse(valueSplit[1], out double binSize))
				return null;

			int[] values = new int[valueSplit.Length - 2];
			for (int i = 2; i < valueSplit.Length; i++)
				if (int.TryParse(valueSplit[i], out values[i]))
					return null;

			return new HistDescriptor(name, offset, binSize, values);
		}

		#endregion

	}
}
