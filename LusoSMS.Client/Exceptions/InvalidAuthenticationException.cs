namespace LusoSMS.Client.Exceptions
{
    using LusoSMS.Client.Enums;

    public class InvalidAuthenticationException
        : LusoSMSException
    {
        public InvalidAuthenticationException()
            : base(ReturnMessages.InvalidAuthentication)
        { }
    }
}