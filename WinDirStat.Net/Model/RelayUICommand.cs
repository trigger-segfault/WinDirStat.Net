using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace WinDirStat.Net.Model {
	public class RelayUICommand : RelayCommand, IRelayUICommand {

		#region Fields

		private string text;
		private ImageSource icon;
		private readonly KeyGesture keyGesture;

		#endregion

		#region Constructors
		
		public RelayUICommand(string text, ImageSource icon, Action execute)
			: this(text, icon, null, execute, null)
		{
		}
		public RelayUICommand(string text, ImageSource icon, Action<object> execute)
			: this(text, icon, null, execute, null)
		{
		}
		
		public RelayUICommand(string text, ImageSource icon, Action execute,
			Func<bool> canExecute)
			: this(text, icon, null, execute, canExecute)
		{
		}
		public RelayUICommand(string text, ImageSource icon, Action<object> execute,
			Predicate<object> canExecute)
			: this(text, icon, null, execute, canExecute)
		{
		}
		
		public RelayUICommand(string text, ImageSource icon, KeyGesture keyGesture,
			Action execute)
			: this(text, icon, keyGesture, execute, null)
		{
		}
		public RelayUICommand(string text, ImageSource icon, KeyGesture keyGesture,
			Action<object> execute)
			: this(text, icon, keyGesture, execute, null)
		{
		}
		
		public RelayUICommand(string text, ImageSource icon, KeyGesture keyGesture,
			Action execute, Func<bool> canExecute)
			: base(execute, canExecute)
		{
			this.text = text;
			this.icon = icon;
			this.keyGesture = keyGesture;
		}
		public RelayUICommand(string text, ImageSource icon, KeyGesture keyGesture,
			Action<object> execute, Predicate<object> canExecute)
			: base(execute, canExecute)
		{
			this.text = text;
			this.icon = icon;
			this.keyGesture = keyGesture;
		}

		#endregion

		#region Properties

		public string Text {
			get => text;
			set {
				if (text != value) {
					text = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public ImageSource Icon {
			get => icon;
			set {
				if (icon != value) {
					icon = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public KeyGesture InputGesture {
			get => keyGesture;
		}

		#endregion
	}
}
