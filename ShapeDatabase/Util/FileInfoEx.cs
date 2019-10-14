using System;
using System.IO;

namespace ShapeDatabase.Util {

	/// <summary>
	/// A class to provide extra operations on files.
	/// </summary>
	public static class FileInfoEx {

		/// <summary>
		/// Moves a specified file to a new location, providing the option to specify
		/// a new file name. The file which was previously stored at this location
		/// will be ovewritten.
		/// </summary>
		/// <param name="current">The specifications of the file to move.</param>
		/// <param name="dir">The new directory with name to move to.</param>
		public static void MoveAndOverwrite(this FileInfo current, string dir) {
			if (current == null)
				throw new ArgumentNullException(nameof(current));
			if (string.IsNullOrEmpty(dir))
				throw new ArgumentNullException(nameof(dir));

			FileInfo newFile = new FileInfo(dir);
			// Check if it is the same file, so we don't need to move it.
			if (newFile.FullName.Equals(current.FullName))
				return;
			// If it is not the same file, check if there is already on there,
			// if so remove it.
			if (newFile.Exists)
				newFile.Delete();
			// Now safely move to the new location.
			current.MoveTo(dir);
		}

		/// <summary>
		/// Gets only the name of the current file with the extension part.
		/// </summary>
		/// <param name="current">An instance containing information about the
		/// current file.</param>
		/// <returns>A string containing the name of the current file.</returns>
		public static string NameWithoutExtension(this FileInfo current) {
			if (current == null)
				throw new ArgumentNullException(nameof(current));
			return Path.GetFileNameWithoutExtension(current.FullName);
		}

	}
}
