using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinDirStat.Net.Wpf.Controls.SortList {
	public class SortView : GridView {

		static SortView() {
			//Console.WriteLine($"DEFAULT: {GridView.ColumnHeaderContainerStyleProperty.DefaultMetadata.DefaultValue}");
			//Console.WriteLine($"COERCE: {GridView.ColumnHeaderContainerStyleProperty.DefaultMetadata.CoerceValueCallback}");
			//ColumnHeaderContainerStyleProperty.OverrideMetadata(typeof(SortView),
			//	new FrameworkPropertyMetadata(null, null, CoerceColumnHeaderContainerStyle));
				//new FrameworkPropertyMetadata(null, null, CoerceColumnHeaderContainerStyle));
		}

		private static object CoerceColumnHeaderContainerStyle(DependencyObject d, object baseValue) {
			if (baseValue == null)
				return SortViewKeys.HeaderContainerLeftAlignStyle;
			return baseValue;
		}


		protected override object DefaultStyleKey {
			get => SortViewStyleKey;
		}

		protected override object ItemContainerDefaultStyleKey {
			get => SortViewItemContainerStyleKey;
		}

		public static ResourceKey SortViewStyleKey { get; } =
			new ComponentResourceKey(typeof(SortView), "SortViewStyleKey");

		public static ResourceKey SortViewItemContainerStyleKey { get; } =
			new ComponentResourceKey(typeof(SortView), "SortViewItemContainerStyleKey");

		public SortView() {
			Columns.CollectionChanged += OnColumnsChanged;
		}
		
		private void OnColumnsChanged(object sender, NotifyCollectionChangedEventArgs e) {
			// Nothing needs to be changed if moving columns around
			if (e.Action == NotifyCollectionChangedAction.Move)
				return;

			if (e.NewItems != null) {
				foreach (object item in e.NewItems) {
					//if (item is SortViewColumn column)
					//	column.HeaderContainerStyle = SortViewKeys.HeaderContainerLeftAlignStyle;
					if (!(item is SortViewColumn))
						throw new InvalidOperationException($"Column is not a {nameof(SortViewColumn)}!");
				}
			}
		}

		protected override void AddChild(object column) {
			if (column is SortViewColumn c)
				Columns.Add(c);
			else
				throw new InvalidOperationException($"column is not a {nameof(SortViewColumn)}!");
		}
	}
}
