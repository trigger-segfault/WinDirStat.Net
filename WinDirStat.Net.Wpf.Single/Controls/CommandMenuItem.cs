using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WinDirStat.Net.Model;
using WinDirStat.Net.Utils;
using WinDirStat.Net.ViewModel;
using WinDirStat.Net.Wpf.Commands;
using WinDirStat.Net.Wpf.Utils;
using WinDirStat.Net.Wpf.ViewModel;

namespace WinDirStat.Net.Wpf.Controls {
	public class CommandMenuItem : ImageMenuItem {
		private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			CommandMenuItem menuItem = (CommandMenuItem) d;

			if (e.OldValue is IRelayInfoCommand oldUICommand)
				oldUICommand.PropertyChanged -= menuItem.OnCommandPropertyChanged;
			if (e.NewValue is IRelayInfoCommand newUICommand)
				newUICommand.PropertyChanged += menuItem.OnCommandPropertyChanged;

			d.CoerceValue(SourceProperty);
			d.CoerceValue(HeaderProperty);
			d.CoerceValue(InputGestureTextProperty);
		}

		private void OnCommandPropertyChanged(object sender, PropertyChangedEventArgs e) {
			CoerceValue(SourceProperty);
			CoerceValue(HeaderProperty);
			CoerceValue(InputGestureTextProperty);
		}

		// Set the header to the command text if no header has been explicitly specified
		private static object CoerceHeader(DependencyObject d, object value) {
			CommandMenuItem menuItem = (CommandMenuItem) d;
			IRelayInfoCommand uiCommand;

			// If no header has been set, use the command's text
			bool unset = menuItem.IsValueUnsetAndNull(HeaderProperty, value);
			if (menuItem.IsValueUnsetAndNull(HeaderProperty, value)) {
				uiCommand = menuItem.Command as IRelayInfoCommand;
				if (uiCommand != null) {
					value = uiCommand.Text;
				}
				return value;
			}

			// If the header had been set to a UICommand by the ItemsControl, replace it with the command's text
			uiCommand = value as IRelayInfoCommand;

			if (uiCommand != null) {
				// The header is equal to the command.
				// If this MenuItem was generated for the command, then go ahead and overwrite the header
				// since the generator automatically set the header.
				ItemsControl parent = ItemsControl.ItemsControlFromItemContainer(menuItem);
				if (parent != null) {
					object originalItem = parent.ItemContainerGenerator.ItemFromContainer(menuItem);
					if (originalItem == value)
						return uiCommand.Text;
				}
			}

			return value;
		}


		// Set the icon to the command text if no icon has been explicitly specified
		private static object CoerceSource(DependencyObject d, object value) {
			CommandMenuItem menuItem = (CommandMenuItem) d;

			// If no icon has been set, use the command's text
			if (menuItem.IsValueUnsetAndNull(SourceProperty, value) &&
				menuItem.Command is IRelayInfoCommand uiCommand)
			{
				value = uiCommand.Icon;
			}

			return value;
		}

		// Gets the input gesture text from the command text if it hasn't been explicitly specified
		private static object CoerceInputGestureText(DependencyObject d, object value) {
			MenuItem menuItem = (MenuItem) d;

			if (menuItem.IsValueUnsetAndNull(InputGestureTextProperty, value) &&
				menuItem.Command is IRelayInfoCommand uiCommand && uiCommand.InputGesture != null)
			{
				return uiCommand.InputGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
			}

			return value;
		}

		static CommandMenuItem() {
			CommandProperty.AddOwner(typeof(CommandMenuItem),
				new FrameworkPropertyMetadata(OnCommandChanged));
			HeaderProperty.AddOwner(typeof(CommandMenuItem),
				new FrameworkPropertyMetadata(null, CoerceHeader));
			SourceProperty.AddOwner(typeof(CommandMenuItem),
				new FrameworkPropertyMetadata(null, CoerceSource));
			InputGestureTextProperty.AddOwner(typeof(CommandMenuItem),
				new FrameworkPropertyMetadata(null, CoerceInputGestureText));
		}

	}
}
