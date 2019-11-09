using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

using static ShapeDatabase.Properties.Resources;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// The exception that is thrown when a snapshot could not be made
	/// in the current situation. Details of the situation are stored
	/// in the message.
	/// </summary>
	[Serializable]
	public class SnapShotException : Exception {

		/// <summary>
		/// Initialises a new message explaining that a snapshot already exists,
		/// and therefore a new one could not be made.
		/// </summary>
		public SnapShotException() : this(EX_Dubble_Snapshot) { }

		/// <summary>
		/// Initialises a new instance of the <see cref="SnapShotException"/> class,
		/// which specifies a problem with creating the current snapshot.
		/// </summary>
		/// <param name="message">The error message to show to the user.</param>
		public SnapShotException(string message) : base(message) { }

		/// <summary>
		/// Initialises a new instance of the <see cref="SnapShotException"/> class,
		/// which specifies a problem with creating the current snapshot.
		/// </summary>
		/// <param name="message">The error message to show to the user.</param>
		/// <param name="innerException">The previous exception which caused this one.
		/// </param>
		public SnapShotException(string message, Exception innerException)
			: base(message, innerException) { }

		/// <summary>
		/// Initialises a new instance of the <see cref="SnapShotException"/> class,
		/// which specifies a problem with creating the current snapshot.
		/// </summary>
		/// <param name="info">The data for serializing or deserializing the object.
		/// </param>
		/// <param name="context">The source and destination for the object.</param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected SnapShotException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }
	}
}
