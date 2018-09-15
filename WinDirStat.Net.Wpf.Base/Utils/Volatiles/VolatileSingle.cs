using System;

namespace WinDirStat.Net.Utils {
	/// <summary>A mutable volatile <see cref="float"/> that supports compound assignments.</summary>
	public class VolatileSingle : Volatile<float> {

		#region Constructors

		/// <summary>Constructs the default <see cref="VolatileSingle"/>.</summary>
		public VolatileSingle() { }
		/// <summary>Constructs the <see cref="VolatileSingle"/> with the specified value.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		public VolatileSingle(float value) : base(value) { }
		/// <summary>Constructs the <see cref="VolatileSingle"/> with the specified lock.</summary>
		/// 
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileSingle(object lockObj) : base(lockObj) { }
		/// <summary>Constructs the <see cref="VolatileSingle"/> with the specified value and lock.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileSingle(float value, object lockObj) : base(value, lockObj) { }

		#endregion

		#region Volatile Operators

		/// <summary>Adds to the value.</summary>
		public float Add(float x) {
			lock (lockObj)
				return value += x;
		}
		/// <summary>Subtracts from the value.</summary>
		public float Sub(float x) {
			lock (lockObj)
				return value -= x;
		}
		/// <summary>Multiplies the value.</summary>
		public float Mul(float x) {
			lock (lockObj)
				return value *= x;
		}
		/// <summary>Divides the value.</summary>
		public float Div(float x) {
			lock (lockObj)
				return value /= x;
		}
		/// <summary>Negates the value.</summary>
		public float Negate() {
			lock (lockObj)
				return value = -value;
		}

		#endregion

		#region Volatile Floating Operators

		/// <summary>Floors the value.</summary>
		public float Floor() {
			lock (lockObj)
				return value = (float) Math.Floor(value);
		}
		/// <summary>Ceilings the value.</summary>
		public float Ceiling() {
			lock (lockObj)
				return value = (float) Math.Ceiling(value);
		}
		/// <summary>Rounds the value.</summary>
		public float Round() {
			lock (lockObj)
				return value = (float) Math.Round(value);
		}
		/// <summary>Truncates the value.</summary>
		public float Truncate() {
			lock (lockObj)
				return value = (float) Math.Truncate(value);
		}
		/// <summary>Squares the value.</summary>
		public float Sqr() {
			lock (lockObj)
				return value *= value;
		}
		/// <summary>Square roots the value.</summary>
		public float Sqrt() {
			lock (lockObj)
				return value = (float) Math.Sqrt(value);
		}
		/// <summary>Powers the value.</summary>
		public float Pow(float y) {
			lock (lockObj)
				return value = (float) Math.Pow(value, y);
		}
		/// <summary>Logs the value.</summary>
		public float Log(float newBase) {
			lock (lockObj)
				return value = (float) Math.Log(value, newBase);
		}

		#endregion
	}
}
