using System.IO;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Wpf.Services {
	/// <summary>A service for image references.</summary>
	public class ResourceImagesService : ImagesServiceBase {

		#region Constructors

		/// <summary>Constructs the <see cref="ResourceImagesService"/>.</summary>
		public ResourceImagesService(IBitmapFactory bitmapFactory,
									 IUIService ui)
			: base(bitmapFactory,
				   ui)
		{
		}

		#endregion

		#region ImagesServiceBase Implementation

		/// <summary>Loads the image resource with the specified path.</summary>
		/// 
		/// <param name="path">The path of the image resource.</param>
		private IImage Load(string path) {
			return BitmapFactory.FromResource(Path.Combine("Resources", path));
		}

		/// <summary>Loads the icon with the specified name.</summary>
		/// 
		/// <param name="name">The name of the icon.</param>
		protected override IImage LoadIcon(string name) {
			return Load(Path.Combine("Icons", name + ".png"));
		}

		/// <summary>Loads the file icon with the specified name.</summary>
		/// 
		/// <param name="name">The name of the file icon.</param>
		protected override IImage LoadFileIcon(string name) {
			return Load(Path.Combine("FileIcons", name + ".png"));
		}

		#endregion
	}
}
