using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinDirStat.Net.Controls {
	/// <summary>A menu item with easy access for setting its icon.</summary>
	public class ImageMenuItem : MenuItem {

		/// <summary>The dependency property for the menu item's image.</summary>
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.RegisterAttached("Source", typeof(ImageSource), typeof(ImageMenuItem),
				new FrameworkPropertyMetadata(OnSourceChanged));

		/// <summary>Gets or sets the source of the menu item's image.</summary>
		[Category("Common")]
		public ImageSource Source {
			get => (ImageSource) GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		/// <summary>Called when the source property for the menu item is changed.</summary>
		private static void OnSourceChanged(object sender, DependencyPropertyChangedEventArgs e) {
			ImageMenuItem element = sender as ImageMenuItem;
			if (element != null) {
				element.image.Source = element.Source;
			}
		}

		/// <summary>Called when the icon property for the menu item is changed.</summary>
		private static void OnIconChanged(object sender, DependencyPropertyChangedEventArgs e) {
			ImageMenuItem element = sender as ImageMenuItem;
			if (element != null && element.Icon != element.image) {
				element.Icon = element.image;
			}
		}

		/// <summary>The image that contains the menu item's icon.</summary>
		private Image image;

		/// <summary>Initializes the image menu item default style.</summary>
		static ImageMenuItem() {
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageMenuItem),
			//		   new FrameworkPropertyMetadata(typeof(ImageMenuItem)));
			IconProperty.OverrideMetadata(typeof(ImageMenuItem),
				new FrameworkPropertyMetadata(OnIconChanged));
		}

		/// <summary>Constructs an empty menu item.</summary>
		public ImageMenuItem() {
			image = new Image() {
				Stretch = Stretch.None,
			};
			Icon = image;
		}

		/// <summary>Constructs an menu item with an image and name.</summary>
		public ImageMenuItem(ImageSource source, string name) {
			image = new Image() {
				Stretch = Stretch.None,
				Source = source,
			};
			Icon = image;
			Header = name;
		}
	}
}
