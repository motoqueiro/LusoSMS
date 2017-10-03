namespace LusoSMS.Client.Exceptions
{
    using LusoSMS.Client.Enums;

    public class ExceededCharactersException
        : LusoSMSException
    {
        public ExceededCharactersException()
            : base(ReturnMessages.ExceededCharacters)
        { }
    }
}