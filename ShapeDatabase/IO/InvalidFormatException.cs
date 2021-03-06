﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ShapeDatabase.Util;

using static ShapeDatabase.Properties.Resources;

namespace ShapeDatabase.IO {

	/// <summary>
	/// The exception that is thrown if a file is read by a reader which does not
	/// support the specified file format.
	/// </summary>
	[DebuggerDisplay("InvalidFormat; actual:{ActulFormat}, expected:{ExpectedFormat}")]
	[Serializable]
	public class InvalidFormatException : IOException {

		/// <summary>
		/// Serialisation names for SerializationInfo.
		/// </summary>
		private const string serFalse = "Actual";
		private const string serTrue = "Expected";

		/// <summary>
		/// The format which was provided to the reader
		/// and couldn't be converted.
		/// </summary>
		public string ActualFormat { get; }

		/// <summary>
		/// The different format(s) which could be converted
		/// by this reader/class.
		/// </summary>
		public string ExpectedFormat { get; }

		/// <summary>
		/// Initialises a new instance of of the <see cref="InvalidFormatException"/> class,
		/// which specified that an invalid file format is used in its reader.
		/// </summary>
		public InvalidFormatException()
			: this(EX_Invalid_Format_No_Args) { }

		/// <summary>
		/// Initialises a new instance of of the <see cref="InvalidFormatException"/> class,
		/// which specified that an invalid file format is used in its reader.
		/// </summary>
		/// <param name="message">The message to display to the user about the exception.</param>
		public InvalidFormatException(string message)
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
		public InvalidFormatException(string message, Exception innerException)
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
			ActualFormat = info.GetValue<string>(serFalse);
			ExpectedFormat = info.GetValue<string>(serTrue);
		}

		/// <summary>
		/// Initialises a new instance of of the <see cref="InvalidFormatException"/> class,
		/// which specified that an invalid file format is used in its reader.
		/// </summary>
		/// <param name="actulFormat">The format which was provided by the file or reader.</param>
		/// <param name="expectedFormat">The format which this class or reader supports.</param>
		public InvalidFormatException(string actulFormat, string expectedFormat)
			: this(string.Format(Settings.Culture, EX_Invalid_Format, actulFormat, expectedFormat)) {
			this.ActualFormat = actulFormat;
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
			: this(string.Format(Settings.Culture, EX_Invalid_Format, actulFormat, expectedFormat), innerException) {
			this.ActualFormat = actulFormat;
			this.ExpectedFormat = expectedFormat;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			if (info == null)
				throw new ArgumentNullException(nameof(info));

			info.AddValue(serFalse, ActualFormat);
			info.AddValue(serTrue, ExpectedFormat);
			base.GetObjectData(info, context);
		}

	}

}
