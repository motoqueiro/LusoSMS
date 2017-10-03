namespace LusoSMS.Client.Exceptions
{
    using System;

    public abstract class LusoSMSException
        : Exception
    {
        public string ReturnMessage { get; private set; }

        public LusoSMSException(string returnMessage)
        {
            this.ReturnMessage = returnMessage;
        }
    }
}