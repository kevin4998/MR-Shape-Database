using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.UI {

	public class KeyController {

		private IDictionary<Key, ICollection<Action>> DownKeybinds	{ get; }
		private IDictionary<Key, ICollection<Action>> UpKeybinds	{ get; }
		private IDictionary<Key, ICollection<Action>> HoldKeybinds	{ get; }

		private HashSet<Key> activeKeys;
		private HashSet<Key> onKeys;

		public KeyController() {
			DownKeybinds	= new Dictionary<Key, ICollection<Action>>();
			UpKeybinds		= new Dictionary<Key, ICollection<Action>>();
			HoldKeybinds	= new Dictionary<Key, ICollection<Action>>();

			activeKeys	= new HashSet<Key>();
			onKeys		= new HashSet<Key>();
		}

		public void OnKeyPress(KeyboardState input) {
			if (!input.IsAnyKeyDown)
				return;

			foreach (Key key in activeKeys)
				if (input.IsKeyUp(key))
					if (UpKeybinds.TryGetValue(key, out ICollection<Action> actions))
						foreach (Action action in actions)
							action();

			foreach (Key key in onKeys)
				if (input.IsKeyDown(key)) {
					if (activeKeys.Contains(key)) { // Continues press
						if (HoldKeybinds.TryGetValue(key, out ICollection<Action> actions))
							foreach (Action action in actions)
								action();
					} else {
						activeKeys.Add(key);        // First time press
						if (DownKeybinds.TryGetValue(key, out ICollection<Action> actions))
							foreach (Action action in actions)
								action();
					}
				}


			activeKeys.RemoveWhere(key => input.IsKeyUp(key));
		}

		public void RegisterDown(Key button, Action action) {
			DicAdd(DownKeybinds, button, action);
			onKeys.Add(button);
		}

		public void RegisterUp(Key button, Action action) {
			DicAdd(UpKeybinds, button, action);
		}

		public void RegisterHold(Key button, Action action) {
			DicAdd(HoldKeybinds, button, action);
			onKeys.Add(button);
		}

		private void DicAdd<K, V>(IDictionary<K, ICollection<V>> dic, K key, V value) {
			if (!dic.TryGetValue(key, out ICollection<V> col))
				col = new List<V>();

			if (!col.Contains(value))
				col.Add(value);

			dic.Add(key, col);
		}

	}
}
