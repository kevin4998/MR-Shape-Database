using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		public override string Serialize() {
			IFormatProvider format = Settings.Culture;
			return Value.ToString(format);
		}

		#endregion

	}
}
