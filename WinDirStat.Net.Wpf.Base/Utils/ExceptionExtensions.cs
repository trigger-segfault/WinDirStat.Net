using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinDirStat.Net.Utils {
	/// <summary>A static class for extensions revolving around exceptions.</summary>
	public static class ExceptionExtensions {

		/// <summary>Gets all inner exceptions within this exception.</summary>
		/// 
		/// <param name="ex">The exception to get the inner exceptions of.</param>
		/// <returns>The collection of inner exceptions.</returns>
		public static IEnumerable<Exception> GetInnerExceptions(this Exception ex) {
			ex = ex.InnerException;
			while (ex != null) {
				yield return ex;
				ex = ex.InnerException;
			}
		}

		/// <summary>Returns a string with every exception message and inner message.</summary>
		/// 
		/// <param name="ex">The exception to get the inner messages of.</param>
		/// <param name="verbose">If true, exceptions will be prefixed with thir type.</param>
		/// <returns>A string with the message from every exception.</returns>
		public static string MessageWithInner(this Exception ex, bool verbose = false) {
			StringBuilder builder = new StringBuilder();
			if (verbose)
				builder.Append($"{ex.GetType().Name}: ");
			builder.Append(ex.Message);
			
			IEnumerable<Exception> innerExceptions = ex.GetInnerExceptions();

			// Label the beginning of inner exceptions
			if (innerExceptions.Any()) {
				builder.AppendLine();
				// Length = 64
				if (verbose)
					builder.AppendLine("----------------------------------------------------------------");
				builder.Append("Inner Exceptions: ");
			}

			// Append each inner exception
			int index = 0;
			foreach (Exception inner in innerExceptions) {
				builder.AppendLine();
				if (verbose)
					builder.Append($"{inner.GetType().Name} ({index}): ");
				else
					builder.Append($"{index}: ");
				builder.Append(inner.Message);
				index++;
			}
			return builder.ToString();
		}

		/// <summary>Converts the exception to a string along with every inner exception.</summary>
		/// 
		/// <param name="ex">The exception to get the inner <see cref="Exception.ToString"/> of.</param>
		/// <returns>A string with the output from every exception.</returns>
		public static string ToStringWithInner(this Exception ex) {
			StringBuilder builder = new StringBuilder();
			builder.Append(ex);

			// Append each inner exception
			int index = 0;
			foreach (Exception inner in ex.GetInnerExceptions()) {
				builder.AppendLine();
				// Length = 64
				builder.AppendLine("----------------------------------------------------------------");
				builder.Append($"Inner {index}: {inner}");
				index++;
			}
			return builder.ToString();
		}
	}
}
