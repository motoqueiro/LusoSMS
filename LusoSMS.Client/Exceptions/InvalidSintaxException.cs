namespace LusoSMS.Client.Exceptions
{
    using LusoSMS.Client.Enums;

    public class InvalidSintaxException
        : LusoSMSException
    {
        public InvalidSintaxException()
            : base(ReturnMessages.InvalidSintax)
        { }
    }
}