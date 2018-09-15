using System;

namespace WinDirStat.Net.Utils {
	/// <summary>A mutable volatile <see cref="double"/> that supports compound assignments.</summary>
	public class VolatileDouble : Volatile<double> {
		
		#region Constructors

		/// <summary>Constructs the default <see cref="VolatileDouble"/>.</summary>
		public VolatileDouble() { }
		/// <summary>Constructs the <see cref="VolatileDouble"/> with the specified value.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		public VolatileDouble(double value) : base(value) { }
		/// <summary>Constructs the <see cref="VolatileDouble"/> with the specified lock.</summary>
		/// 
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileDouble(object lockObj) : base(lockObj) { }
		/// <summary>Constructs the <see cref="VolatileDouble"/> with the specified value and lock.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileDouble(double value, object lockObj) : base(value, lockObj) { }

		#endregion
		
		#region Volatile Operators
		
		/// <summary>Adds to the value.</summary>
		public double Add(double x) {
			lock (lockObj)
				return value += x;
		}
		/// <summary>Subtracts from the value.</summary>
		public double Sub(double x) {
			lock (lockObj)
				return value -= x;
		}
		/// <summary>Multiplies the value.</summary>
		public double Mul(double x) {
			lock (lockObj)
				return value *= x;
		}
		/// <summary>Divides the value.</summary>
		public double Div(double x) {
			lock (lockObj)
				return value /= x;
		}
		/// <summary>Negates the value.</summary>
		public double Negate() {
			lock (lockObj)
				return value = -value;
		}

		#endregion

		#region Volatile Floating Operators

		/// <summary>Floors the value.</summary>
		public double Floor() {
			lock (lockObj)
				return value = (double) Math.Floor(value);
		}
		/// <summary>Ceilings the value.</summary>
		public double Ceiling() {
			lock (lockObj)
				return value = (double) Math.Ceiling(value);
		}
		/// <summary>Rounds the value.</summary>
		public double Round() {
			lock (lockObj)
				return value = (double) Math.Round(value);
		}
		/// <summary>Truncates the value.</summary>
		public double Truncate() {
			lock (lockObj)
				return value = (double) Math.Truncate(value);
		}
		/// <summary>Squares the value.</summary>
		public double Sqr() {
			lock (lockObj)
				return value *= value;
		}
		/// <summary>Square roots the value.</summary>
		public double Sqrt() {
			lock (lockObj)
				return value = (double) Math.Sqrt(value);
		}
		/// <summary>Powers the value.</summary>
		public double Pow(double y) {
			lock (lockObj)
				return value = (double) Math.Pow(value, y);
		}
		/// <summary>Logs the value.</summary>
		public double Log(double newBase) {
			lock (lockObj)
				return value = (double) Math.Log(value, newBase);
		}

		#endregion
	}
}
