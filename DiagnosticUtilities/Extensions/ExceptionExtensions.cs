namespace DiagnosticUtilities.Extensions
{
	using System;

	using DiagnosticUtilities.Infrastructure;

	public static class ExceptionExtensions
	{
		/// <summary>
		/// Outputs all inner exception messages and stack traces.
		/// </summary>
		public static string ToDetailedString(this Exception exception)
		{
			var visitor = new DetailedStringExceptionVisitor();
			new ExceptionTree(exception).Accept(visitor);
			return visitor.DetailedString;
		}

		/// <summary>
		/// Outputs all inner exception messages and stack traces while replacing a secrect string with Xs.
		/// </summary>
		public static string ToCleansedDetailedString(this Exception exception, string cleanseCandidate)
		{
			var visitor = new DetailedStringExceptionVisitor(cleanseCandidate: cleanseCandidate);
			new ExceptionTree(exception).Accept(visitor);
			return visitor.DetailedString;
		}

		/// <summary>
		/// Outputs all inner exception messages.
		/// </summary>
		public static string ToSimpleString(this Exception exception)
		{
			var visitor = new DetailedStringExceptionVisitor(includeStackTrace: false);
			new ExceptionTree(exception).Accept(visitor);
			return visitor.DetailedString;
		}

		/// <summary>
		/// Outputs messages of innermost exceptions only (aka root causes).
		/// </summary>
		public static string ToRootCauseSimpleString(this Exception exception)
		{
			var visitor = new RootCauseExceptionVisitor();
			new ExceptionTree(exception).Accept(visitor);
			return visitor.RootCauseMessage;
		}
	}
}