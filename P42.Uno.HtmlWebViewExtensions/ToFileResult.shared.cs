using System;

namespace P42.Uno.HtmlWebViewExtensions
{
    /// <summary>
    /// Result from ToPng and ToPdf processes
    /// </summary>
    public class ToFileResult
    {
        /// <summary>
        /// Flags if the Result is an error;
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Either success or error result
        /// </summary>
		public string Result
        {
            get;
            private set;
        }

        /// <summary>
        /// Html to PNG result
        /// </summary>
        /// <param name="isError"></param>
        /// <param name="result"></param>
		public ToFileResult(bool isError, string result)
        {
            IsError = isError;
            Result = result;
        }
    }
}
