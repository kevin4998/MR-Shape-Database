using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Features;
using ShapeDatabase.Util.Attributes;

namespace ShapeDatabase {

	/// <summary>
	/// A temporary class to read in files and convert them to the permanent
	/// <see cref="Settings"/> and <see cref="WeightManager"/> classes.
	/// </summary>
	public class TempSettings {

		#region --- Properties ---

		private readonly IDictionary<string, double> weights
			= new SortedDictionary<string, double>();
		private readonly IDictionary<string, int>    variables
			= new SortedDictionary<string, int>();
		private readonly IDictionary<string, bool>   flow
			= new SortedDictionary<string, bool>();

		/// <summary>
		/// All the weights for each Descriptor during this program.
		/// </summary>
		public IEnumerable<(string, double)> Weights {
			get {
				foreach(KeyValuePair<string, double> pair in weights)
					yield return (pair.Key, pair.Value);
			}
		}
		/// <summary>
		/// All the variables which contain amounts during the application.
		/// </summary>
		public IEnumerable<(string, int)> Variables {
			get {
				foreach (KeyValuePair<string, int> pair in variables)
					yield return (pair.Key, pair.Value);
			}

		}
		/// <summary>
		/// All the variables which control flow during the application.
		/// </summary>
		public IEnumerable<(string, bool)> Flow {
			get {
				foreach (KeyValuePair<string, bool> pair in flow)
					yield return (pair.Key, pair.Value);
			}
		}

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Creates a new class to temporary contain variables from the
		/// <see cref="Settings"/> file.
		/// </summary>
		public TempSettings() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Reflection code to get all the setters of the specified type so it can
		/// be read to or written from an external file.
		/// </summary>
		/// <typeparam name="T">The type of setters to retrieve from
		/// <see cref="Settings"/> </typeparam>
		/// <returns>All setters of the specified type in <see cref="Settings"/></returns>
		private (string, Action<T>)[] SettingSetters<T>() {
			Type clazz = typeof(Settings);
			PropertyInfo[] properties = clazz.GetProperties(
				BindingFlags.Public |
				BindingFlags.Static |
				BindingFlags.SetProperty
			);

			IList<(string, Action<T>)> variables = new List<(string, Action<T>)>();
			foreach (PropertyInfo property in properties) {
				if (!property.CanWrite) continue;
				if (property.PropertyType != typeof(T)) continue;
				if (Attribute.IsDefined(property, typeof(IgnoreAttribute))) continue;

				try { 
					Delegate del = Delegate.CreateDelegate(typeof(Action<T>),
														   property.GetSetMethod());
					if (del is Action<T> action)
						variables.Add((property.Name, action));
				} catch (ArgumentException ex) {
					// It appears that our property is not of the right type, somehow.
					// So let's ignore it in this case and continue with the safe ones.
				}
			}

			return variables.ToArray();
		}

		/// <summary>
		/// Reflection code to get all the getters of the specified type so it can
		/// be read to or written from an external file.
		/// </summary>
		/// <typeparam name="T">The type of getters to retrieve from
		/// <see cref="Settings"/></typeparam>
		/// <returns>All getters of the specified type in <see cref="Settings"/></returns>
		private (string, Func<T>)[] SettingGetters<T>() {
			Type clazz = typeof(Settings);
			PropertyInfo[] properties = clazz.GetProperties(
				BindingFlags.Public |
				BindingFlags.Static |
				BindingFlags.GetProperty
			);

			IList<(string, Func<T>)> variables = new List<(string, Func<T>)>();
			foreach (PropertyInfo property in properties) {
				if (!property.CanRead) continue;
				if (property.PropertyType != typeof(T)) continue;
				if (Attribute.IsDefined(property, typeof(IgnoreAttribute))) continue;

				try {
					Delegate del = Delegate.CreateDelegate(typeof(Func<T>),
														   property.GetGetMethod());
					if (del is Func<T> action)
						variables.Add((property.Name, action));
				} catch (ArgumentException ex) {
					// It appears that our property is not of the right type, somehow.
					// So let's ignore it in this case and continue with the safe ones.
				}
			}

			return variables.ToArray();
		}


		/// <summary>
		/// Describes a new weight for the specified Descriptor.
		/// </summary>
		/// <param name="name">The name of the descriptor who will get a weight.</param>
		/// <param name="weight">The given weight of this descriptor in comparison
		/// to all others.</param>
		public void AddWeight(string name, double weight) => weights.Add(name, weight);
		/// <summary>
		/// Descibres a new variable which will determine a setting for one
		/// of the objects.
		/// </summary>
		/// <param name="name">The name of the new variable to pay attention to.</param>
		/// <param name="var">The value which should hold throughout the application.
		/// </param>
		public void AddVariable(string name, int var) => variables.Add(name, var);
		/// <summary>
		/// Describes a variable which controls the flow of the program.
		/// </summary>
		/// <param name="name">The name of the new variable to pay attention to.</param>
		/// <param name="var">The value which should hold throughout the application.
		/// </param>
		public void AddFlow(string name, bool var) => flow.Add(name, var);


		/// <summary>
		/// Convert the temporary file into the permament Settings and Weights objects.
		/// </summary>
		public void Finalise() {
			WeightManager weightManager = Settings.Weights;
			foreach (KeyValuePair<string, double> pair in weights)
				weightManager[pair.Key] = pair.Value;

			(string, Action<int>)[] sVariables = SettingSetters<int>();
			foreach((string name, Action<int> setter) in sVariables)
				if (variables.TryGetValue(name, out int value))
					setter(value);

			(string, Action<bool>)[] sFlow = SettingSetters<bool>();
			foreach ((string name, Action<bool> setter) in sFlow)
				if (flow.TryGetValue(name, out bool value))
					setter(value);
		}

		/// <summary>
		/// Read in all the values from Settings and Weights so it can be written
		/// to a file.
		/// </summary>
		public void Initialise() {
			WeightManager weightManager = Settings.Weights;
			foreach ((string name, double weight) in weightManager.Weights)
				weights[name] = weight;

			(string, Func<int>)[] sVariables = SettingGetters<int>();
			foreach((string name, Func<int> value) in sVariables)
				variables[name] = value();

			(string, Func<bool>)[] sFlow = SettingGetters<bool>();
			foreach ((string name, Func<bool> value) in sFlow)
				flow[name] = value();
		}

		#endregion

	}

}
