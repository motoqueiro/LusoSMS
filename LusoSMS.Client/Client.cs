namespace LusoSMS.Client
{
    using Flurl;
    using Flurl.Http;
    using System;
    using System.Threading.Tasks;

    public class Client
    {
        public const string BaseUrl = "http://www.lusosms.com";

        private readonly string _username;

        private readonly string _password;

        public Client(string username, string password)
        {
            this._username = username;
            this._password = password;
        }

        public async Task<double> CheckCredit()
        {
            var result = await Client.BaseUrl
                .AppendPathSegment("ver_credito_get.php")
                .SetQueryParam("username", this._username)
                .SetQueryParam("password", this._password)
                .GetStringAsync();

            if (double.TryParse(result, out var credits))
            {
                return credits;
            }

            throw new Exception(result);
        }

        public async Task SendSms(
            string message,
            string origin,
            string destination,
            bool longMessage = false,
            SmsTypeEnum type = SmsTypeEnum.Normal,
            SmsMethodEnum method = SmsMethodEnum.POST)
        {
            if ((!longMessage && message.Length > 155) ||
                (longMessage && message.Length > 300))
            {
                throw new Exception("caracteres_excedidos");
            }

            var escapedMessage = message.Replace(' ', '+');

            var longMessageRaw = this.ResolveLongMessage(longMessage);
            var rawType = this.ResolveMessageType(type);
            string result;
            switch (method)
            {
                case SmsMethodEnum.POST:
                    result = await this.PostSms(
                        escapedMessage,
                        origin,
                        destination,
                        longMessageRaw,
                        rawType);
                    break;

                case SmsMethodEnum.GET:
                    result = await this.GetSms(
                        escapedMessage,
                        origin,
                        destination,
                        longMessageRaw,
                        rawType);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }

            if (result != "mensagem_enviada")
            {
                throw new Exception(result);
            }
        }

        private string ResolveMessageType(SmsTypeEnum type)
        {
            return type == SmsTypeEnum.Flash ? "flash" : "normal";
        }

        private ushort ResolveLongMessage(bool longMessage)
        {
            return longMessage ? (ushort)1 : (ushort)0;
        }

        private async Task<string> GetSms(
                            string message,
            string origin,
            string destination,
            ushort longMessage,
            string type)
        {
            return await Client.BaseUrl
                .AppendPathSegment("enviar_sms_get.php")
                .SetQueryParam("username", this._username)
                .SetQueryParam("password", this._password)
                .SetQueryParam("origem", origin)
                .SetQueryParam("destino", destination)
                .SetQueryParam("mensagem", message)
                .SetQueryParam("mensagemlonga", longMessage)
                .SetQueryParam("tipo", type)
                .GetStringAsync();
        }

        private async Task<string> PostSms(
            string message,
            string origin,
            string destination,
            ushort longMessage,
            string type)
        {
            return await Client.BaseUrl
                .AppendPathSegment("enviar_sms_post.php")
                .PostUrlEncodedAsync(new
                {
                    username = this._username,
                    password = this._password,
                    origem = origin,
                    destino = destination,
                    mensagem = message,
                    mensagemlonga = longMessage,
                    tipo = type
                })
                .ReceiveString();
        }
    }
}