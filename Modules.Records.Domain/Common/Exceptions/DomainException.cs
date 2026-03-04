namespace Modules.Records.Domain.Common.Exceptions
{
    public class DomainException : Exception
    {
        public string Code { get; }

        public DomainException(string code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}
