namespace LusoSMS.Client.Exceptions
{
    using LusoSMS.Client.Enums;

    public class InsufficientCreditsException
        : LusoSMSException
    {
        public InsufficientCreditsException()
            : base(ReturnMessages.InsufficientCredit)
        { }
    }
}