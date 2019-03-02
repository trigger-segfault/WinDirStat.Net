using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.ViewModel {
	partial class MainViewModel {

		public void Loaded() {
		}

		public void SortFiles() {
			SuppressFileTreeRefresh = true;
			RootItem?.Sort(FileComparer.Compare);
			SuppressFileTreeRefresh = false;
		}
		
		public void SortExtensions() {
			Extensions.Sort(ExtensionComparer.Compare);
		}

		public void ActivateItem() {
			// TODO: Determine the default action for each item type
		}
		
		public void UpdateEmptyRecycleBin() {
			Debug.WriteLine("UpdateEmptyRecycleBin");
			RecycleBinInfo info = OS.GetAllRecycleBinInfo();
			string label = "Empty Recycle Bins";
			if (info != null) {
				label += " (";
				if (info.ItemCount == 0 && info.Size == 0) {
					label += "Empty";
				}
				else {
					label += $"{info.ItemCount:N0} ";
					if (info.ItemCount == 1)
						label += $"Item";
					else
						label += $"Items";
					label += $", {FormatBytes.Format(info.Size)}";
				}
				label += ")";
			}
			EmptyRecycleBinLabel = label;
			allRecycleBinInfo = info;
			EmptyRecycleBin.RaiseCanExecuteChanged();
		}

		public void WindowShown() {
			Open.Execute();
		}
	}
}
