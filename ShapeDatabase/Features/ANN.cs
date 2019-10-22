using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Accord.Collections;
using Accord.MachineLearning;
using Accord.Math.Distances;
using HNSW.Net;

namespace ShapeDatabase.Features
{
	public class ANN
	{

		public void Test()
		{
			/*
			;
			var parameters = new SmallWorld<float[], float>.Parameters()
			{
				M = 15,
				LevelLambda = 1 / Math.Log(15)
			};

			Func<float[], float[], float> hoi = (x, y) => 1;

			float[] vectors = new float[] { 0, 3, 4 };
			var graph = new SmallWorld<float[], float>(hoi, new generator(), parameters);
			graph.BuildGraph(vectors, new Random(42), parameters);

			SmallWorld<float[], float> graph = GetGraph();

			float[] query = Enumerable.Repeat(1f, 100).ToArray();
			var best20 = graph.KNNSearch(query, 20);
			var best1 = best20.OrderBy(r => r.Distance).First();

			;
		}

		public class generator : IProvideRandomValues
		{
			public bool IsThreadSafe { get; } = true;

			public int Next(int minValue, int maxValue)
			{
				return 1;
			}
			public float NextFloat()
			{
				return 1;
			}
			public void NextFloats(Span<float> buffer)
			{
			}

		*/
		}
	}
}
