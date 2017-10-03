namespace LusoSMS.Client.Exceptions
{
    using LusoSMS.Client.Enums;

    public class CommunicationErrorException
        : LusoSMSException
    {
        public CommunicationErrorException()
            : base(ReturnMessages.CommunicationError)
        { }
    }
}