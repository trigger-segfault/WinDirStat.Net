using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WinDirStat.Net.Model.Data.Extensions;
using WinDirStat.Net.Settings.Geometry;

namespace WinDirStat.Net.Model.View.Extensions {
	public class ExtensionRecordViewModel : ObservableObject {
		public static readonly ExtensionRecordViewModel Empty = new ExtensionRecordViewModel();

		private readonly ExtensionRecordsViewModel records;
		private readonly ExtensionRecord model;
		private string name;
		private ImageSource icon;
		private ImageSource preview;

		private ExtensionRecordViewModel() {
			model = ExtensionRecord.Empty;
			name = "Not a File";
		}

		public ExtensionRecordViewModel(ExtensionRecordsViewModel records, ExtensionRecord model) {
			this.records = records;
			this.model = model;
			icon = IconCache.FileIcon;
			if (IsEmptyExtension) {
				name = "File";
			}
			else {
				name = model.Extension.TrimStart('.').ToUpper() + " File";
				records.Icons.CacheFileTypeAsync(model.Extension, OnCacheIcon);
			}
			model.Changed += OnModelChanged;
		}

		private void OnModelChanged(ExtensionRecord sender, ExtensionRecordEventArgs e) {
			switch (e.Action) {
			case ExtensionRecordAction.GetView:
				e.View = this;
				break;
			}
		}

		private void OnCacheIcon(ImageSource icon, string name) {
			if (icon != null)
				Icon = icon;
			if (name != null)
				Name = name;
		}

		public ExtensionRecord Model {
			get => model;
		}

		public string Extension {
			get => model.Extension;
		}

		public string Name {
			get => name;
			private set {
				if (name != value) {
					name = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public long Size {
			get => model.Size;
		}

		public double Percent {
			get => model.Percent;
		}

		public int FileCount {
			get => model.FileCount;
		}

		public bool IsEmptyExtension {
			get => model.IsEmptyExtension;
		}

		public Rgb24Color Color {
			get => model.Color;
			internal set {
				model.Color = value;
				AutoRaisePropertyChanged();
			}
		}

		public ImageSource Icon {
			get => icon;
			private set {
				if (icon != value) {
					icon = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public ImageSource Preview {
			get => preview;
			internal set {
				if (preview != value) {
					preview = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public override string ToString() {
			return model.Extension;
		}
	}
}
