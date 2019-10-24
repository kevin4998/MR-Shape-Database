using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for writing the featurevectors of the featuremanager to a csv file.
	/// </summary>
	public class FMWriter : IWriter<FeatureManager>
	{

		#region --- Properties ---

		/// <summary>
		/// The character which is used to seperate values in a csv document.
		/// </summary>
		public static char SeperatorChar => ',';
		/// <summary>
		/// A string value which represent the seperater character.
		/// <see cref="SeperatorChar"/>
		/// </summary>
		public static string Seperator => SeperatorChar.ToString(Settings.Culture);


		private static readonly Lazy<FMWriter> lazy =
			new Lazy<FMWriter>(() => new FMWriter());

		/// <summary>
		/// Provides a writer to convert <see cref="FeatureManager"/>s into csv.
		/// </summary>
		public static FMWriter Instance => lazy.Value;
		public ICollection<string> SupportedFormats => new string[] { ".csv" };

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new writer for featurevectors.
		/// </summary>
		private FMWriter() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Writes featurevectors to a csv file.
		/// </summary>
		/// <param name="type">The featuremanager containing the featurevectors</param>
		/// <param name="location">Location of the csv file</param>
		public void WriteFile(FeatureManager type, string location)
		{
			using (StreamWriter writer = new StreamWriter(location))
			{
				WriteFile(type, writer);
			}
		}

		/// <summary>
		/// Writes featurevectors to a csv file.
		/// </summary>
		/// <param name="type">The featuremanager containing the featurevectors</param>
		/// <param name="location">The streamwriter to be used</param>
		public void WriteFile(FeatureManager type, StreamWriter writer)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			// First line specify the descriptor names
			StringBuilder builder = new StringBuilder("MeshName");
			foreach (string name in type.DescriptorNames)
				builder.Append(Seperator).Append(name);
			writer.WriteLine(builder.ToString());

			// Next lines specify the descriptor values
			foreach(KeyValuePair<string, FeatureVector> vector in type.VectorDictionary)
			{
				builder.Clear();
				builder.Append(vector.Key);
				foreach(IDescriptor desc in vector.Value.Descriptors)
					builder.Append(SeperatorChar).Append(desc.Serialize());

				writer.WriteLine(builder.ToString());
			}

			// Finally make sure that all the data is written.
			writer.Flush();
		}

		public Task WriteFileAsync(FeatureManager fm, string location)
		{
			return Task.Run(() => WriteFile(fm, location));
		}

		public Task WriteFileAsync(FeatureManager fm, StreamWriter writer)
		{
			return Task.Run(() => WriteFile(fm, writer));
		}

		#endregion

	}
}
