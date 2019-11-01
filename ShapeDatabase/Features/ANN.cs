using System;
using System.Collections;
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
using ShapeDatabase.Util;

namespace ShapeDatabase.Features {

	/// <summary>
	/// Class for performing Approximate Nearest Neighbour (ANN) searches
	/// </summary>
	public class ANN {

		#region --- Properties ---

		/// <summary>
		/// The world to which a query vector will be compared.
		/// </summary>
		private readonly SmallWorld<NamedFeatureVector, double> world;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initializes the ANN class, given all featuresvectors to which
		/// a query will be compared.
		/// </summary>
		/// <param name="database">All featurevectors from the database.</param>
		public ANN(IDictionary<string, FeatureVector> database)
			:this(database.Select(x => new NamedFeatureVector(x.Key, x.Value))) { }

		/// <summary>
		/// Initializes the ANN class, given all featuresvectors to which
		/// a query will be compared.
		/// </summary>
		/// <param name="database">All featurevectors of its world.</param>
		public ANN(IEnumerable<NamedFeatureVector> database) {
			IReadOnlyList<NamedFeatureVector> vectors = database.ToList().AsReadOnly();
			world = new SmallWorld<NamedFeatureVector, double>(ANNDistance);

			SmallWorld<NamedFeatureVector, double>.Parameters parameters =
				new SmallWorld<NamedFeatureVector, double>.Parameters
			{
				EnableDistanceCacheForConstruction = true
			};

			using (MetricsEventListener listener =
				new MetricsEventListener(EventSources.GraphBuildEventSource.Instance)) {
				world.BuildGraph(vectors, RandomUtil.ThreadSafeRandom, parameters);
			}
		}

		#endregion

		#region --- Methods ---

		/// <summary>
		/// Perform ANN search on the class' world, given a vector, returning the (approximate) k-best results.
		/// </summary>
		/// <param name="name">The name of the querried object.</param>
		/// <param name="vector">The featurevector for comparison between vectors.</param>
		/// <param name="kBest">How many results should be returned.</param>
		/// <returns>The best results for the specified item in a
		/// <see cref="QueryResult"/> object.</returns>
		public QueryResult RunANNQuery(string name, FeatureVector vector, int kBest) {
			return RunANNQuery(new NamedFeatureVector(name, vector), kBest);
		}

		/// <summary>
		/// Perform ANN search on the class' world, given a vector, returning the (approximate) k-best results.
		/// </summary>
		/// <param name="queryVector">The query vector</param>
		/// <param name="kBest">The K Value</param>
		/// <returns>The k-best result</returns>
		public QueryResult RunANNQuery(NamedFeatureVector queryVector, int kBest) {

			QueryResult queryresult = new QueryResult(queryVector.Name);

			IList<SmallWorld<NamedFeatureVector, double>.KNNSearchResult> kBestResults = world.KNNSearch(queryVector, kBest);

			foreach (SmallWorld<NamedFeatureVector, double>.KNNSearchResult queryitem in kBestResults) {
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
		private static double ANNDistance(NamedFeatureVector vector1, NamedFeatureVector vector2) {
			return vector1.FeatureVector.Compare(vector2.FeatureVector);
		}

		#endregion

		/// <summary>
		/// Eventlistener used for building the world.
		/// </summary>
		private class MetricsEventListener : EventListener {
			private readonly EventSource eventSource;

			public MetricsEventListener(EventSource eventSource) {
				this.eventSource = eventSource;
				EnableEvents(this.eventSource, EventLevel.LogAlways, EventKeywords.All, new Dictionary<string, string> { { "EventCounterIntervalSec", "1" } });
			}

			public override void Dispose() {
				DisableEvents(this.eventSource);
				base.Dispose();
			}

			protected override void OnEventWritten(EventWrittenEventArgs eventData) {
				IDictionary<string, object> counterData = eventData.Payload?.FirstOrDefault() as IDictionary<string, object>;
				if (counterData?.Count == 0) {
					return;
				}
			}
		}
	}
}
