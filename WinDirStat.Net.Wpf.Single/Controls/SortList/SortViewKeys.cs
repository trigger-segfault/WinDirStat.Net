using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WinDirStat.Net.Wpf.Controls.SortList {
	public class SortViewKeys {
		private static Style headerContainerLeftAlignStyle;
		private static Style headerContainerCenterAlignStyle;
		private static Style headerContainerRightAlignStyle;
		
		public static Style HeaderContainerLeftAlignStyle {
			get {
				if (headerContainerLeftAlignStyle == null)
					headerContainerLeftAlignStyle = (Style) Application.Current.FindResource(HeaderContainerLeftAlignStyleKey);
				return headerContainerLeftAlignStyle;
			}
		}

		public static Style HeaderContainerCenterAlignStyle {
			get {
				if (headerContainerCenterAlignStyle == null)
					headerContainerCenterAlignStyle = (Style) Application.Current.FindResource(HeaderContainerCenterAlignStyleKey);
				return headerContainerCenterAlignStyle;
			}
		}

		public static Style HeaderContainerRightAlignStyle {
			get {
				if (headerContainerRightAlignStyle == null)
					headerContainerRightAlignStyle = (Style) Application.Current.FindResource(HeaderContainerRightAlignStyleKey);
				return headerContainerRightAlignStyle;
			}
		}

		public static Style GetHeaderContainerLeftAlignStyle(Control control) {
			return (Style) control.FindResource(HeaderContainerLeftAlignStyleKey);
		}
		public static Style GetHeaderContainerCenterAlignStyle(Control control) {
			return (Style) control.FindResource(HeaderContainerCenterAlignStyleKey);
		}
		public static Style GetHeaderContainerRightAlignStyle(Control control) {
			return (Style) control.FindResource(HeaderContainerRightAlignStyleKey);
		}

		public static Style GetHeaderContainerLeftAlignStyle(DependencyObject d) {
			return (Style) Window.GetWindow(d).FindResource(HeaderContainerLeftAlignStyleKey);
		}
		public static Style GetHeaderContainerCenterAlignStyle(DependencyObject d) {
			return (Style) Window.GetWindow(d).FindResource(HeaderContainerCenterAlignStyleKey);
		}
		public static Style GetHeaderContainerRightAlignStyle(DependencyObject d) {
			return (Style) Window.GetWindow(d).FindResource(HeaderContainerRightAlignStyleKey);
		}

		public static ResourceKey HeaderContainerLeftAlignStyleKey =>
			new ComponentResourceKey(typeof(SortViewKeys), "HeaderContainerLeftAlignStyleKey");

		public static ResourceKey HeaderContainerCenterAlignStyleKey =>
			new ComponentResourceKey(typeof(SortViewKeys), "HeaderContainerCenterAlignStyleKey");

		public static ResourceKey HeaderContainerRightAlignStyleKey =>
			new ComponentResourceKey(typeof(SortViewKeys), "HeaderContainerRightAlignStyleKey");
	}
}
