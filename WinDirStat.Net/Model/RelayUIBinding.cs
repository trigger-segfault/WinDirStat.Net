using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WinDirStat.Net.Model {
	public class RelayUIBinding : KeyBinding {

		private static void OnCommandChanged(object sender, DependencyPropertyChangedEventArgs e) {
			((RelayUIBinding) sender).OnCommandChanged(e);
		}

		private void OnCommandChanged(DependencyPropertyChangedEventArgs e) {
			base.Gesture = RelayUICommand?.InputGesture;
		}

		static RelayUIBinding() {
			CommandProperty.AddOwner(typeof(RelayUIBinding),
				new UIPropertyMetadata(OnCommandChanged));
		}

		public IRelayUICommand RelayUICommand {
			get => Command as IRelayUICommand;
		}

		public override InputGesture Gesture {
			get => base.Gesture;
			set => throw new NotSupportedException();
		}
	}
}
