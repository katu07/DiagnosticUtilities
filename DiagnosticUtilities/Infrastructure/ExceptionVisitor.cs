namespace DiagnosticUtilities.Infrastructure
{
	using System;

	public abstract class ExceptionVisitor
	{
		public int Depth { get; set; }

		public virtual void Visit(Exception exception)
		{
		}

		public virtual void VisitComplexException(Exception exception)
		{
		}

		public virtual void VisitInnerException(Exception exception)
		{
		}

		public virtual void VisitSiblingInnerException(Exception exception)
		{
		}

		public virtual void VisitRootCause(Exception exception)
		{
		}
	}
}