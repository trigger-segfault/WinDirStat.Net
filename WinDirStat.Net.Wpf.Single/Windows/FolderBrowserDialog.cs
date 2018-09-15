using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using FormsFolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using Window = System.Windows.Window;

namespace WinDirStat.Net.Wpf.Windows {
	/// <summary>
	/// A wrapper for the folder browser that makes sure it scrolls to the selected folder.
	/// </summary>
	/// <remarks>
	/// <a href="https://stackoverflow.com/questions/6942150/why-folderbrowserdialog-dialog-does-not-scroll-to-selected-folder"/>
	/// </remarks>
	public class FolderBrowserDialog {

		/// <summary>
		/// Gets or sets a value indicating whether the New Folder button appears in the folder browser
		/// dialog box.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(true)]
		[Localizable(false)]
		public bool ShowNewFolderButton { get; set; } = true;

		/// <summary>Gets or sets the path selected by the user.</summary>
		[Browsable(true)]
		[DefaultValue("")]
		public string SelectedPath { get; set; } = "";

		/// <summary>Gets or sets the root folder where the browsing starts from.</summary>
		[Browsable(true)]
		[DefaultValue(Environment.SpecialFolder.Desktop)]
		public Environment.SpecialFolder RootFolder { get; set; } = Environment.SpecialFolder.Desktop;

		/// <summary>
		/// Gets or sets the descriptive text displayed above the tree view control in the dialog box.
		/// </summary>
		[Browsable(true)]
		[DefaultValue("")]
		public string Description { get; set; } = "";
		
		/// <summary>Shows the folder browser dialog.</summary>
		/// <returns>The result of the dialog.</returns>
		public bool? ShowDialog() {
			return ShowDialog(null);
		}

		/// <summary>Shows the folder browser dialog.</summary>
		/// <param name="owner">The window that owns the dialog</param>
		/// <returns>The result of the dialog.</returns>
		public bool? ShowDialog(Window owner) {
			using (FormsFolderBrowserDialog dialog = new FormsFolderBrowserDialog()) {
				dialog.ShowNewFolderButton = ShowNewFolderButton;
				dialog.SelectedPath = SelectedPath;
				dialog.RootFolder = RootFolder;
				dialog.Description = Description;
				
				bool? result = ShowFolderBrowser(dialog, owner);
				if (result ?? false)
					SelectedPath = dialog.SelectedPath;
				return result;
			}
		}

		/// <summary>
		/// Using title text to look for the top level dialog window is fragile.
		/// In particular, this will fail in non-English applications.
		/// </summary>
		private const string _topLevelSearchString = "Browse For Folder";

		/// <summary>
		/// These should be more robust.  We find the correct child controls in the dialog
		/// by using the GetDlgItem method, rather than the FindWindow(Ex) method,
		/// because the dialog item IDs should be constant.
		/// </summary>
		private const int _dlgItemBrowseControl = 0;
		private const int _dlgItemTreeView = 100;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Some of the messages that the Tree View control will respond to
		/// </summary>
		private const int TV_FIRST = 0x1100;
		private const int TVM_SELECTITEM = (TV_FIRST + 11);
		private const int TVM_GETNEXTITEM = (TV_FIRST + 10);
		private const int TVM_GETITEM = (TV_FIRST + 12);
		private const int TVM_ENSUREVISIBLE = (TV_FIRST + 20);

		/// <summary>
		/// Constants used to identity specific items in the Tree View control
		/// </summary>
		private const int TVGN_ROOT = 0x0;
		private const int TVGN_NEXT = 0x1;
		private const int TVGN_CHILD = 0x4;
		private const int TVGN_FIRSTVISIBLE = 0x5;
		private const int TVGN_NEXTVISIBLE = 0x6;
		private const int TVGN_CARET = 0x9;

		/// <summary>
		/// Calling this method is identical to calling the ShowDialog method of the provided
		/// FolderBrowserDialog, except that an attempt will be made to scroll the Tree View
		/// to make the currently selected folder visible in the dialog window.
		/// </summary>
		/// <param name="dlg"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		private static bool? ShowFolderBrowser(FormsFolderBrowserDialog dlg, Window owner = null) {
			DialogResult result = DialogResult.Cancel;
			int retries = 40;
			NativeWindow parent = null;
			if (owner != null) {
				parent = new NativeWindow();
				parent.AssignHandle(new WindowInteropHelper(owner).Handle);
			}

			using (Timer t = new Timer()) {
				t.Tick += (s, a) => {
					if (retries > 0) {
						--retries;
						IntPtr hwndDlg = FindWindow((string) null, _topLevelSearchString);
						if (hwndDlg != IntPtr.Zero) {
							IntPtr hwndFolderCtrl = GetDlgItem(hwndDlg, _dlgItemBrowseControl);
							if (hwndFolderCtrl != IntPtr.Zero) {
								IntPtr hwndTV = GetDlgItem(hwndFolderCtrl, _dlgItemTreeView);

								if (hwndTV != IntPtr.Zero) {
									IntPtr item = SendMessage(hwndTV, (uint) TVM_GETNEXTITEM, new IntPtr(TVGN_CARET), IntPtr.Zero);
									if (item != IntPtr.Zero) {
										// Let's send this 40 times until we drill it into the folder browser's thick skull.
										// Otherwise it will just go back to the beginning.
										SendMessage(hwndTV, TVM_ENSUREVISIBLE, IntPtr.Zero, item);
										//retries = 0;
										//t.Stop();
									}
								}
							}
						}
					}

					else {
						//
						//  We failed to find the Tree View control.
						//
						//  As a fall back (and this is an UberUgly hack), we will send
						//  some fake keystrokes to the application in an attempt to force
						//  the Tree View to scroll to the selected item.
						//
						t.Stop();
						//SendKeys.Send("{TAB}{TAB}{DOWN}{DOWN}{UP}{UP}");
					}
				};

				t.Interval = 10;
				t.Start();

				result = dlg.ShowDialog(parent);
				// Very important, I've learned my lesson
				parent.ReleaseHandle();
			}

			return (result == DialogResult.OK);
		}
	}
}
