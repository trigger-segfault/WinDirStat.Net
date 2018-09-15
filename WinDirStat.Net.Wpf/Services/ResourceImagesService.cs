using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.Wpf.Services {
	/// <summary>A service for image references.</summary>
	public class ResourceImagesService : ImagesServiceBase {

		#region Constructors

		/// <summary>Constructs the <see cref="ResourceImagesService"/>.</summary>
		public ResourceImagesService(BitmapFactory bitmapFactory,
									 UIService ui)
			: base(bitmapFactory,
				   ui)
		{
		}

		#endregion

		#region ImagesServiceBase Implementation

		/// <summary>Loads the image resource with the specified path.</summary>
		/// 
		/// <param name="path">The path of the image resource.</param>
		private ImageSource Load(string path) {
			return BitmapFactory.FromResource(Path.Combine("Resources", path));
		}

		/// <summary>Loads the icon with the specified name.</summary>
		/// 
		/// <param name="name">The name of the icon.</param>
		protected override ImageSource LoadIcon(string name) {
			return Load(Path.Combine("Icons", name + ".png"));
		}

		/// <summary>Loads the file icon with the specified name.</summary>
		/// 
		/// <param name="name">The name of the file icon.</param>
		protected override ImageSource LoadFileIcon(string name) {
			return Load(Path.Combine("FileIcons", name + ".png"));
		}

		#endregion
	}
}
