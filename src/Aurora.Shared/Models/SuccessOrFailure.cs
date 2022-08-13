namespace Aurora.Shared.Models
{
    public struct SuccessOrFailure
    {
        public bool IsSuccessfull { get; set; }
        public bool IsFailure => !IsSuccessfull;

        public string? FailureMessage { get; set; }

        public static SuccessOrFailure CreateSuccess()
        {
            return new SuccessOrFailure
            {
                IsSuccessfull = true
            };
        }

        public static SuccessOrFailure CreateFailure(string? failureMessage = null)
        {
            return new SuccessOrFailure
            {
                IsSuccessfull = false,
                FailureMessage = failureMessage
            };
        }

        public static implicit operator SuccessOrFailure(bool flag)
        {
            if (flag == false)
            {
                return CreateFailure();
            }
            return CreateSuccess();
        }
    }
}
