using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	public delegate void SpeedTestCallback(SpeedTestResult results);

	public struct SpeedTestResult {
		public string Name { get; }
		public int Calls { get; }
		public TimeSpan Duration { get; }
		public double CallsPerSecond { get; }
		public double CallsPerMillisecond => CallsPerSecond / 1000d;
		public double CallsPerMicrosecond => CallsPerSecond / 1000000d;
		public TimeSpan AverageCallTime { get; }

		public SpeedTestResult(string name, TimeSpan duration) {
			Name = name;
			Calls = 1;
			Duration = duration;
			CallsPerSecond = 1d / duration.TotalSeconds;
			AverageCallTime = duration;
		}

		public SpeedTestResult(string name, int calls, TimeSpan duration) {
			Name = name;
			Calls = calls;
			Duration = duration;
			CallsPerSecond = calls / duration.TotalSeconds;
			AverageCallTime = TimeSpan.FromSeconds(duration.TotalSeconds / calls);
			Console.WriteLine($"Duration: {duration.TotalMilliseconds:N2}");
		}

		public override string ToString() {
			return $"{Name}: {Calls} calls, {Duration.TotalMilliseconds:n3}ms";
		}
	}

	public class SpeedTester : IDisposable {
		private enum SpeedTestMode {
			Single,
			Looped,
			Timed,
		}

		private readonly SpeedTestMode mode;
		private readonly int loops;
		private readonly TimeSpan duration;

		private Timer timer;
		private volatile bool timerFinished;

		private SpeedTestCallback callback;
		private bool ignoreFirstAction;

		public SpeedTestCallback Callback {
			get => callback;
			set => callback = value;
		}

		public bool IgnoreFirstAction {
			get => ignoreFirstAction;
			set => ignoreFirstAction = value;
		}

		private void DefaultLoopCallback(SpeedTestResult r) {
			if (r.CallsPerMicrosecond >= 1)
				Console.WriteLine($"{r.Name}: calls per μs {r.CallsPerMicrosecond:n}");
			else if (r.CallsPerMillisecond >= 1)
				Console.WriteLine($"{r.Name}: calls per ms {r.CallsPerMillisecond:n}");
			else
				Console.WriteLine($"{r.Name}: calls per s {r.CallsPerSecond:n}");
		}

		private void DefaultTimedCallback(SpeedTestResult r) {
			if (r.CallsPerMicrosecond >= 1)
				Console.WriteLine($"{r.Name}: {r.Calls}, calls per μs {r.CallsPerMicrosecond:n}");
			else if (r.CallsPerMillisecond >= 1)
				Console.WriteLine($"{r.Name}: {r.Calls}, calls per ms {r.CallsPerMillisecond:n}");
			else
				Console.WriteLine($"{r.Name}: {r.Calls}, calls per s {r.CallsPerSecond:n}");
		}

		public SpeedTester() {
			mode = SpeedTestMode.Single;
			callback = DefaultLoopCallback;
		}

		public SpeedTester(int loops) : this() {
			if (loops <= 0)
				throw new ArgumentOutOfRangeException(nameof(loops));
			this.loops = loops;
			mode = SpeedTestMode.Looped;
			callback = DefaultLoopCallback;
		}

		public SpeedTester(TimeSpan duration) : this() {
			if (duration <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(duration));
			this.duration = duration;
			mode = SpeedTestMode.Timed;
			callback = DefaultTimedCallback;
		}

		public SpeedTestResult Run(string name, Action action) {
			if (ignoreFirstAction)
				action();
			SpeedTestResult results = default(SpeedTestResult);
			switch (mode) {
			case SpeedTestMode.Single: results = RunSingle(name, action); break;
			case SpeedTestMode.Looped: results = RunLooped(name, action); break;
			case SpeedTestMode.Timed: results = RunTimed(name, action); break;
			}
			callback?.Invoke(results);
			return default(SpeedTestResult);
		}

		private SpeedTestResult RunSingle(string name, Action action) {
			Stopwatch watch = Stopwatch.StartNew();
			action();
			return new SpeedTestResult(name, watch.Elapsed);
		}
		private SpeedTestResult RunLooped(string name, Action action) {
			Stopwatch watch = Stopwatch.StartNew();
			for (int i = 0; i < loops; i++) {
				action();
			}
			return new SpeedTestResult(name, loops, watch.Elapsed);
		}

		private SpeedTestResult RunTimed(string name, Action action) {
			timer?.Dispose();
			timerFinished = false;
			timer = new Timer(TimerCallback, null, TimeSpan.Zero, duration);
			Stopwatch watch = Stopwatch.StartNew();
			// We need to run this at least once if the user made the duration stupid-short
			int calls = 0;
			do {
				action();
				calls++;
			} while (!timerFinished);
			return new SpeedTestResult(name, calls, watch.Elapsed);
		}

		private void TimerCallback(object state) {
			timerFinished = true;
			timer?.Dispose();
			timer = null;
		}

		public void Dispose() {
			timerFinished = true;
			timer?.Dispose();
		}
	}
}
