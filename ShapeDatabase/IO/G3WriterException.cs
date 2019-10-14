using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using g3;
using ShapeDatabase.Util;

using static ShapeDatabase.Properties.Resources;

namespace ShapeDatabase.IO {

	/// <summary>
	/// The exception that is thrown if a file could not be written to by the
	/// <see cref="g3"/> libraries writers.
	/// </summary>
	[DebuggerDisplay("G3WriterException; Result:{Result}")]
	[Serializable]
	public class G3WriterException : IOException {

		#region --- Properties ---

		// Serialisation names for SerializationInfo.
		private const string serCode = "Code";
		/// <summary>
		/// The message returned from the writer to inform the user of what
		/// went wrong in the deserialisation process.
		/// </summary>
		public IOWriteResult Result { get; }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new instance of of the <see cref="G3WriterException"/> class,
		/// which specified that a problem occured in an external writer.
		/// </summary>
		protected G3WriterException()
			: base() {
			throw new InvalidOperationException(EX_G3_Writer);
		}

		/// <summary>
		/// Initialises a new instance of of the <see cref="G3WriterException"/> class,
		/// which specified that a problem occured in an external writer.
		/// </summary>
		/// <param name="message">The error message to show to the user.</param>
		protected G3WriterException(string message)
			: base(message) { }

		/// <summary>
		/// Initialises a new instance of of the <see cref="G3WriterException"/> class,
		/// which specified that a problem occured in an external writer.
		/// </summary>
		/// <param name="message">The error message to show to the user.</param>
		/// <param name="innerException">The previous exception which caused this one.</param>
		protected G3WriterException(string message, Exception innerException)
			: base(message, innerException) { }


		/// <summary>
		/// Initialises a new instance of of the <see cref="G3WriterException"/> class,
		/// which specified that a problem occured in an external writer.
		/// </summary>
		/// <param name="result">The error message provided by the other writer.</param>
		public G3WriterException(IOWriteResult result)
			: this(result, null) { }

		/// <summary>
		/// Initialises a new instance of of the <see cref="G3WriterException"/> class,
		/// which specified that a problem occured in an external writer.
		/// </summary>
		/// <param name="result">The error message provided by the other writer.</param>
		/// <param name="innerException">
		/// The exception that is the cause of the current exception. If the innerException
		/// parameter is not null, the current exception is raised in a catch block that
		/// handles the inner exception.
		/// </param>
		public G3WriterException(IOWriteResult result, Exception innerException)
			: this(result.message, innerException) {
			Result = result;
		}


		/// <summary>
		/// Initialises a new instance of of the <see cref="G3WriterException"/> class,
		/// which specified that a problem occured in an external writer.
		/// </summary>
		/// <param name="info">The data for serializing or deserializing the object.</param>
		/// <param name="context">The source and destination for the object.</param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected G3WriterException(SerializationInfo info, StreamingContext context) 
			: base(info, context) {
			Result = info.GetValue<IOWriteResult>(serCode);
		}

		#endregion

		#region --- Isntance Methods ---

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			if (info == null)
				throw new ArgumentNullException(nameof(info));

			info.AddValue(serCode, Result);
			base.GetObjectData(info, context);
		}

		#endregion

	}

}
