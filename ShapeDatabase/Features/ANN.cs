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
		private const int TestSize = 10 * SampleSize;
		private const int Dimensionality = 32;
		private const string VectorsPathSuffix = "vectors.hnsw";
		private const string GraphPathSuffix = "graph.hnsw";

		public static void BuildAndSave()
		{
			Stopwatch clock;
			List<float[]> sampleVectors;

			var parameters = new Parameters();
			parameters.EnableDistanceCacheForConstruction = true;
			var world = new SmallWorld<float[], float>(CosineDistance.NonOptimized);

			Console.Write($"Generating {SampleSize} sample vectos... ");
			clock = Stopwatch.StartNew();
			sampleVectors = RandomVectors(Dimensionality, SampleSize);
			Console.WriteLine($"Done in {clock.ElapsedMilliseconds} ms.");

			Console.WriteLine("Building HNSW graph... ");
			using (var listener = new MetricsEventListener(EventSources.GraphBuildEventSource.Instance))
			{
				clock = Stopwatch.StartNew();
				world.BuildGraph(sampleVectors, new Random(42), parameters);
				Console.WriteLine($"Done in {clock.ElapsedMilliseconds} ms.");
			}

			//Console.Write($"Saving HNSW graph to '${Path.Combine(Directory.GetCurrentDirectory(), pathPrefix)}'... ");
			clock = Stopwatch.StartNew();
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream sampleVectorsStream = new MemoryStream();
			formatter.Serialize(sampleVectorsStream, sampleVectors);
			//File.WriteAllBytes($"{pathPrefix}.{VectorsPathSuffix}", sampleVectorsStream.ToArray());
			//File.WriteAllBytes($"{pathPrefix}.{GraphPathSuffix}", world.SerializeGraph());
			Console.WriteLine($"Done in {clock.ElapsedMilliseconds} ms.");


			float[] query = Enumerable.Repeat(1f, 32).ToArray();
			var best20 = world.KNNSearch(query, 20);
			var best1 = best20.OrderBy(r => r.Distance).First();
			;

		}

		private static void LoadAndSearch(string pathPrefix)
		{
			Stopwatch clock;
			var world = new SmallWorld<float[], float>(CosineDistance.NonOptimized);

			Console.Write("Loading HNSW graph... ");
			clock = Stopwatch.StartNew();
			BinaryFormatter formatter = new BinaryFormatter();
			var sampleVectors = (List<float[]>)formatter.Deserialize(new MemoryStream(File.ReadAllBytes($"{pathPrefix}.{VectorsPathSuffix}")));
			world.DeserializeGraph(sampleVectors, File.ReadAllBytes($"{pathPrefix}.{GraphPathSuffix}"));
			Console.WriteLine($"Done in {clock.ElapsedMilliseconds} ms.");

			Console.Write($"Generating {TestSize} test vectos... ");
			clock = Stopwatch.StartNew();
			var vectors = RandomVectors(Dimensionality, TestSize);
			Console.WriteLine($"Done in {clock.ElapsedMilliseconds} ms.");

			Console.WriteLine("Running search agains the graph... ");
			using (var listener = new MetricsEventListener(EventSources.GraphSearchEventSource.Instance))
			{
				clock = Stopwatch.StartNew();
				Parallel.ForEach(vectors, (vector) =>
				{
					world.KNNSearch(vector, 10);
				});
				Console.WriteLine($"Done in {clock.ElapsedMilliseconds} ms.");
			}
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
