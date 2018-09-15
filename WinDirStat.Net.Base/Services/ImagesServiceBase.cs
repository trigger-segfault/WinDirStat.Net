using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WinDirStat.Net.Services {
	/// <summary>A service for image references.</summary>
	public abstract class ImagesServiceBase {

		#region File Icon Fields

		/// <summary>Gets the icon for file collection items.</summary>
		public ImageSource FileCollection { get; protected set; }
		/// <summary>Gets the icon for free space items.</summary>
		public ImageSource FreeSpace { get; protected set; }
		/// <summary>Gets the icon for unknown space items.</summary>
		public ImageSource UnknownSpace { get; protected set; }
		/// <summary>Gets the icon for files that no longer exist.</summary>
		public ImageSource Missing { get; protected set; }

		#endregion

		#region Icon Fields

		/// <summary>The icon for the Close command.</summary>
		public ImageSource Close { get; protected set; }
		/// <summary>The icon Command Prompt command.</summary>
		public ImageSource Cmd { get; protected set; }
		//public ImageSource CmdElevated { get; protected set; }
		/// <summary>The icon Copy command.</summary>
		public ImageSource Copy { get; protected set; }
		/// <summary>The icon Cut command.</summary>
		public ImageSource Cut { get; protected set; }
		/// <summary>The icon Delete command.</summary>
		public ImageSource Delete { get; protected set; }
		/// <summary>The icon Elevate command.</summary>
		public ImageSource Elevate { get; protected set; }
		/// <summary>The icon Empty Recycle Bin command.</summary>
		public ImageSource EmptyRecycleBin { get; protected set; }
		/// <summary>The icon Exit command.</summary>
		public ImageSource Exit { get; protected set; }
		/// <summary>The icon Expand command.</summary>
		public ImageSource Expand { get; protected set; }
		/// <summary>The icon ExploreCommand command.</summary>
		public ImageSource Explore { get; protected set; }
		/// <summary>The icon Open command.</summary>
		public ImageSource Open { get; protected set; }
		/// <summary>The icon Paste command.</summary>
		public ImageSource Paste { get; protected set; }
		/// <summary>The icon Copy Path command.</summary>
		public ImageSource CopyPath { get; protected set; }
		/// <summary>The icon PowerShell command.</summary>
		public ImageSource PowerShell { get; protected set; }
		//public ImageSource PowerShellElevated { get; protected set; }
		/// <summary>The icon Properties command.</summary>
		public ImageSource Properties { get; protected set; }
		/// <summary>The icon Recycle command.</summary>
		public ImageSource RecycleBin { get; protected set; }
		/// <summary>The icon Redo command.</summary>
		public ImageSource Redo { get; protected set; }
		/// <summary>The icon Reload command.</summary>
		public ImageSource Reload { get; protected set; }
		/// <summary>The icon Refresh Selected command.</summary>
		public ImageSource RefreshSelected { get; protected set; }
		/// <summary>The icon Run (Open Item) command.</summary>
		public ImageSource Run { get; protected set; }
		/// <summary>The icon Save command.</summary>
		public ImageSource Save { get; protected set; }
		/// <summary>The icon Search command.</summary>
		public ImageSource Search { get; protected set; }
		/// <summary>The icon Settings command.</summary>
		public ImageSource Settings { get; protected set; }
		/*/// <summary>The icon Show File Types command.</summary>
		public ImageSource ShowFileTypes { get; protected set; }*/
		/// <summary>The icon Show Total Space command.</summary>
		public ImageSource ShowTotalSpace { get; protected set; }
		/// <summary>The icon Show Treemap command.</summary>
		public ImageSource ShowTreemap { get; protected set; }
		/// <summary>The icon Undo command.</summary>
		public ImageSource Undo { get; protected set; }

		#endregion

		#region Fields

		/// <summary>The bitmap factory service.</summary>
		protected BitmapFactory BitmapFactory { get; }
		/// <summary>Gets the UI service.</summary>
		protected UIService UI { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="ImagesServiceBase"/>.</summary>
		public ImagesServiceBase(BitmapFactory bitmapFactory,
								 UIService ui)
		{
			BitmapFactory = bitmapFactory;
			UI = ui;

			UI.Invoke(() => {
				// Load file type icons with a different method
				FileCollection  = LoadFileIcon(nameof(FileCollection));
				FreeSpace       = LoadFileIcon(nameof(FreeSpace));
				UnknownSpace    = LoadFileIcon(nameof(UnknownSpace));
				Missing			= LoadFileIcon(nameof(Missing));

				// Load all unassigned icons with reflection
				foreach (PropertyInfo propInfo in typeof(ImagesServiceBase).GetProperties()) {
					if (propInfo.PropertyType == typeof(ImageSource) && propInfo.CanWrite) {
						ImageSource value = (ImageSource) propInfo.GetValue(this);
						if (value != null)
							continue;
						value = LoadIcon(propInfo.Name);
						propInfo.SetValue(this, value);
					}
				}
			});
		}

		#endregion

		#region Abstract Methods

		/// <summary>Loads the icon with the specified name.</summary>
		/// 
		/// <param name="name">The name of the icon.</param>
		protected abstract ImageSource LoadIcon(string name);

		/// <summary>Loads the file icon with the specified name.</summary>
		/// 
		/// <param name="name">The name of the file icon.</param>
		protected abstract ImageSource LoadFileIcon(string name);

		#endregion
	}
}
