using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features {

	/// <summary>
	/// The modus to specify how large a single query operation should be.
	/// </summary>
	public enum QuerySize {

		/// <summary>
		/// The query operation size should be a number called K.
		/// </summary>
		KBest,
		/// <summary>
		/// The query operation size should be the size of the class.
		/// </summary>
		Class,
		/// <summary>
		/// The query operation size should retrieve all the shapes in the database.
		/// </summary>
		All

	}

}
