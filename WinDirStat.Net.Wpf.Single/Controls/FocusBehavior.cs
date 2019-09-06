using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace WinDirStat.Net.Wpf.Controls {
	/// <summary>A behavior for binding to readonly focus of the element.</summary>
	public class FocusBehavior : Behavior<UIElement> {

		#region Dependency Properties

		/// <summary>Gets the property for standard control focus.</summary>
		public static readonly DependencyProperty IsFocusedProperty =
			DependencyProperty.Register(
				"IsFocused", typeof(bool), typeof(FocusBehavior),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		/// <summary>Gets the property for keyboard control focus.</summary>
		public static readonly DependencyProperty IsKeyboardFocusedProperty =
			DependencyProperty.Register(
				"IsKeyboardFocused", typeof(bool), typeof(FocusBehavior),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		/// <summary>Gets if the control has standard focus.</summary>
		public bool IsFocused {
			get => (bool) GetValue(IsFocusedProperty);
			set => SetValue(IsFocusedProperty, value);
		}

		/// <summary>Gets if the control has keyboard focus.</summary>
		public bool IsKeyboardFocused {
			get => (bool) GetValue(IsKeyboardFocusedProperty);
			set => SetValue(IsKeyboardFocusedProperty, value);
		}

		#endregion

		#region Override Methods

		/// <summary>Attaches the focus events.</summary>
		protected override void OnAttached() {
			base.OnAttached();
			AssociatedObject.GotFocus += OnGotFocus;
			AssociatedObject.LostFocus += OnLostFocus;
			AssociatedObject.GotKeyboardFocus += OnGotKeyboardFocus;
			AssociatedObject.LostKeyboardFocus += OnLostKeyboardFocus;
		}

		/// <summary>Detaches the focus events.</summary>
		protected override void OnDetaching() {
			base.OnDetaching();
			if (AssociatedObject != null) {
				AssociatedObject.GotFocus -= OnGotFocus;
				AssociatedObject.LostFocus -= OnLostFocus;
				AssociatedObject.GotKeyboardFocus -= OnGotKeyboardFocus;
				AssociatedObject.LostKeyboardFocus -= OnLostKeyboardFocus;
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>Called when the control receives standard focus.</summary>
		private void OnGotFocus(object sender, RoutedEventArgs e) {
			Debug.WriteLine("GotFocus");
			IsFocused = true;
		}
		/// <summary>Called when the control loses standard focus.</summary>
		private void OnLostFocus(object sender, RoutedEventArgs e) {
			Debug.WriteLine("LostFocus");
			IsFocused = false;
		}

		/// <summary>Called when the control receives keyboard focus.</summary>
		private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			Debug.WriteLine("GotKeyboardFocus");
			IsKeyboardFocused = true;
		}
		/// <summary>Called when the control loses keyboard focus.</summary>
		private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			Debug.WriteLine("LostKeyboardFocus");
			IsKeyboardFocused = false;
		}

		#endregion
	}
}
