using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using HNSW.Net;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Query;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for performing Approximate Nearest Neighbour (ANN) searches
	/// </summary>
	public class ANN
	{
		/// <summary>
		/// The world to which a query vector will be compared.
		/// </summary>
		private readonly SmallWorld<NamedFeatureVector, double> world;

		/// <summary>
		/// Initializes the ANN class, given all featuresvectors to which a query will be compared.
		/// </summary>
		/// <param name="database">All featurevectors of its world.</param>
		public ANN(IEnumerable<NamedFeatureVector> database)
		{
			IReadOnlyList<NamedFeatureVector> vectors = database.ToList().AsReadOnly();
			world = new SmallWorld<NamedFeatureVector, double>(ANNDistance);

			var parameters = new SmallWorld<NamedFeatureVector, double>.Parameters
			{
				EnableDistanceCacheForConstruction = true
			};

			using (var listener = new MetricsEventListener(EventSources.GraphBuildEventSource.Instance))
			{
				world.BuildGraph(vectors, new Random(), parameters);
			}
		}

		/// <summary>
		/// Perform ANN search on the class' world, given a vector, returning the (approximate) k-best results.
		/// </summary>
		/// <param name="queryVector">The query vector</param>
		/// <param name="kBest">The K Value</param>
		/// <returns>The k-best result</returns>
		public QueryResult RunANNQuery(NamedFeatureVector queryVector, int kBest)
		{
			if (queryVector == null)
				throw new ArgumentNullException();

			QueryResult queryresult = new QueryResult(queryVector.Name);

			IList<SmallWorld<NamedFeatureVector, double>.KNNSearchResult> kBestResults = world.KNNSearch(queryVector, kBest);

			foreach(SmallWorld<NamedFeatureVector, double>.KNNSearchResult queryitem in kBestResults)
			{
				queryresult.AddItem(new QueryItem(queryitem.Item.Name, queryitem.Distance));
			}

			return queryresult;
		}

		/// <summary>
		/// Distance function used for calculating the distances between vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns></returns>
		private static double ANNDistance(NamedFeatureVector vector1, NamedFeatureVector vector2)
		{
			if (vector1 == null || vector2 == null)
				throw new ArgumentNullException();

			return vector1.FeatureVector.Compare(vector2.FeatureVector);
		}

		/// <summary>
		/// Eventlistener used for building the world.
		/// </summary>
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
			}
		}
	}
}
