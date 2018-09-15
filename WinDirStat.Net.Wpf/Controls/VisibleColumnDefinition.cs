using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WinDirStat.Net.Wpf.Controls {
	public class VisibleColumnDefinition : ColumnDefinition {

		public static readonly DependencyProperty VisibleProperty =
			DependencyProperty.Register("Visible", typeof(bool), typeof(VisibleColumnDefinition),
				new PropertyMetadata(true, OnVisibleChanged));

		private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			VisibleColumnDefinition col = (VisibleColumnDefinition) d;

			if (col.Visible) {
				col.visibleChanging = true;
				col.Width    = col.storedWidth;
				col.MinWidth = col.storedMinWidth;
				col.MaxWidth = col.storedMaxWidth;
				col.visibleChanging = false;
			}
			else {
				col.visibleChanging = true;
				col.storedWidth    = col.Width;
				col.storedMinWidth = col.MinWidth;
				col.storedMaxWidth = col.MaxWidth;
				col.MinWidth = 0d;
				col.Width    = new GridLength(0);
				col.MaxWidth = 0d;
				col.visibleChanging = false;
			}
		}

		private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			VisibleColumnDefinition col = (VisibleColumnDefinition) d;
			if (!col.Visible && !col.visibleChanging) {
				col.storedWidth = (GridLength) e.NewValue;
				col.Width = new GridLength(0);
			}
		}

		private static void OnMinWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			VisibleColumnDefinition col = (VisibleColumnDefinition) d;
			if (!col.Visible && !col.visibleChanging) {
				col.storedMinWidth = (double) e.NewValue;
				col.MinWidth = 0;
			}
		}

		private static void OnMaxWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			VisibleColumnDefinition col = (VisibleColumnDefinition) d;
			if (!col.Visible && !col.visibleChanging) {
				col.storedMaxWidth = (double) e.NewValue;
				col.MaxWidth = 0;
			}
		}

		static VisibleColumnDefinition() {
			WidthProperty.AddOwner(typeof(VisibleColumnDefinition),
				new FrameworkPropertyMetadata(WidthProperty.DefaultMetadata.DefaultValue, OnWidthChanged));
			MinWidthProperty.AddOwner(typeof(VisibleColumnDefinition),
				new FrameworkPropertyMetadata(MinWidthProperty.DefaultMetadata.DefaultValue, OnMinWidthChanged));
			MaxWidthProperty.AddOwner(typeof(VisibleColumnDefinition),
				new FrameworkPropertyMetadata(MaxWidthProperty.DefaultMetadata.DefaultValue, OnMaxWidthChanged));
		}

		public bool Visible {
			get => (bool) GetValue(VisibleProperty);
			set => SetValue(VisibleProperty, value);
		}

		private bool visibleChanging = false;
		private GridLength storedWidth = (GridLength) WidthProperty.DefaultMetadata.DefaultValue;
		private double storedMinWidth = (double) MinWidthProperty.DefaultMetadata.DefaultValue;
		private double storedMaxWidth = (double) MaxWidthProperty.DefaultMetadata.DefaultValue;
	}
}
