﻿using System;
using System.IO;

namespace ShapeDatabase.Refine {

	/// <summary>
	/// Interface for mesh refinement.
	/// </summary>
	public interface IRefiner<T> {

		/// <summary>
		/// Checks whether mesh needs refinment (true) or not (false).
		/// </summary>
		/// <param name="mesh">The item to check for refinement.</param>
		/// <returns><see langword="true"/> if a refinement operation is needed
		/// for an optimal shape.</returns>
		bool RequireRefinement(T mesh);

		/// <summary>
		/// Refines the mesh. Overwrites the specified file.
		/// </summary>
		/// <param name="file">The file containing the shape to refine.</param>
		/// <param name="attemps">The number of times that this file has been
		/// refined.</param>
		/// <exception cref="ArgumentNullException">If the given file is
		/// <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">If the given file does not exist.</exception>
		void RefineMesh(T mesh, FileInfo file);

	}

}
