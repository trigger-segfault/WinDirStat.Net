using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model {
	public abstract class ObservableObject : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsWatched {
			get => PropertyChanged != null;
		}

		protected void RaisePropertyChanged(PropertyChangedEventArgs e) {
			PropertyChanged?.Invoke(this, e);
		}

		protected void RaisePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		protected void RaisePropertiesChanged(params string[] names) {
			for (int i = 0; i < names.Length; i++)
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(names[i]));
		}

		protected void AutoRaisePropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	public abstract class ObservableCollectionObject : ObservableObject, INotifyCollectionChanged {
		private bool isRaisingEvent;

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public new bool IsWatched {
			get => base.IsWatched || CollectionChanged != null;
		}

		protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) {
			Debug.Assert(!isRaisingEvent);
			isRaisingEvent = true;
			try {
				CollectionChanged?.Invoke(this, e);
			}
			finally {
				isRaisingEvent = false;
			}
		}

		protected void ThrowOnReentrancy() {
			if (isRaisingEvent)
				throw new InvalidOperationException($"Collection cannot be changed during " +
					$"{nameof(CollectionChanged)} event!");
		}
	}
}
