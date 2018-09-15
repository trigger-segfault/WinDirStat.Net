using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WinDirStat.Net.ViewModel.Files;
using WinDirStat.Net.Wpf.ViewModel;
//using WinDirStat.Net.Wpf.Controls.FileList;

namespace WinDirStat.Net.Wpf.Controls {
	public class SubtreePercentage : Control {
		
		public static readonly DependencyProperty FillProperty =
			Rectangle.FillProperty.AddOwner(typeof(SubtreePercentage));

		public static readonly DependencyProperty PercentageProperty =
			DependencyProperty.Register("Percentage", typeof(double), typeof(SubtreePercentage),
				new FrameworkPropertyMetadata(0d, OnPercentageChanged, CoercePercentage));
		
		private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is SubtreePercentage subtreePercentage) {
				subtreePercentage.UpdatePercentage();
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

		static SubtreePercentage() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SubtreePercentage),
				new FrameworkPropertyMetadata(typeof(SubtreePercentage)));
			SnapsToDevicePixelsProperty.OverrideMetadata(typeof(SubtreePercentage),
				new FrameworkPropertyMetadata(true));
		}

		private ColumnDefinition PART_FillColoumn;
		private ColumnDefinition PART_EmptyColoumn;

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			PART_FillColoumn = GetTemplateChild("PART_FillColoumn") as ColumnDefinition;
			PART_EmptyColoumn = GetTemplateChild("PART_EmptyColoumn") as ColumnDefinition;
			UpdatePercentage();
			UpdateTemplate();
		}

		private void UpdatePercentage() {
			if (Template != null) {
				PART_FillColoumn.Width = new GridLength(Percentage, GridUnitType.Star);
				PART_EmptyColoumn.Width = new GridLength(1d - Percentage, GridUnitType.Star);
			}
		}

		public FileItemViewModel Item {
			get => ((FileItemViewModel) DataContext);
		}

		/*public FileTreeViewItem ParentItem { get; private set; }

		public FileTreeView ParentTreeView {
			get { return ParentItem.ParentTreeView; }
		}

		protected override void OnVisualParentChanged(DependencyObject oldParent) {
			base.OnVisualParentChanged(oldParent);
			ParentItem = this.FindAncestor<FileTreeViewItem>();
		}*/

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			if (e.Property == DataContextProperty) {
				UpdateDataContext(e.OldValue as FileItemViewModel, e.NewValue as FileItemViewModel);
			}
		}

		void UpdateDataContext(FileItemViewModel oldNode, FileItemViewModel newNode) {
			if (newNode != null && Template != null) {
				UpdateTemplate();
			}
		}

		void UpdateTemplate() {
			var spacer = GetTemplateChild("PART_SpacerColumn") as ColumnDefinition;
			var filler = GetTemplateChild("PART_BarColumn") as ColumnDefinition;
			int level = Item.Level;
			double intent = CalculateIndent(level);
			spacer.Width = new GridLength(1d - intent, GridUnitType.Star);
			filler.Width = new GridLength(intent, GridUnitType.Star);
			Fill = new SolidColorBrush((Color) Item.ViewModel.Settings.GetSubtreePaletteColor(level));
		}

		private const double IndentRatio = 9d / 10d;

		internal double CalculateIndent(int level) {
			if (Item == null || level == 0)
				return 1d;
			return Math.Max(0d, Math.Min(1d, Math.Pow(IndentRatio, level)));
		}
	}
}
