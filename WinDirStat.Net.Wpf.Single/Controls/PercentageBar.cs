using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WinDirStat.Net.Wpf.Controls {
	public class PercentageBar : Control {

		public static readonly DependencyProperty FillProperty =
			Rectangle.FillProperty.AddOwner(typeof(PercentageBar),
				new FrameworkPropertyMetadata(Brushes.LimeGreen));

		public static readonly DependencyProperty PercentageProperty =
			DependencyProperty.Register("Percentage", typeof(double), typeof(PercentageBar),
				new FrameworkPropertyMetadata(0d, OnPercentageChanged, CoercePercentage));

		private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is PercentageBar percentageBar) {
				percentageBar.UpdatePercentage();
			}
		}

		private static object CoercePercentage(DependencyObject d, object baseValue) {
			return Math.Max(0d, Math.Min(1d, (double) baseValue));
		}

		public Brush Fill {
			get => (Brush) GetValue(FillProperty);
			set => SetValue(FillProperty, value);
		}

		public double Percentage {
			get => (double) GetValue(PercentageProperty);
			set => SetValue(PercentageProperty, value);
		}

		static PercentageBar() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PercentageBar),
				new FrameworkPropertyMetadata(typeof(PercentageBar)));
			SnapsToDevicePixelsProperty.OverrideMetadata(typeof(PercentageBar),
				new FrameworkPropertyMetadata(true));
		}

		private ColumnDefinition PART_FillColoumn;
		private ColumnDefinition PART_EmptyColoumn;

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			PART_FillColoumn = GetTemplateChild("PART_FillColoumn") as ColumnDefinition;
			PART_EmptyColoumn = GetTemplateChild("PART_EmptyColoumn") as ColumnDefinition;
			UpdatePercentage();
		}

		private void UpdatePercentage() {
			if (Template != null) {
				PART_FillColoumn.Width = new GridLength(Percentage, GridUnitType.Star);
				PART_EmptyColoumn.Width = new GridLength(1d - Percentage, GridUnitType.Star);
			}
		}
	}
}
