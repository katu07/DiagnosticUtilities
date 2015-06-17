namespace DiagnosticUtilities.Infrastructure
{
	using System;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;

	public class DetailedStringExceptionVisitor : ExceptionVisitor
	{
		private const string Tab = "   ";

		private readonly bool includeStackTrace;
		private readonly string cleanseCandidate;
		private readonly StringBuilder stringBuilder;

		private int siblingInnerExceptionIndex = 0;

		public DetailedStringExceptionVisitor(bool includeStackTrace = true, string cleanseCandidate = null)
		{
			this.includeStackTrace = includeStackTrace;
			this.cleanseCandidate = cleanseCandidate;
			this.stringBuilder = new StringBuilder();
		}

		public string DetailedString
		{
			get { return this.stringBuilder.ToString(); }
		}

		protected string Padding
		{
			get { return string.Concat(Enumerable.Repeat(Tab, this.Depth)); }
		}

		public override void Visit(Exception exception)
		{
			this.stringBuilder.AppendFormat("[{0}] ", exception.GetType());
			this.stringBuilder.Append(this.Cleanse(exception.Message));

			if (this.includeStackTrace && exception.StackTrace != null)
			{
				string[] methods = exception.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
				foreach (string method in methods)
				{
					this.stringBuilder.AppendLine();
					this.stringBuilder.Append(this.Padding);
					this.stringBuilder.Append(this.Cleanse(method));
				}
			}
		}

		public override void VisitComplexException(Exception exception)
		{
			// reset our counter
			this.siblingInnerExceptionIndex = 0;
		}

		public override void VisitInnerException(Exception exception)
		{
			this.PrepareForInnerException();
			this.stringBuilder.Append("CAUSED BY: ");
		}

		public override void VisitSiblingInnerException(Exception exception)
		{
			this.PrepareForInnerException();
			this.stringBuilder.Append("CAUSE #");
			this.stringBuilder.Append(++this.siblingInnerExceptionIndex);
			this.stringBuilder.Append(": ");
		}

		public override void VisitRootCause(Exception exception)
		{
			this.stringBuilder.Append("[ROOT] ");
		}

		private void PrepareForInnerException()
		{
			if (this.includeStackTrace)
			{
				// a little extra space to help separate exceptions
				this.stringBuilder.AppendLine();
			}

			this.stringBuilder.AppendLine();
			this.stringBuilder.Append(this.Padding);
		}

		private string Cleanse(string value)
		{
			if (value == null)
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(this.cleanseCandidate))
			{
				return value;
			}

			string cleansed = Regex.Replace(this.cleanseCandidate, @"[a-zA-Z0-9]", "x");
			return value.Replace(this.cleanseCandidate, cleansed);
		}
	}
}