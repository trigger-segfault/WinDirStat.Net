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
using WinDirStat.Net.Data.Nodes;
using WinDirStat.Net.TreeView;

namespace WinDirStat.Net.Controls {
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
		}

		private void UpdatePercentage() {
			if (Template != null) {
				PART_FillColoumn.Width = new GridLength(Percentage, GridUnitType.Star);
				PART_EmptyColoumn.Width = new GridLength(1d - Percentage, GridUnitType.Star);
			}
		}

		public FileNode Node {
			get { return DataContext as FileNode; }
		}

		public FileTreeViewItem ParentItem { get; private set; }

		public FileTreeView ParentTreeView {
			get { return ParentItem.ParentTreeView; }
		}

		protected override void OnVisualParentChanged(DependencyObject oldParent) {
			base.OnVisualParentChanged(oldParent);
			ParentItem = this.FindAncestor<FileTreeViewItem>();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			if (e.Property == DataContextProperty) {
				UpdateDataContext(e.OldValue as FileNode, e.NewValue as FileNode);
			}
		}

		void UpdateDataContext(FileNode oldNode, FileNode newNode) {
			if (newNode != null && Template != null) {
				UpdateTemplate();
			}
		}

		void UpdateTemplate() {
			var spacer = Template.FindName("spacer", this) as FrameworkElement;
			//spacer.Width = CalculateIndent();
		}

		internal double CalculateIndent() {
			int result = 19 * Node.Level;
			if (ParentTreeView.ShowRoot) {
				if (!ParentTreeView.ShowRootExpander) {
					if (ParentTreeView.Root != Node) {
						result -= 15;
					}
				}
			}
			else {
				result -= 19;
			}
			if (result < 0) {
				Debug.WriteLine("Negative indent level detected for node " + Node);
				result = 0;
			}
			return result;
		}
	}
}
