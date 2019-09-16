using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ShapeDatabase.Util;

namespace ShapeDatabase.IO {

	/// <summary>
	/// The exception that is thrown if a file is read by a reader which does not
	/// support the specified file format.
	/// </summary>
	[DebuggerDisplay("InvalidFormat; actual:{ActulFormat}, expected:{ExpectedFormat}")]
	[Serializable]
	public class InvalidFormatException : IOException {

		// Message attributes, should be refactored for Localisation.
		private const string NoArgMsg = "The provided format could not be processed by this class.";
		private const string ArgsMsg = "The current class cannot read '{0}', expected format(s) '{1}'.";
		// Serialisation names for SerializationInfo.
		private const string serFalse = "Actual";
		private const string serTrue = "Expected";
		// File formats from the exception.
		/// <summary>
		/// The format which was provided to the reader
		/// and couldn't be converted.
		/// </summary>
		public string ActulFormat { get; }
		/// <summary>
		/// The different format(s) which could be converted
		/// by this reader/class.
		/// </summary>
		public string ExpectedFormat { get; }


		/// <summary>
		/// Initialises a new instance of of the <see cref="InvalidFormatException"/> class,
		/// which specified that an invalid file format is used in its reader.
		/// </summary>
		protected InvalidFormatException()
			: this(NoArgMsg) { }

		/// <summary>
		/// Initialises a new instance of of the <see cref="InvalidFormatException"/> class,
		/// which specified that an invalid file format is used in its reader.
		/// </summary>
		/// <param name="message"> The message to display to the user about the exception.</param>
		protected InvalidFormatException(string message)
			: base(message) { }

		/// <summary>
		/// Initialises a new instance of of the <see cref="InvalidFormatException"/> class,
		/// which specified that an invalid file format is used in its reader.
		/// </summary>
		/// <param name="message"> The message to display to the user about the exception.</param>
		/// <param name="innerException">
		/// The exception that is the cause of the current exception. If the innerException
		/// parameter is not null, the current exception is raised in a catch block that
		/// handles the inner exception.
		/// </param>
		protected InvalidFormatException(string message, Exception innerException)
			: base(message, innerException) { }

		/// <summary>
		/// Initialises a new instance of of the <see cref="InvalidFormatException"/> class,
		/// which specified that an invalid file format is used in its reader.
		/// </summary>
		/// <param name="info">The data for serializing or deserializing the object.</param>
		/// <param name="context">The source and destination for the object.</param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected InvalidFormatException(SerializationInfo info, StreamingContext context) 
			: base(info, context) {
			ActulFormat    = info.GetValue<string>(serFalse);
			ExpectedFormat = info.GetValue<string>(serTrue);
		}


		/// <summary>
		/// Initialises a new instance of of the <see cref="InvalidFormatException"/> class,
		/// which specified that an invalid file format is used in its reader.
		/// </summary>
		/// <param name="actulFormat">The format which was provided by the file or reader.</param>
		/// <param name="expectedFormat">The format which this class or reader supports.</param>
		public InvalidFormatException(string actulFormat, string expectedFormat)
			: this(string.Format(ArgsMsg, actulFormat, expectedFormat)) {
			this.ActulFormat = actulFormat;
			this.ExpectedFormat = expectedFormat;
		}

		/// <summary>
		/// Initialises a new instance of of the <see cref="InvalidFormatException"/> class,
		/// which specified that an invalid file format is used in its reader.
		/// </summary>
		/// <param name="actulFormat">The format which was provided by the file or reader.</param>
		/// <param name="expectedFormat">The format which this class or reader supports.</param>
		/// <param name="innerException">
		/// The exception that is the cause of the current exception. If the innerException
		/// parameter is not null, the current exception is raised in a catch block that
		/// handles the inner exception.
		/// </param>
		public InvalidFormatException(string actulFormat, string expectedFormat, Exception innerException)
			: this(string.Format(ArgsMsg, actulFormat, expectedFormat), innerException) {
			this.ActulFormat = actulFormat;
			this.ExpectedFormat = expectedFormat;
		}


		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			if (info == null)
				throw new ArgumentNullException(nameof(info));

			info.AddValue(serFalse, ActulFormat);
			info.AddValue(serTrue, ExpectedFormat);
			base.GetObjectData(info, context);
		}

	}

}
