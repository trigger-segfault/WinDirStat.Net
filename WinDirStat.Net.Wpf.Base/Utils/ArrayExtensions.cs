using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TriggersTools.SharpUtils.Collections {
	/// <summary>How to handle BlockCast when the array byte sizes do not align.</summary>
	public enum BlockCastAlign {
		/// <summary>Throw an exception due to unalignment.</summary>
		Error = 0,
		/// <summary>Oversize the new array by one element.</summary>
		Oversize,
		/// <summary>Undersize the new array by one element.</summary>
		Undersize,
	}

	/// <summary>Extensions for 1-dimensional <see cref="Array"/> objects.</summary>
	public static class ArrayExtensions {

		#region Contains

		/// <summary>Returns true if the array contains the specified value.</summary>
		/// <param name="array">The array to check for containment.</param>
		/// <param name="value">The value to check for.</param>
		/// <returns>Returns true if the array contains the specified value.</returns>
		public static bool Contains(this Array array, object value) {
			return Array.IndexOf(array, value) != -1;
		}

		/// <summary>Returns true if the array contains the specified value.</summary>
		/// <typeparam name="T">The element type of the array.</typeparam>
		/// <param name="array">The array to check for containment.</param>
		/// <param name="value">The value to check for.</param>
		/// <returns>Returns true if the array contains the specified value.</returns>
		public static bool Contains<T>(this T[] array, T value) {
			return Array.IndexOf(array, value) != -1;
		}

		#endregion

		#region BlockCast

		/// <summary>
		/// Block copies the source array to a new array of primitive type <typeparamref name="TResult"/>.
		/// </summary>
		/// 
		/// <typeparam name="TResult">The primitive type to cast to.</typeparam>
		/// <param name="source">The array to block cast.</param>
		/// <param name="align">How alignment remainders are handled.</param>
		/// <returns>Return a new array with the casted type.</returns>
		/// 
		/// <exception cref="ArgumentException">
		/// <paramref name="source"/> is not an array of primitives, <typeparamref name="TResult"/>
		/// is not a primitive type, or <paramref name="align"/> is invalid.
		/// </exception>
		/// <exception cref="OverflowException">
		/// The length of <paramref name="source"/> is greater than <see cref="int.MaxValue"/>.
		/// </exception>
		public static TResult[] BlockCast<TResult>(this Array source,
			BlockCastAlign align = BlockCastAlign.Error) where TResult : struct
		{
			Type srcType = source.GetType().GetElementType();
			Type resultType = typeof(TResult);
			if (!srcType.IsPrimitive)
				throw new ArgumentException($"{nameof(source)} is not an array of " +
					$"primitives!", nameof(source));
			else if (!resultType.IsPrimitive)
				throw new ArgumentException($"{nameof(TResult)} '{resultType.Name}' " +
					$"is not a primitive type!", nameof(TResult));

			int srcSize = Marshal.SizeOf(srcType);
			int srcByteLength = source.Length * srcSize;

			int resultSize = Marshal.SizeOf<TResult>();
			int resultLength = Math.DivRem(srcByteLength, resultSize, out int rem);
			int copyByteLength = srcByteLength;
			if (rem != 0) {
				switch (align) {
				case BlockCastAlign.Oversize:
					resultLength += resultSize;
					break;
				case BlockCastAlign.Undersize:
					copyByteLength = resultLength * resultSize;
					break;
				case BlockCastAlign.Error:
					throw new ArgumentException($"{srcType.Name}[] of length " +
						$"'{source.Length}' cannot align with {resultType.Name}[] " +
						$"of length '{resultLength}`!", nameof(source));
				default:
					throw new ArgumentException($"{nameof(align)} is invalid!",
						nameof(align));
				}
			}

			TResult[] result = new TResult[resultLength];
			Buffer.BlockCopy(source, 0, result, 0, copyByteLength);
			return result;
		}

		#endregion

		#region Reverse

		/// <summary>Creates a reversed copy the array.</summary>
		/// 
		/// <param name="array">The arrat to copy.</param>
		/// <returns>A reversed array.</returns>
		/// 
		/// <exception cref="RankException"><paramref name="array"/> is multidimensional.</exception>
		public static Array Reverse(this Array array) {
			Array output = (Array) array.Clone();
			Array.Reverse(output);
			return output;
		}

		/// <summary>Creates a reversed copy the array.</summary>
		/// 
		/// <typeparam name="T">The element type of the array.</typeparam>
		/// <param name="array">The arrat to copy.</param>
		/// <returns>A reversed array.</returns>
		public static T[] Reverse<T>(this T[] array) {
			T[] output = (T[]) array.Clone();
			Array.Reverse(output);
			return output;
		}

		#endregion

		#region Swap

		/// <summary>Swaps two elements within an array.</summary>
		/// 
		/// <param name="array">The array to swap elements in.</param>
		/// <param name="indexA">The index of the first element to swap.</param>
		/// <param name="indexB">The index of the second element to swap.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="indexA"/> or <paramref name="indexB"/> is outside the bounds of the array.
		/// </exception>
		public static void Swap(this Array array, int indexA, int indexB) {
			object swap = array.GetValue(indexA);
			array.SetValue(array.GetValue(indexB), indexA);
			array.SetValue(swap, indexB);
		}

		/// <summary>Swaps two elements within an array.</summary>
		/// 
		/// <typeparam name="T">The element type of the array.</typeparam>
		/// <param name="array">The array to swap elements in.</param>
		/// <param name="indexA">The index of the first element to swap.</param>
		/// <param name="indexB">The index of the second element to swap.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="indexA"/> or <paramref name="indexB"/> is outside the bounds of the array.
		/// </exception>
		public static void Swap<T>(this T[] array, int indexA, int indexB) {
			T swap = array[indexA];
			array[indexA] = array[indexB];
			array[indexB] = swap;
		}

		#endregion

		#region ToJagged
		
		/// <summary>Copies a 2D multidimension array to a 2D jagged array.</summary>
		/// 
		/// <typeparam name="T">The type of the array.</typeparam>
		/// <param name="array">The array to create a jagged version of.</param>
		/// <param name="reverse">
		/// True if the first dimension in the multidimension array should be the last dimension in the
		/// jagged array.
		/// </param>
		/// <returns>The new jagged array.</returns>
		public static T[][] ToJagged<T>(this T[,] array, bool reverse) {
			T[][] output;
			int lengthx, lengthy;
			if (reverse) {
				lengthx = array.GetLength(1);
				lengthy = array.GetLength(0);
				output = new T[lengthx][];
				for (int x = 0; x < lengthx; x++) {
					output[x] = new T[lengthy];
					for (int y = 0; y < lengthy; y++)
						output[x][y] = array[y, x];
				}
			}
			else {
				lengthx = array.GetLength(0);
				lengthy = array.GetLength(1);
				output = new T[lengthx][];
				for (int x = 0; x < lengthx; x++) {
					output[x] = new T[lengthy];
					for (int y = 0; y < lengthy; y++)
						output[x][y] = array[x, y];
				}
			}
			return output;
		}

		/// <summary>Copies a 3D multidimension array to a 3D jagged array.</summary>
		/// 
		/// <typeparam name="T">The type of the array.</typeparam>
		/// <param name="array">The array to create a jagged version of.</param>
		/// <param name="reverse">
		/// True if the first dimension in the multidimension array should be the last dimension in the
		/// jagged array.
		/// </param>
		/// <returns>The new jagged array.</returns>
		public static T[][][] ToJagged<T>(this T[,,] array, bool reverse) {
			T[][][] output;
			int lengthx, lengthy, lengthz;
			if (reverse) {
				lengthx = array.GetLength(2);
				lengthy = array.GetLength(1);
				lengthz = array.GetLength(0);
				output = new T[lengthx][][];
				for (int x = 0; x < lengthx; x++) {
					output[x] = new T[lengthy][];
					for (int y = 0; y < lengthy; y++) {
						output[x][y] = new T[lengthz];
						for (int z = 0; z < lengthz; z++)
							output[x][y][z] = array[z, y, x];
					}
				}
			}
			else {
				lengthx = array.GetLength(0);
				lengthy = array.GetLength(1);
				lengthz = array.GetLength(2);
				output = new T[lengthx][][];
				for (int x = 0; x < lengthx; x++) {
					output[x] = new T[lengthy][];
					for (int y = 0; y < lengthy; y++) {
						output[x][y] = new T[lengthz];
						for (int z = 0; z < lengthz; z++)
							output[x][y][z] = array[x, y, z];
					}
				}
			}
			return output;
		}

		#endregion

		#region ToMultidimension

		/// <summary>Copies a 2D jagged array to a 2D multidimension array.</summary>
		/// 
		/// <typeparam name="T">The type of the array.</typeparam>
		/// <param name="array">The array to create a jagged version of.</param>
		/// <param name="reverse">
		/// True if the first dimension in the jagged array should be the last dimension in the
		/// multidimension array.
		/// </param>
		/// <returns>The new multidimension array.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// One of the dimensions in <paramref name="array"/> is null.
		/// </exception>
		public static T[,] ToMultidimension<T>(this T[][] array, bool reverse) {
			T[,] output;
			int lengthx = array.Length;
			int lengthy = array.FirstOrDefault()?.Length ?? 0;
			if (reverse) {
				output = new T[lengthy, lengthx];
				for (int x = 0; x < lengthx; x++) {
					LengthCheck(array, x, lengthy, 1);
					for (int y = 0; y < lengthy; y++)
						output[y, x] = array[x][y];
				}
			}
			else {
				output = new T[lengthx, lengthy];
				for (int x = 0; x < lengthx; x++) {
					LengthCheck(array, x, lengthy, 1);
					for (int y = 0; y < lengthy; y++)
						output[x, y] = array[x][y];
				}
			}
			return output;
		}

		/// <summary>Copies a 3D jagged array to a 3D multidimension array.</summary>
		/// 
		/// <typeparam name="T">The type of the array.</typeparam>
		/// <param name="array">The array to create a jagged version of.</param>
		/// <param name="reverse">
		/// True if the first dimension in the jagged array should be the last dimension in the
		/// multidimension array.
		/// </param>
		/// <returns>The new multidimension array.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		/// One of the dimensions in <paramref name="array"/> is null.
		/// </exception>
		public static T[,,] ToMultidimension<T>(this T[][][] array, bool reverse) {
			T[,,] output;
			int lengthx = array.Length;
			int lengthy = array.FirstOrDefault()?.Length ?? 0;
			int lengthz = array.FirstOrDefault()?.FirstOrDefault()?.Length ?? 0;
			if (reverse) {
				output = new T[lengthz, lengthy, lengthx];
				for (int x = 0; x < lengthx; x++) {
					LengthCheck(array, x, lengthy, 1);
					for (int y = 0; y < lengthy; y++) {
						LengthCheck(array, y, lengthz, 1);
						for (int z = 0; z < lengthz; y++)
							output[z, y, x] = array[x][y][z];
					}
				}
			}
			else {
				output = new T[lengthx, lengthy, lengthz];
				for (int x = 0; x < lengthx; x++) {
					LengthCheck(array, x, lengthy, 1);
					for (int y = 0; y < lengthy; y++) {
						LengthCheck(array, y, lengthz, 1);
						for (int z = 0; z < lengthz; y++)
							output[x, y, z] = array[x][y][z];
					}
				}
			}
			return output;
		}

		#endregion

		#region Private
		
		/// <summary>Ensures the jagged array conforms to the same length for every index.</summary>
		/// 
		/// <typeparam name="T">The element type of the array.</typeparam>
		/// <param name="array">The array to check the length of.</param>
		/// <param name="index">The index of the array to check the length of.</param>
		/// <param name="length">The initial length of the array.</param>
		/// <param name="depth">The current pdeth of teh array.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void LengthCheck<T>(T[][] array, int index, int length, int depth) {
			if (array[index].Length != length) {
				throw new ArgumentException($"Jagged array of depth: {depth}, index: {index}, " +
					$"length: {array[index].Length} does not match initial length of {length}!");
			}
		}

		#endregion
	}
}
