using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WinDirStat.Net.Wpf.Commands {
	partial class RelayInfo {

		public static readonly DependencyProperty CollectionProperty =
			DependencyProperty.RegisterAttached("Collection", typeof(object), typeof(RelayInfo),
				new PropertyMetadata(null, OnCollectionChanged));

		private static void OnCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			Control control = (Control) d;

			if (e.OldValue == null)
				Hook(control);
			if (e.NewValue == null)
				Unhook(control);

			object dataContext = control.DataContext;
			if (dataContext != null) {
				if (e.OldValue != null)
					UnattachDataContext(control, dataContext, (RelayInfoCollection) e.OldValue);
				if (e.NewValue != null)
					AttachDataContext(control, dataContext, (RelayInfoCollection) e.NewValue);
			}
		}

		public static RelayInfoCollection GetCollection(DependencyObject d) {
			return (RelayInfoCollection) d.GetValue(CollectionProperty);
		}
		public static void SetCollection(DependencyObject d, RelayInfoCollection value) {
			d.SetValue(CollectionProperty, value);
		}

		private static void OnControlDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
			Control control = (Control) sender;

			// If not attached, do nothing
			RelayInfoCollection collection = GetCollection(control);
			if (collection == null)
				return;

			if (e.OldValue != null)
				UnattachDataContext(control, e.OldValue, collection);
			if (e.NewValue != null)
				AttachDataContext(control, e.NewValue, collection);
		}

		/*private static void OnControlLoaded(object sender, RoutedEventArgs e) {
			Control control = (Control) sender;

			// If not attached, do nothing
			RelayInfoCollection collection = GetCollection(control);
			if (collection == null)
				return;

			object dataContext = control.DataContext;
			if (dataContext != null)
				AttachDataContext(control, dataContext, collection);
		}*/

		private static void Unhook(Control control) {
			control.DataContextChanged -= OnControlDataContextChanged;
			//control.Loaded -= OnControlLoaded;
		}

		private static void Hook(Control control) {
			control.DataContextChanged += OnControlDataContextChanged;
			//control.Loaded += OnControlLoaded;
		}

		private static void UnattachDataContext(Control control, object dataContext, RelayInfoCollection collection) {
			for (int i = 0; i < control.InputBindings.Count; i++) {
				if (control.InputBindings[i] is RelayInfoCommandBinding binding) {
					control.InputBindings.RemoveAt(i--);
				}
			}
		}

		private static void AttachDataContext(Control control, object dataContext, RelayInfoCollection collection) {
			Type dataType = dataContext.GetType();
			foreach (RelayInfo info in collection) {
				PropertyInfo prop = dataType.GetProperty(info.Name);
				if (prop == null)
					throw new ArgumentNullException($"No property found with the name \"{info.Name}\"!");
				//if (!typeof(ICommand).IsAssignableFrom(prop.PropertyType))
				//	throw new ArgumentNullException($"Property \"{info.Name}\" is not a type of {nameof(ICommand)}!");
				IRelayInfoCommand command = (IRelayInfoCommand) prop.GetValue(dataContext);
				command.Info = info;
				if (command.InputGesture != null)
					control.InputBindings.Add(new RelayInfoCommandBinding(command));
			}
		}
	}
}
