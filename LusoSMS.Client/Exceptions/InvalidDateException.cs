namespace LusoSMS.Client.Exceptions
{
    using LusoSMS.Client.Enums;

    public class InvalidDateException
        : LusoSMSException
    {
        public InvalidDateException()
            : base(ReturnMessages.InvalidDate)
        { }
    }
}