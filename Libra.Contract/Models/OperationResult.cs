using System.Collections.Generic;
using System.Linq;

namespace Libra.Contract
{
    public class OperationResult
    {
        public OperationResult()
        {
            Issues = new List<Issue>();
        }

        public OperationResult(string message, IssueSeverity severity)
        {
            Issues = new List<Issue>
            {
                new Issue(message, severity)
            };
        }

        public ICollection<Issue> Issues { get; }

        public bool IsSuccess => Issues.All(i => !i.IsError);
    }

    public class OperationResult<TModel> : OperationResult
    {
        public OperationResult()
        {
        }

        public OperationResult(string message, IssueSeverity severity)
            : base(message, severity)
        {
        }

        public OperationResult(TModel model)
        {
            Model = model;
        }

        public TModel Model { get; set; }
    }

    public struct Issue
    {
        private readonly IssueSeverity severity;

        public Issue(string message, IssueSeverity severity)
        {
            Message = message;
            this.severity = severity;
        }

        public string Message { get; }
        public string Severity => severity.ToString().ToLowerInvariant();
        public bool IsSuccess => severity == IssueSeverity.Success;
        public bool IsInfo => severity == IssueSeverity.Info;
        public bool IsWarning => severity == IssueSeverity.Warning;
        public bool IsError => severity == IssueSeverity.Error;
    }

    public enum IssueSeverity
    {
        Success = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
}