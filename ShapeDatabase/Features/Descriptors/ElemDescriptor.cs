using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors
{
	/// <summary>
	/// A descriptor which can summarise the shape with a single value.
	/// </summary>
	public class ElemDescriptor : BaseDescriptor<ElemDescriptor> {

		#region --- Properties ---

		/// <summary>
		/// Value of the elementary descriptor
		/// </summary>
		public double Value { get; }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Constructor of the elementary descriptor
		/// </summary>
		/// <param name="name">Name of the descriptor</param>
		/// <param name="value">Value of the descritor</param>
		/// <exception cref="ArgumentNullException">If the name is <see langword="null"/>.
		/// </exception>
		public ElemDescriptor(string name, double value) 
			: base(name) {
			Value = value;
		}

		#endregion

		#region --- Instance Methods ---

		public override double Compare(ElemDescriptor desc) {
			throw new NotImplementedException();
		}

		protected override StringBuilder SubSerialise(StringBuilder builder) {
			return builder?.Append(Value);
		}

		public static ElemDescriptor DeSerialise(string serialised) {
			if (string.IsNullOrEmpty(serialised))
				throw new ArgumentNullException(nameof(serialised));
			
			string[] split = serialised.Split(NameSeperator);
			if (split.Length == 0)
				return null;

			string name = split[0];
			string stringValue = (split.Length > 1) ? split[1] : "0";

			if (!double.TryParse(stringValue, out double value))
				return null;

			return new ElemDescriptor(name, value);
		}

		#endregion

	}
}
