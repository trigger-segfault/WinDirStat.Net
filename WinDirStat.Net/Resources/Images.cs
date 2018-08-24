using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Resources {
	public static class Images {
		
		private static Dictionary<string, ImageSource> images = new Dictionary<string, ImageSource>();

		public static ImageSource Cmd { get; } = AutoLoadIcon();
		public static ImageSource CmdElevated { get; } = AutoLoadIcon();
		public static ImageSource Copy { get; } = AutoLoadIcon();
		public static ImageSource Cut { get; } = AutoLoadIcon();
		public static ImageSource Delete { get; } = AutoLoadIcon();
		public static ImageSource DeleteFolder { get; } = AutoLoadIcon();
		public static ImageSource DeleteRecycleBin { get; } = AutoLoadIcon();
		public static ImageSource Elevate { get; } = AutoLoadIcon();
		public static ImageSource Exit { get; } = AutoLoadIcon();
		public static ImageSource Expand { get; } = AutoLoadIcon();
		public static ImageSource Explore { get; } = AutoLoadIcon();
		public static ImageSource Open { get; } = AutoLoadIcon();
		public static ImageSource Paste { get; } = AutoLoadIcon();
		public static ImageSource Path { get; } = AutoLoadIcon();
		public static ImageSource PowerShell { get; } = AutoLoadIcon();
		public static ImageSource PowerShellElevated { get; } = AutoLoadIcon();
		public static ImageSource Properties { get; } = AutoLoadIcon();
		public static ImageSource Recycle { get; } = AutoLoadIcon();
		public static ImageSource Redo { get; } = AutoLoadIcon();
		public static ImageSource Reload { get; } = AutoLoadIcon();
		public static ImageSource RefreshSelected { get; } = AutoLoadIcon();
		public static ImageSource Run { get; } = AutoLoadIcon();
		public static ImageSource Save { get; } = AutoLoadIcon();
		public static ImageSource Search { get; } = AutoLoadIcon();
		public static ImageSource Settings { get; } = AutoLoadIcon();
		public static ImageSource Undo { get; } = AutoLoadIcon();

		static Images() {

		}

		private static string NormalizePath(string path) {
			return path.ToLower().Replace(@"\", "/"); ;
		}

		public static ImageSource Load(string path) {
			path = NormalizePath(path);
			if (!images.TryGetValue(path, out ImageSource image)) {
				image = BitmapUtils.FromResource(
					System.IO.Path.Combine("Resources", path),
					typeof(Images).Assembly);
				images.Add(path, image);
			}
			return image;
		}

		public static ImageSource LoadIcon(string path) {
			return Load(System.IO.Path.Combine("Icons", path));
		}

		public static ImageSource LoadFileIcon(string path) {
			return Load(System.IO.Path.Combine("FileIcons", path));
		}

		private static ImageSource AutoLoad([CallerMemberName] string path = null) {
			return Load(path + ".png");
		}

		private static ImageSource AutoLoadIcon([CallerMemberName] string path = null) {
			return LoadIcon(path + ".png");
		}

		private static ImageSource AutoLoadFileIcon([CallerMemberName] string path = null) {
			return LoadFileIcon(path + ".png");
		}
	}
}
