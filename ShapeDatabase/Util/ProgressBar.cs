// ============================================================
//
// Copyright (c) 2015 DanielSWolf. MIT License
//
// ============================================================
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
// THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ============================================================
//
//	Class: ProgressBar
//
// <OWNER>Daniel S. Wolf</OWNER>
// <EDITOR>Kevin J. J. Westerbaan</EDITOR>
//
// Revisions: November 2019
//			- Allowed for integer increments which shows the number of operations
//			  which have completed.
//
// Purpose: An ASCII progress bar
//
// ============================================================

using System;
using System.Text;
using System.Threading;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Util { 

	/// <summary>
	/// An ASCII progress bar which can be used in console application
	/// to notify the user of a progress of an async application.
	/// </summary>
	public class ProgressBar : IDisposable, IProgress<double> {

		#region --- Properties ---

		private const int blockCount = 10;
		private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
		private const string animation = @"|/-\";

		private readonly Timer timer;

		private double currentProgress = 0;
		private string currentText = string.Empty;
		private bool disposed = false;
		private int animationIndex = -1;

		private double totalTasks;
		private int completedTasks = 0;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new progress bar which can show the application progress.
		/// </summary>
		public ProgressBar(int taskCount) {
			if (taskCount < 0)
				throw new ArgumentException(
					string.Format(
						Settings.Culture,
						Resources.EX_ExpPosValue,
						taskCount
					),
					nameof(taskCount)
				);

			totalTasks = taskCount;
			timer = new Timer(TimerHandler);

			// A progress bar is only for temporary display in a console window.
			// If the console output is redirected to a file, draw nothing.
			// Otherwise, we'll end up with a lot of garbage in the target file.
			if (!Console.IsOutputRedirected) {
				ResetTimer();
			}
		}

		#endregion

		#region --- Instance Methods ---

		public void CompleteTask() {
			int newValue = Interlocked.Increment(ref completedTasks);
			Report(newValue / totalTasks);
		}

		public void Report(double value) {
			// Make sure value is in [0..1] range
			value = Math.Max(0, Math.Min(1, value));
			// Change the report value only if it is bigger.
			double currentValue;
			do {
				currentValue = currentProgress;
				if (value <= currentValue) return;
			} while (Interlocked.Exchange(ref currentProgress, value) != currentValue);
		}

		public void Dispose() {
			lock (timer) {
				disposed = true;
				UpdateText(string.Empty);
			}
		}


		private void TimerHandler(object state) {
			lock (timer) {
				if (disposed) return;

				double progress = currentProgress;
				int progressBlockCount = (int) (progress * blockCount);
				int percent = (int) (progress * 100);
				string text = string.Format(
					Settings.Culture,
					"[{0}{1}] {2,3}% {3}",
					new string('#', progressBlockCount),
					new string('-', blockCount - progressBlockCount),
					percent,
					animation[NextAnimationIndex()]);
				UpdateText(text);

				ResetTimer();
			}
		}

		private int NextAnimationIndex() {
			return (animationIndex = (++animationIndex % animation.Length));
		}

		private void UpdateText(string text) {
			// Get length of common portion
			int commonPrefixLength = 0;
			int commonLength = Math.Min(currentText.Length, text.Length);
			while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength]) {
				commonPrefixLength++;
			}

			// Backtrack to the first differing character
			StringBuilder outputBuilder = new StringBuilder();
			outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

			// Output new suffix
			outputBuilder.Append(text.Substring(commonPrefixLength));

			// If the new text is shorter than the old one: delete overlapping characters
			int overlapCount = currentText.Length - text.Length;
			if (overlapCount > 0) {
				outputBuilder.Append(' ', overlapCount);
				outputBuilder.Append('\b', overlapCount);
			}

			Console.Write(outputBuilder);
			currentText = text;
		}

		private void ResetTimer() {
			timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
		}

		#endregion

	}

}