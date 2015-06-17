namespace DiagnosticUtilities.UnitTests
{
	using System;
	using System.Reflection;

	using DiagnosticUtilities.Extensions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ExceptionExtensionsTests
	{
		private readonly Exception exceptionWithoutStack;
		private readonly Exception exceptionWithStack;
		private readonly string constructorStackTrace;

		public ExceptionExtensionsTests()
		{
			this.constructorStackTrace = this.GetType().FullName + "..ctor()";
			this.exceptionWithoutStack = new Exception("foo");

			try
			{
				throw new Exception("bar");
			}
			catch (Exception ex)
			{
				this.exceptionWithStack = ex;
			}
		}

		[TestMethod]
		public void ToDetailedString_ReturnsMessage_WhenOneExceptionWithNoStack()
		{
			// arrange
			// act
			string output = this.exceptionWithoutStack.ToDetailedString();

			// assert
			Assert.AreEqual("[ROOT] [System.Exception] foo", output);
		}

		[TestMethod]
		public void ToDetailedString_ReturnsMessageAndStack_WhenOneException()
		{
			// arrange
			// act
			string output = this.exceptionWithStack.ToDetailedString();

			// assert
			Assert.IsTrue(output.StartsWith(this.InsertConstructorStackTrace(
@"[ROOT] [System.Exception] bar
   at {0} in ")));
		}

		[TestMethod]
		public void ToDetailedString_ReturnsInnerMessage_WhenThereIsInnerExceptionWithNoStack()
		{
			// arrange
			var input = new Exception("foo", new ArgumentException("bar"));

			// act
			string output = input.ToDetailedString();

			// assert
			Assert.AreEqual(
@"[System.Exception] foo

   CAUSED BY: [ROOT] [System.ArgumentException] bar", output);
		}

		[TestMethod]
		public void ToDetailedString_ReturnsInnerMessageAndStack_WhenThereIsInnerException()
		{
			// arrange
			var input = new Exception("foo", this.exceptionWithStack);

			// act
			string output = input.ToDetailedString();

			// assert
			Assert.IsTrue(output.StartsWith(this.InsertConstructorStackTrace(
@"[System.Exception] foo

   CAUSED BY: [ROOT] [System.Exception] bar
      at {0} in ")));
		}

		[TestMethod]
		public void ToDetailedString_ReturnsAllAggregateExceptions_WhenTopLevelIsAggregate()
		{
			// arrange
			var input = new AggregateException("foo", new Exception("bar1"), new Exception("bar2"));

			// act
			string output = input.ToDetailedString();

			// assert
			Assert.AreEqual(
@"[System.AggregateException] foo

   CAUSE #1: [ROOT] [System.Exception] bar1

   CAUSE #2: [ROOT] [System.Exception] bar2", output);
		}

		[TestMethod]
		public void ToDetailedString_ReturnsAllAggregateExceptionStacks_WhenInnerHaveStacks()
		{
			// arrange
			var input = new AggregateException("foo", new Exception("bar0", this.exceptionWithStack), this.exceptionWithStack);

			// act
			string output = input.ToDetailedString();

			// assert
			Assert.IsTrue(output.StartsWith(this.InsertConstructorStackTrace(
@"[System.AggregateException] foo

   CAUSE #1: [System.Exception] bar0

      CAUSED BY: [ROOT] [System.Exception] bar
         at {0} in ")));
			Assert.IsTrue(output.Contains(this.InsertConstructorStackTrace(
@"   CAUSE #2: [ROOT] [System.Exception] bar
      at {0} in ")));
		}

		[TestMethod]
		public void ToDetailedString_ReturnsAllLoaderExceptions_WhenInnerIsReflectionTypeLoadException()
		{
			// arrange
			var input = new Exception(
				"foo",
				new ReflectionTypeLoadException(
					new[] { typeof(string), typeof(int) },
					new[] { this.exceptionWithoutStack, this.exceptionWithStack }));

			// act
			string output = input.ToDetailedString();

			// assert
			Assert.IsTrue(output.StartsWith(this.InsertConstructorStackTrace(
@"[System.Exception] foo

   CAUSED BY: [ROOT] [System.Reflection.ReflectionTypeLoadException] Exception of type 'System.Reflection.ReflectionTypeLoadException' was thrown.

      CAUSE #1: [ROOT] [System.Exception] foo

      CAUSE #2: [ROOT] [System.Exception] bar
         at {0} in ")));
		}

		[TestMethod]
		public void ToCleansedDetailedString_ReplacesSecret_WhenExceptionMessageContainsIt()
		{
			// arrange
			var input = new Exception("foo secret bar");

			// act
			string output = input.ToCleansedDetailedString("secret");

			// assert
			Assert.AreEqual("[ROOT] [System.Exception] foo xxxxxx bar", output);
		}

		[TestMethod]
		public void ToCleansedDetailedString_ReturnsMessageVerbatim_WhenItHasNoSecret()
		{
			// arrange
			var input = new Exception("foo bar");

			// act
			string output = input.ToCleansedDetailedString("secret");

			// assert
			Assert.AreEqual("[ROOT] [System.Exception] foo bar", output);
		}

		[TestMethod]
		public void ToCleansedDetailedString_HidesSecretInStackTrace_WhenItContainsSecret()
		{
			// arrange
			// act
			string output = this.exceptionWithStack.ToCleansedDetailedString(this.constructorStackTrace);

			// assert
			Assert.IsTrue(output.StartsWith(
@"[ROOT] [System.Exception] bar
   at xxxxxxx"));
		}

		[TestMethod]
		public void ToSimpleString_ReturnsMessage_WhenExceptionHasStack()
		{
			// arrange
			// act
			string output = this.exceptionWithStack.ToSimpleString();

			// assert
			Assert.AreEqual("[ROOT] [System.Exception] bar", output);
		}

		[TestMethod]
		public void ToRootCauseSimpleString_ReturnsMessage_WhenOneException()
		{
			// arrange
			// act
			string output = this.exceptionWithStack.ToRootCauseSimpleString();

			// assert
			Assert.AreEqual("bar", output);
		}

		[TestMethod]
		public void ToRootCauseSimpleString_ReturnsInnermostMessage_WhenInnerException()
		{
			// arrange
			var input = new Exception("foo", new ArgumentException("bar"));

			// act
			string output = input.ToRootCauseSimpleString();

			// assert
			Assert.AreEqual("bar", output);
		}

		[TestMethod]
		public void ToRootCauseSimpleString_ReturnsAllSiblingRootMessages_WhenReflectionTypeLoadException()
		{
			// arrange
			var input = new Exception(
				"foo",
				new ReflectionTypeLoadException(
					new[] { typeof(string), typeof(int) },
					new[] { this.exceptionWithoutStack, this.exceptionWithStack }));

			// act
			string output = input.ToRootCauseSimpleString();

			// assert
			Assert.AreEqual("Exception of type 'System.Reflection.ReflectionTypeLoadException' was thrown. foo bar", output);
		}

		/// <summary>
		/// Expects one format argument for the stack trace.
		/// </summary>
		private string InsertConstructorStackTrace(string message)
		{
			return string.Format(message, this.constructorStackTrace);
		}
	}
}