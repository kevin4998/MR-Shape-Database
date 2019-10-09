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
		public static string Seperator => SeperatorChar.ToString();
		/// <summary>
		/// The character which is used to seperate values of a single histogram descriptor.
		/// </summary>
		public static char HistSeperatorChar => ';';
		/// <summary>
		/// A string value which represent the histogram seperater character.
		/// <see cref="HistSeperatorChar"/>
		/// </summary>
		public static string HistSeperator => HistSeperatorChar.ToString();

		private static readonly Lazy<FMWriter> lazy =
			new Lazy<FMWriter>(() => new FMWriter());

		/// <summary>
		/// Provides a writer to convert <see cref="RecordHolder"/>s into csv.
		/// </summary>
		public static FMWriter Instance => lazy.Value;
		public string[] SupportedFormats => new string[] { ".csv" };

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instatiates a new writer for featurevectors.
		/// </summary>
		private FMWriter() { }

		#endregion

		#region --- Instance Methods ---

		public void WriteFile(FeatureManager type, string location)
		{
			using (StreamWriter writer = new StreamWriter(location))
			{
				WriteFile(type, writer);
			}
		}

		public void WriteFile(FeatureManager type, StreamWriter writer)
		{
			if (type.featureVectors == null)
				throw new ArgumentNullException(nameof(type));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			// First line specify the descriptor names
			List<string> descriptorNames = new List<string>();
			descriptorNames.Add("MeshName");
			foreach(IDescriptor desc in type.featureVectors.First().Value.Descriptors)
			{
				descriptorNames.Add(desc.Name);
			}
			writer.WriteLine(string.Join(Seperator, descriptorNames.ToArray()));

			// Next lines specify the descriptor values
			List<string> descriptorValues = new List<string>();
			foreach(KeyValuePair<string, FeatureVector> vector in type.featureVectors)
			{
				descriptorValues.Clear();
				descriptorValues.Add(vector.Key);
				foreach(IDescriptor desc in vector.Value.Descriptors)
				{
					if(desc is ElemDescriptor)
					{
						ElemDescriptor elem = (ElemDescriptor)desc;
						descriptorValues.Add(elem.Value.ToString());
					}
					else if(desc is HistDescriptor)
					{
						HistDescriptor hist = (HistDescriptor)desc;
						string[] histValues = new string[12];
						histValues[0] =	hist.Offset.ToString();
						histValues[1] = hist.BinSize.ToString();
						for(int i = 2; i < 12; i++)
						{
							histValues[i] = hist.BinValues[i].ToString();
						}
						descriptorValues.Add(string.Join(HistSeperator, histValues));
					}
				}
				writer.WriteLine(string.Join(Seperator, descriptorValues.ToArray()));
			}

			// Finally make sure that all the data is written.
			writer.Flush();
		}

		public Task WriteFileAsync(FeatureManager records, string location)
		{
			return Task.Run(() => WriteFile(records, location));
		}

		public Task WriteFileAsync(FeatureManager records, StreamWriter writer)
		{
			return Task.Run(() => WriteFile(records, writer));
		}

		#endregion

	}
}
