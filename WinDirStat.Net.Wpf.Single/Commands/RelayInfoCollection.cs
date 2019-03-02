using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinDirStat.Net.ViewModel.Commands;

namespace WinDirStat.Net.Wpf.Commands {
	/// <summary>
	/// A collection for defining relay command infos in XAML.
	/// </summary>
	/// <remarks>
	/// Why is this not using the correct generic type you ask? Becaues the designer is shit.
	/// </remarks>
	public class RelayInfoCollection : ObservableCollection<object> {

		#region Fields

		private readonly Dictionary<string, PropertyInfo> commands = new Dictionary<string, PropertyInfo>();
		private Type viewModelType = typeof(object);
		private List<object> oldInfos = new List<object>();
		private HashSet<string> oldNames = new HashSet<string>();

		#endregion

		#region Constructors

		public RelayInfoCollection() {
			CollectionChanged += OnCollectionChanged;
		}

		#endregion

		#region Accessors

		public IRelayInfoCommand Get(object viewModel, string commandName) {
			if (commands.TryGetValue(commandName, out PropertyInfo prop)) {
				return (IRelayInfoCommand) prop.GetValue(viewModel);
			}
			return null;
		}

		#endregion

		#region Properties

		public Type ViewModelType {
			get => viewModelType;
			set {
				if (viewModelType == null)
					throw new ArgumentNullException(nameof(ViewModelType));
				if (viewModelType != value) {
					viewModelType = value;

					// Recollect all valid command names
					commands.Clear();
					foreach (PropertyInfo prop in viewModelType.GetProperties()) {
						if (typeof(ICommand).IsAssignableFrom(prop.PropertyType)) {
							commands.Add(prop.Name, prop);
						}
					}

					Validate(this.Cast<RelayInfo>());
				}
			}
		}

		#endregion

		private void Validate(IEnumerable<RelayInfo> infos) {
			foreach (RelayInfo info in infos)
				Validate(info);
		}
		private void Validate(RelayInfo info) {
			if (!string.IsNullOrEmpty(info.Name) && !commands.ContainsKey(info.Name))
				throw new ArgumentException($"Command with name {info.Name} does not exist!");
		}

		private void Unhook(IEnumerable<RelayInfo> infos) {
			foreach (RelayInfo info in infos)
				Unhook(info);
		}
		private void Unhook(RelayInfo info) {
			info.PropertyChanged -= OnInfoPropertyChanged;
		}

		private void Hook(IEnumerable<RelayInfo> infos) {
			foreach (RelayInfo info in infos)
				Hook(info);
		}
		private void Hook(RelayInfo info) {
			info.PropertyChanged += OnInfoPropertyChanged;
		}

		#region Event Handlers

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			switch (e.Action) {
			case NotifyCollectionChangedAction.Reset:
				Unhook(oldInfos.Cast<RelayInfo>());
				Hook(this.Cast<RelayInfo>());
				Validate(this.Cast<RelayInfo>());
				break;
			case NotifyCollectionChangedAction.Replace:
				Unhook(e.OldItems.Cast<RelayInfo>());
				Hook(e.NewItems.Cast<RelayInfo>());
				Validate(e.NewItems.Cast<RelayInfo>());
				break;
			case NotifyCollectionChangedAction.Add:
				Hook(e.NewItems.Cast<RelayInfo>());
				Validate(e.NewItems.Cast<RelayInfo>());
				break;
			case NotifyCollectionChangedAction.Remove:
				Unhook(e.OldItems.Cast<RelayInfo>());
				break;
			}
			oldInfos = new List<object>(this);
			oldNames = new HashSet<string>(this.Cast<RelayInfo>().Select(i => i.Name));
		}

		private void OnInfoPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(RelayInfo.Name))
				Validate((RelayInfo) sender);
		}

		#endregion
	}
}
