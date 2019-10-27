using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Util.Attributes {

	/// <summary>
	/// An attribute to identify that a value is ignored during serialisation/
	/// deserialisation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class IgnoreAttribute : Attribute { }

}
