using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WinDirStat.Net.SortingView {
	public class SortViewColumn : GridViewColumn {

		public static readonly DependencyProperty TextAlignmentProperty =
			TextBlock.TextAlignmentProperty.AddOwner(typeof(SortViewColumn),
				new FrameworkPropertyMetadata(TextAlignment.Left, OnTextAlignmentChanged));

		private static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is SortViewColumn column) {
				switch (column.TextAlignment) {
				case TextAlignment.Left:
					column.HeaderContainerStyle = SortViewKeys.HeaderContainerLeftAlignStyle;
					break;
				case TextAlignment.Right:
					column.HeaderContainerStyle = SortViewKeys.HeaderContainerRightAlignStyle;
					break;
				default:
					column.HeaderContainerStyle = SortViewKeys.HeaderContainerCenterAlignStyle;
					break;
				}
				if (column.cellBinding != null)
					column.RebuildCellTemplate();
			}
		}

		public TextAlignment TextAlignment {
			get => (TextAlignment) GetValue(TextAlignmentProperty);
			set => SetValue(TextAlignmentProperty, value);
		}

		public static readonly DependencyProperty DefaultSortDirectionProperty =
			DependencyProperty.Register("DefaultSortDirection", typeof(ListSortDirection), typeof(SortViewColumn),
				new FrameworkPropertyMetadata(ListSortDirection.Ascending));

		public ListSortDirection DefaultSortDirection {
			get => (ListSortDirection) GetValue(DefaultSortDirectionProperty);
			set => SetValue(DefaultSortDirectionProperty, value);
		}

		public static readonly DependencyProperty SortModeProperty =
			DependencyProperty.Register("SortMode", typeof(string), typeof(SortViewColumn),
				new FrameworkPropertyMetadata(null));

		public string SortMode {
			get => (string) GetValue(SortModeProperty);
			set => SetValue(SortModeProperty, value);
		}

		public ListSortDirection? SortDirection {
			get => SortListView.GetColumnSortDirection(this);
			//internal set => SetValue(SortDirectionPropertyKey, value);
		}

		public static readonly DependencyProperty CellDataTemplateProperty =
			DependencyProperty.Register("CellDataTemplate", typeof(DataTemplate), typeof(SortViewColumn),
				new FrameworkPropertyMetadata(OnCellDataTemplateChanged));

		public DataTemplate CellDataTemplate {
			get => (DataTemplate) GetValue(CellDataTemplateProperty);
			set => SetValue(CellDataTemplateProperty, value);
		}

		static SortViewColumn() {
			//HeaderContainerStyleProperty.OverrideMetadata(typeof(SortViewColumn),
			//	new FrameworkPropertyMetadata(Application.Current.FindResource(SortListView.ColumnHeaderContainerStyleKey)));
			//CellTemplateProperty.OverrideMetadata(typeof(SortViewColumn),
			//	new FrameworkPropertyMetadata(SortListView.CellTemplateKey));
			//HeaderContainerStyleProperty.OverrideMetadata(typeof(SortViewColumn),
			//	new FrameworkPropertyMetadata(SortViewKeys.HeaderContainerLeftAlignStyle));
			/*CellTemplateProperty.OverrideMetadata(typeof(SortViewColumn),
				new FrameworkPropertyMetadata(typeof(SortViewColumn)));*/
		}

		public SortViewColumn() {
			HeaderContainerStyle = SortViewKeys.HeaderContainerLeftAlignStyle;
			//if (SortViewKeys.HeaderContainerLeftAlignStyle == null)
			//	throw new InvalidOperationException("HALP!");
			/*SetValue(HeaderContainerStyleProperty,
				Activator.CreateInstance(Type.GetType(
					"System.Windows.ResourceReferenceExpression, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"),
					SortViewKeys.HeaderContainerLeftAlignStyleKey));*/
		}

		/*private static Style headerContainerLeftAlignStyle;
		private static Style headerContainerCenterAlignStyle;
		private static Style headerContainerRightAlignStyle;

		private static ResourceDictionary resourceDictionary;

		private static ResourceDictionary ResourceDictionary {
			get {
				if (resourceDictionary == null) {
					resourceDictionary = new ResourceDictionary();
					resourceDictionary.Source = new Uri("/WinDirStat.Net;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);
				}
				return resourceDictionary;
			}
		}

		public static Style HeaderContainerLeftAlignStyle {
			get {
				if (headerContainerLeftAlignStyle == null)
					headerContainerLeftAlignStyle = (Style) ResourceDictionary[HeaderContainerLeftAlignStyleKey];
				return headerContainerLeftAlignStyle;
			}
		}

		public static Style HeaderContainerCenterAlignStyle {
			get {
				if (headerContainerCenterAlignStyle == null)
					headerContainerCenterAlignStyle = (Style) ResourceDictionary[HeaderContainerCenterAlignStyleKey];
				return headerContainerCenterAlignStyle;
			}
		}

		public static Style HeaderContainerRightAlignStyle {
			get {
				if (headerContainerRightAlignStyle == null)
					headerContainerRightAlignStyle = (Style) ResourceDictionary[HeaderContainerRightAlignStyleKey];
				return headerContainerRightAlignStyle;
			}
		}

		public static ResourceKey HeaderContainerLeftAlignStyleKey { get; } =
			new ComponentResourceKey(typeof(SortViewColumn), "HeaderContainerLeftAlignStyleKey");

		public static ResourceKey HeaderContainerCenterAlignStyleKey { get; } =
			new ComponentResourceKey(typeof(SortViewColumn), "HeaderContainerCenterAlignStyleKey");

		public static ResourceKey HeaderContainerRightAlignStyleKey { get; } =
			new ComponentResourceKey(typeof(SortViewColumn), "HeaderContainerRightAlignStyleKey");*/

		private static void OnCellDataTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is SortViewColumn column) {
				if (column.cellBinding == null) {
					column.CellTemplate = column.CellDataTemplate;
				}
			}
		}

		private void RebuildCellTemplate() {
			var factory = new FrameworkElementFactory(typeof(TextBlock));
			factory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment);
			factory.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
			factory.SetBinding(TextBlock.TextProperty, cellBinding);
			DataTemplate template = new DataTemplate();
			template.VisualTree = factory;
			CellTemplate = template;
		}

		public BindingBase CellBinding {
			get => cellBinding;
			set {
				if (cellBinding != value) {
					cellBinding = value;
					if (cellBinding == null) {
						CellTemplate = CellDataTemplate;
					}
					else {
						RebuildCellTemplate();
					}
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(CellBinding)));
				}
			}
		}

		private BindingBase cellBinding;
	}
}
