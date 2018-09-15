using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.ViewModel.Extensions {
	/// <summary>The view model that represents an <see cref="ExtensionItem"/>.</summary>
	public class ExtensionItemViewModel : ObservableObject, IDisposable {

		#region Constants

		/// <summary>The <see cref="ExtensionItemViewModel"/> used to represent a non-file.</summary>
		public static readonly ExtensionItemViewModel NotAFile = new ExtensionItemViewModel();

		#endregion

		#region Fields

		/// <summary>The collection containing this item.</summary>
		private readonly ExtensionItemViewModelCollection extensions;
		/// <summary>Gets the model that this view model represents.</summary>
		public ExtensionItem Model { get; }

		/// <summary>The icon of the associated file type.</summary>
		private ImageSource icon;
		/// <summary>The type name of the associated file type.</summary>
		private string typeName;
		/// <summary>The cache state for the icon.</summary>
		private IconCacheState cacheState;
		/// <summary>True if events have been hooked to the model.</summary>
		private bool eventsHooked;
		/// <summary>The preview image for the file palette color.</summary>
		private ImageSource preview;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs an <see cref="ExtensionItemViewModel"/> used to represent a non-file.
		/// </summary>
		private ExtensionItemViewModel() {
			Model = ExtensionItem.NotAFile;
		}

		/// <summary>Constructs an <see cref="ExtensionItemViewModel"/>.</summary>
		/// 
		/// <param name="extensions">The collection containing this item.</param>
		/// <param name="model">The model that this view model item represents.</param>
		public ExtensionItemViewModel(ExtensionItemViewModelCollection extensions, ExtensionItem model) {
			this.extensions = extensions;
			Model = model;
			icon = IconCache.FileIcon;
			if (IsEmptyExtension || Extension.Contains(" ")) {
				// Extensions with spaces have no rights, so says Windows
				typeName = "File";
				cacheState = IconCacheState.Cached;
			}
			else {
				typeName = model.Extension.TrimStart('.').ToUpper() + " File";
				if (CacheMode >= IconCacheMode.FileType)
					LoadIcon();
			}
			HookEvents();
		}

		/// <summary>Disposes of the <see cref="ExtensionItemViewModel"/>.</summary>
		/*~ExtensionItemViewModel() {
			Dispose();
		}*/

		#endregion

		#region Event Handlers

		private void OnModelChanged(ExtensionItem sender, ExtensionItemEventArgs e) {
			switch (e.Action) {
			case ExtensionItemAction.ColorChanged:
				Preview = Settings.GetFilePalettePreview(e.Index);
				break;
			case ExtensionItemAction.GetViewModel:
				e.ViewModel = this;
				break;
			}
		}

		#endregion

		#region CacheIcon

		/// <summary>Attempts to load the icon if it has not already been loaded.</summary>
		public void LoadIcon() {
			if (cacheState == IconCacheState.NotCached) {
				cacheState = IconCacheState.Caching;
				IconCache.CacheFileTypeAsync(Extension, OnCacheIconAndTypeName);
			}
		}

		/// <summary>The callback for asynchronously caching the file type icon and type name.</summary>
		/// 
		/// <param name="iconName">The resulting icon and type name.</param>
		private void OnCacheIconAndTypeName(IconAndName iconName) {
			if (iconName != null) {
				Icon = iconName.Icon;
				TypeName = iconName.Name;
			}
			// Even if "I failed, I failed, I failed", we're not gonna reattempt this
			cacheState = IconCacheState.Cached;
		}

		#endregion

		#region Properties

		/// <summary>Gets the name of the extension with the dot.</summary>
		public string Extension => Model.Extension;
		/// <summary>Gets the total size of all the files that use this extension.</summary>
		public long Size => Model.Size;
		/// <summary>Gets the number of files that use this extension.</summary>
		public int FileCount => Model.FileCount;
		/// <summary>Gets this extension's size relative to the total used space.</summary>
		public double Percent => Model.Percent;
		/// <summary>Gets if this extension is the empty extension with nothing after the dot.</summary>
		public bool IsEmptyExtension => Model.IsEmptyExtension;

		/// <summary>Gets the icon of the associated file type.</summary>
		public ImageSource Icon {
			get => icon;
			private set => Set(ref icon, value);
		}

		/// <summary>Gets the type name of the associated file type.</summary>
		public string TypeName {
			get => typeName;
			private set => Set(ref typeName, value);
		}

		/// <summary>Gets the preview image for the file palette color.</summary>
		public ImageSource Preview {
			get => preview;
			internal set => Set(ref preview, value);
		}

		/// <summary>Gets the cache state for the icon.</summary>
		public IconCacheState CacheState {
			get => cacheState;
			private set => Set(ref cacheState, value);
		}

		/// <summary>Gets the icon cache service.</summary>
		private IconCacheService IconCache => extensions.IconCache;

		/// <summary>Gets the program settings service.</summary>
		private SettingsService Settings => extensions.Settings;

		/// <summary>Gets the icon cache mode setting.</summary>
		private IconCacheMode CacheMode => extensions.Settings.IconCacheMode;

		#endregion

		#region IDisposable Implementation

		/// <summary>Disposes of the <see cref="ExtensionItemViewModel"/>.</summary>
		public void Dispose() {
			UnhookEvents();
		}

		/// <summary>Hooks up events to the model changed event.</summary>
		private void HookEvents() {
			if (!eventsHooked) {
				eventsHooked = true;
				Model.Changed += OnModelChanged;
			}
		}

		/// <summary>Unhooks events from the model changed event.</summary>
		private void UnhookEvents() {
			if (eventsHooked) {
				eventsHooked = false;
				Model.Changed -= OnModelChanged;
			}
		}

		#endregion
	}
}
