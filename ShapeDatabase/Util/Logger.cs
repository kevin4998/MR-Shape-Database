using System;
using System.Globalization;
using System.IO;

namespace ShapeDatabase.Util {

	/// <summary>
	/// An object to handle messages which should be send to the user.
	/// </summary>
	public class Logger {

		#region --- Properties ---

		/// <summary>
		/// The globally accessible instance of the logger.
		/// All methods should use this variable for writing information.
		/// </summary>
		public static Logger Instance => lazy.Value;
		private static Lazy<Logger> lazy = new Lazy<Logger>();


		private TextWriter output;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new handler which can send messages to the user.
		/// </summary>
		public Logger() : this(Console.Out) { }

		/// <summary>
		/// Initialises a new handler which can send messages to the user.
		/// </summary>
		/// <param name="output">The directed output where all messages will be written
		/// to. By default this will be the console but it can also be redirected to
		/// a file to keep logs of operations.</param>
		public Logger(TextWriter output) {
			this.output = output ?? throw new ArgumentNullException(nameof(output));
		}

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Sends a message specifying information for the user.
		/// </summary>
		/// <param name="message">The message which should be displayed
		/// without any formats.</param>
		public void LogMessage(string message) => output.WriteLine(message);

		/// <summary>
		/// Sends a message which should show debug information to the user.
		/// </summary>
		/// <param name="message">The message which should be displayed
		/// without any formats.</param>
		public void DebugMessage(string message) {
			if (Settings.ShowDebug) output.WriteLine(message);
		}

		#endregion

		#region --- Static Methods ---

		/// <summary>
		/// Sets the logger to allow redirecting information output.
		/// </summary>
		/// <param name="logger">The new item which will receive logging and debug
		/// information from methods.</param>
		public static void SetLogger(Logger logger) {
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			lazy = new Lazy<Logger>(() => logger);
		}


		/// <summary>
		/// Sends a message specifying information for the user.
		/// </summary>
		/// <param name="message">The message which should be displayed
		/// without any formats.</param>
		public static void Log(string message) =>
			Instance.LogMessage(message);

		/// <summary>
		/// Sends a message specifying information for the user.
		/// </summary>
		/// <param name="format">The format of a message which should be reconstructed
		/// bases on argument information.</param>
		/// <param name="args">The objects which should be included in the format.
		/// The <see cref="ToString()"/> method will be called for all the objects
		/// in order to be able to insert them into the string.</param>
		public static void Log(string format, params object[] args) =>
			Log(Settings.Culture, format, args);

		/// <summary>
		/// Sends a message specifying information for the user.
		/// </summary>
		/// <param name="provider">The formater which helps convert the objects
		/// to the correct string representation.</param>
		/// <param name="format">The format of a message which should be reconstructed
		/// bases on argument information.</param>
		/// <param name="args">The objects which should be included in the format.
		/// The <see cref="ToString()"/> method will be called for all the objects
		/// in order to be able to insert them into the string.</param>
		public static void Log(IFormatProvider provider,
							  string format, params object[] args) =>
			Log(string.Format(provider, format, args));


		/// <summary>
		/// Sends a message which should show debug information to the user.
		/// </summary>
		/// <param name="message">The message which should be displayed
		/// without any formats.</param>
		public static void Debug(string message) =>
			Instance.DebugMessage(message);

		/// <summary>
		/// Sends a message which should show debug information to the user.
		/// </summary>
		/// <param name="format">The format of a message which should be reconstructed
		/// bases on argument information.</param>
		/// <param name="args">The objects which should be included in the format.
		/// The <see cref="ToString()"/> method will be called for all the objects
		/// in order to be able to insert them into the string.</param>
		public static void Debug(string format, params object[] args) =>
			Debug(Settings.Culture, format, args);

		/// <summary>
		/// Sends a message which should show debug information to the user.
		/// </summary>
		/// <param name="provider">The formater which helps convert the objects
		/// to the correct string representation.</param>
		/// <param name="format">The format of a message which should be reconstructed
		/// bases on argument information.</param>
		/// <param name="args">The objects which should be included in the format.
		/// The <see cref="ToString()"/> method will be called for all the objects
		/// in order to be able to insert them into the string.</param>
		public static void Debug(IFormatProvider provider,
							  string format, params object[] args) =>
			Debug(string.Format(provider, format, args));

		#endregion

	}

}
