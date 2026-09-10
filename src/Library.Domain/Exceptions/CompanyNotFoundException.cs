namespace Library.Domain.Exceptions
{
    public class CompanyNotFoundException : Exception
    {
        public Guid? CompanyId { get; private set; }

        public CompanyNotFoundException() : base() { }

        public CompanyNotFoundException(string? message) : base(message) { }

        public CompanyNotFoundException(string message, Exception innerException):
            base(message, innerException) { }

        public CompanyNotFoundException(Guid companyId)
        : base($"Company with id '{companyId}' was not found.")
        {
            CompanyId = companyId;
        }
    }
}
