using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	public class DelayedAction : IDisposable {

		private readonly static HashSet<DelayedAction> actions = new HashSet<DelayedAction>();

		private readonly Action action;
		private readonly Timer timer;

		public static DelayedAction RunIn(TimeSpan delay, Action action) {
			return new DelayedAction(delay, action);
		}

		public DelayedAction(TimeSpan delay, Action action) {
			this.action = action ?? throw new ArgumentNullException(nameof(action));
			actions.Add(this);
			timer = new Timer(OnTick, null, delay, Timeout.InfiniteTimeSpan);
		}

		private void OnTick(object state) {
			action();
			Dispose();
		}

		public void Dispose() {
			timer?.Dispose();
			actions.Remove(this);
		}

	}
}
