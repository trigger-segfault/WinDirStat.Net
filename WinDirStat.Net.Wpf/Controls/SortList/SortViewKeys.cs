using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

		public static ResourceKey HeaderContainerLeftAlignStyleKey { get; } =
			new ComponentResourceKey(typeof(SortViewKeys), "HeaderContainerLeftAlignStyleKey");

		public static ResourceKey HeaderContainerCenterAlignStyleKey { get; } =
			new ComponentResourceKey(typeof(SortViewKeys), "HeaderContainerCenterAlignStyleKey");

		public static ResourceKey HeaderContainerRightAlignStyleKey { get; } =
			new ComponentResourceKey(typeof(SortViewKeys), "HeaderContainerRightAlignStyleKey");
	}
}
