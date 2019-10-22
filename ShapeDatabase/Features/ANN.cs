using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using HNSW.Net;
using Parameters = HNSW.Net.SmallWorld<float[], float>.Parameters;

namespace ShapeDatabase.Features
{
	public class ANN
	{

		private const int SampleSize = 1_000;
		private const int Dimensionality = 32;

		public static void BuildAndSave()
		{
			List<float[]> sampleVectors;

			var parameters = new Parameters();
			parameters.EnableDistanceCacheForConstruction = true;
			var world = new SmallWorld<float[], float>(CosineDistance.NonOptimized);

			Console.Write($"Generating {SampleSize} sample vectos... ");
			sampleVectors = RandomVectors(Dimensionality, SampleSize);

			Console.WriteLine("Building HNSW graph... ");
			using (var listener = new MetricsEventListener(EventSources.GraphBuildEventSource.Instance))
			{
				world.BuildGraph(sampleVectors, new Random(42), parameters);
			}
					   
			float[] query = Enumerable.Repeat(0f, 32).ToArray();
			var best20 = world.KNNSearch(query, 20);
			var best1 = best20.OrderBy(r => r.Distance).First();
			;
		}

		private static List<float[]> RandomVectors(int vectorSize, int vectorsCount)
		{
			var random = new Random(42);
			var vectors = new List<float[]>();

			for (int i = 0; i < vectorsCount; i++)
			{
				var vector = new float[vectorSize];
				for (int j = 0; j < vectorSize; j++)
				{
					vector[j] = (float)random.NextDouble();
				}

				vectors.Add(vector);
			}

			return vectors;
		}

		private class MetricsEventListener : EventListener
		{
			private readonly EventSource eventSource;

			public MetricsEventListener(EventSource eventSource)
			{
				this.eventSource = eventSource;
				this.EnableEvents(this.eventSource, EventLevel.LogAlways, EventKeywords.All, new Dictionary<string, string> { { "EventCounterIntervalSec", "1" } });
			}

			public override void Dispose()
			{
				this.DisableEvents(this.eventSource);
				base.Dispose();
			}

			protected override void OnEventWritten(EventWrittenEventArgs eventData)
			{
				var counterData = eventData.Payload?.FirstOrDefault() as IDictionary<string, object>;
				if (counterData?.Count == 0)
				{
					return;
				}

				Console.WriteLine($"[{counterData["Name"]}]: Avg={counterData["Mean"]}; SD={counterData["StandardDeviation"]}; Count={counterData["Count"]}");
			}
		}
	}
}
