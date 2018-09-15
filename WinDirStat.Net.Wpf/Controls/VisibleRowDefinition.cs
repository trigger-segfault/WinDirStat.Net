using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WinDirStat.Net.Wpf.Controls {
	public class VisibleRowDefinition : RowDefinition {

		public static readonly DependencyProperty VisibleProperty =
			DependencyProperty.Register("Visible", typeof(bool), typeof(VisibleRowDefinition),
				new PropertyMetadata(true, OnVisibleChanged));

		private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			VisibleRowDefinition row = (VisibleRowDefinition) d;

			if (row.Visible) {
				row.visibleChanging = true;
				row.Height    = row.storedHeight;
				row.MinHeight = row.storedMinHeight;
				row.MaxHeight = row.storedMaxHeight;
				row.visibleChanging = false;
			}
			else {
				row.visibleChanging = true;
				row.storedHeight    = row.Height;
				row.storedMinHeight = row.MinHeight;
				row.storedMaxHeight = row.MaxHeight;
				row.MinHeight = 0d;
				row.Height    = new GridLength(0);
				row.MaxHeight = 0d;
				row.visibleChanging = false;
			}
		}

		private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			VisibleRowDefinition row = (VisibleRowDefinition) d;
			if (!row.Visible && !row.visibleChanging) {
				row.storedHeight = (GridLength) e.NewValue;
				row.Height = new GridLength(0);
			}
		}

		private static void OnMinHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			VisibleRowDefinition row = (VisibleRowDefinition) d;
			if (!row.Visible && !row.visibleChanging) {
				row.storedMinHeight = (double) e.NewValue;
				row.MinHeight = 0;
			}
		}

		private static void OnMaxHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			VisibleRowDefinition row = (VisibleRowDefinition) d;
			if (!row.Visible && !row.visibleChanging) {
				row.storedMaxHeight = (double) e.NewValue;
				row.MaxHeight = 0;
			}
		}

		static VisibleRowDefinition() {
			HeightProperty.AddOwner(typeof(VisibleRowDefinition),
				new FrameworkPropertyMetadata(HeightProperty.DefaultMetadata.DefaultValue, OnHeightChanged));
			MinHeightProperty.AddOwner(typeof(VisibleRowDefinition),
				new FrameworkPropertyMetadata(MinHeightProperty.DefaultMetadata.DefaultValue, OnMinHeightChanged));
			MaxHeightProperty.AddOwner(typeof(VisibleRowDefinition),
				new FrameworkPropertyMetadata(MaxHeightProperty.DefaultMetadata.DefaultValue, OnMaxHeightChanged));
		}

		public bool Visible {
			get => (bool) GetValue(VisibleProperty);
			set => SetValue(VisibleProperty, value);
		}

		private bool visibleChanging = false;
		private GridLength storedHeight = (GridLength) HeightProperty.DefaultMetadata.DefaultValue;
		private double storedMinHeight = (double) MinHeightProperty.DefaultMetadata.DefaultValue;
		private double storedMaxHeight = (double) MaxHeightProperty.DefaultMetadata.DefaultValue;
	}
}
