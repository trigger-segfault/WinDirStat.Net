using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Services {
	/// <summary>A service for image references.</summary>
	public abstract class ImagesServiceBase {

		#region File Icon Fields

		/// <summary>Gets the icon for file collection items.</summary>
		public IImage FileCollection { get; protected set; }
		/// <summary>Gets the icon for free space items.</summary>
		public IImage FreeSpace { get; protected set; }
		/// <summary>Gets the icon for unknown space items.</summary>
		public IImage UnknownSpace { get; protected set; }
		/// <summary>Gets the icon for files that no longer exist.</summary>
		public IImage Missing { get; protected set; }

		#endregion

		#region Icon Fields

		/// <summary>The icon for the Close command.</summary>
		public IImage Close { get; protected set; }
		/// <summary>The icon Command Prompt command.</summary>
		public IImage Cmd { get; protected set; }
		//public ImageSource CmdElevated { get; protected set; }
		/// <summary>The icon Copy command.</summary>
		public IImage Copy { get; protected set; }
		/// <summary>The icon Cut command.</summary>
		public IImage Cut { get; protected set; }
		/// <summary>The icon Delete command.</summary>
		public IImage Delete { get; protected set; }
		/// <summary>The icon Elevate command.</summary>
		public IImage Elevate { get; protected set; }
		/// <summary>The icon Empty Recycle Bin command.</summary>
		public IImage EmptyRecycleBin { get; protected set; }
		/// <summary>The icon Exit command.</summary>
		public IImage Exit { get; protected set; }
		/// <summary>The icon Expand command.</summary>
		public IImage Expand { get; protected set; }
		/// <summary>The icon ExploreCommand command.</summary>
		public IImage Explore { get; protected set; }
		/// <summary>The icon Open command.</summary>
		public IImage Open { get; protected set; }
		/// <summary>The icon Paste command.</summary>
		public IImage Paste { get; protected set; }
		/// <summary>The icon Copy Path command.</summary>
		public IImage CopyPath { get; protected set; }
		/// <summary>The icon PowerShell command.</summary>
		public IImage PowerShell { get; protected set; }
		//public IImage PowerShellElevated { get; protected set; }
		/// <summary>The icon Properties command.</summary>
		public IImage Properties { get; protected set; }
		/// <summary>The icon Recycle command.</summary>
		public IImage RecycleBin { get; protected set; }
		/// <summary>The icon Redo command.</summary>
		public IImage Redo { get; protected set; }
		/// <summary>The icon Reload command.</summary>
		public IImage Reload { get; protected set; }
		/// <summary>The icon Refresh Selected command.</summary>
		public IImage RefreshSelected { get; protected set; }
		/// <summary>The icon Run (Open Item) command.</summary>
		public IImage Run { get; protected set; }
		/// <summary>The icon Save command.</summary>
		public IImage Save { get; protected set; }
		/// <summary>The icon Search command.</summary>
		public IImage Search { get; protected set; }
		/// <summary>The icon Settings command.</summary>
		public IImage Settings { get; protected set; }
		/*/// <summary>The icon Show File Types command.</summary>
		public IImage ShowFileTypes { get; protected set; }*/
		/// <summary>The icon Show Total Space command.</summary>
		public IImage ShowTotalSpace { get; protected set; }
		/// <summary>The icon Show Treemap command.</summary>
		public IImage ShowTreemap { get; protected set; }
		/// <summary>The icon Undo command.</summary>
		public IImage Undo { get; protected set; }

		#endregion

		#region Fields

		/// <summary>The bitmap factory service.</summary>
		protected IBitmapFactory BitmapFactory { get; }
		/// <summary>Gets the UI service.</summary>
		protected IUIService UI { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="ImagesServiceBase"/>.</summary>
		public ImagesServiceBase(IBitmapFactory bitmapFactory,
								 IUIService ui)
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
					if (propInfo.PropertyType == typeof(IImage) && propInfo.CanWrite) {
						IImage value = (IImage) propInfo.GetValue(this);
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
		protected abstract IImage LoadIcon(string name);

		/// <summary>Loads the file icon with the specified name.</summary>
		/// 
		/// <param name="name">The name of the file icon.</param>
		protected abstract IImage LoadFileIcon(string name);

		#endregion
	}
}
