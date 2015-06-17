namespace DiagnosticUtilities.Infrastructure
{
	using System;
	using System.Text;

	public class RootCauseExceptionVisitor : ExceptionVisitor
	{
		private readonly StringBuilder stringBuilder;

		public RootCauseExceptionVisitor()
		{
			this.stringBuilder = new StringBuilder();
		}

		public string RootCauseMessage
		{
			get { return this.stringBuilder.ToString(); }
		}

		public override void VisitRootCause(Exception exception)
		{
			if (this.stringBuilder.Length > 0)
			{
				this.stringBuilder.Append(" ");
			}

			this.stringBuilder.Append(exception.Message);
		}
	}
}