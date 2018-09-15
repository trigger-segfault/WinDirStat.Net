using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinDirStat.Net.Utils;
using WinDirStat.Net.Wpf.Utils;

namespace WinDirStat.Net.Wpf.Controls {
	public class ImageButton : Button {

		/// <summary>The dependency property for the button's image.</summary>
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.RegisterAttached("Source", typeof(ImageSource), typeof(ImageButton),
				new FrameworkPropertyMetadata(OnSourceChanged));

		/// <summary>Gets or sets the source of the button's image.</summary>
		[Category("Common")]
		public ImageSource Source {
			get => (ImageSource) GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		/// <summary>Called when the source property for the button is changed.</summary>
		private static void OnSourceChanged(object sender, DependencyPropertyChangedEventArgs e) {
			ImageButton button = (ImageButton) sender;
			button.image.Source = button.Source;

			button.CoerceValue(ContentProperty);
		}

		private static object CoerceContent(DependencyObject d, object value) {
			ImageButton button = (ImageButton) d;

			if (button.IsValueUnsetAndNull(ContentProperty, value)) {
				return button.image;
			}
			return value;
		}

		/// <summary>The image that contains the buttons's icon.</summary>
		private Image image;

		/// <summary>Initializes the image buttons default style.</summary>
		static ImageButton() {
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton),
			//		   new FrameworkPropertyMetadata(typeof(ImageButton)));
			ContentProperty.OverrideMetadata(typeof(ImageButton),
				new FrameworkPropertyMetadata(null, CoerceContent));
		}

		/// <summary>Constructs an empty buttons.</summary>
		public ImageButton() {
			image = new Image() {
				Stretch = Stretch.None,
			};
		}
	}
}
