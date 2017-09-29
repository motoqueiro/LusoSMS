namespace LusoSMS.Client.UnitTests
{
    using Flurl.Http.Testing;
    using SimpleFixture;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    [Trait("Category", "Unit Tests")]
    public class ClientUnitTest
        : IDisposable
    {
        private readonly Fixture _fixture;

        private readonly HttpTest _httpTest;

        private readonly string _username;

        private readonly string _password;

        private readonly Client _client;

        public ClientUnitTest()
        {
            this._fixture = new Fixture();
            this._httpTest = new HttpTest();
            this._username = this._fixture.Generate<string>();
            this._password = this._fixture.Generate<string>();
            this._client = new Client(
                this._username,
                this._password);
        }

        public void Dispose()
        {
            this._httpTest?.Dispose();
        }

        [Fact]
        public void BaseUrl_ShouldBeLusoSms()
        {
            Assert.Equal("http://www.lusosms.com", Client.BaseUrl);
        }

        [Fact]
        [Trait("Category", "Check Credit")]
        public async Task CheckCredit_ShouldReturnCredits()
        {
            //Arrange
            var credits = this._fixture.Generate<double>(constraints: new
            {
                min = 0,
                max = 1000
            });
            this._httpTest.RespondWith(credits.ToString());

            //Act
            var result = await this._client.CheckCredit();

            //Assert
            this.AssertCheckCredit();
            Assert.Equal(credits, result);
        }

        [Theory]
        [InlineData("autenticacao_invalida")]
        [InlineData("sintaxe_invalida")]
        [Trait("Category", "Check Credit")]
        public async Task CheckCredit_ShouldThrowException(string returnMessage)
        {
            //Arrange
            this._httpTest.RespondWith(returnMessage);

            //Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await this._client.CheckCredit());

            //Assert
            this.AssertCheckCredit();
            Assert.Equal(returnMessage, exception.Message);
        }

        [Theory]
        [InlineData(true, 400)]
        [InlineData(false, 200)]
        [Trait("Category", "Send Sms")]
        public async Task SendSms_ShouldThrowException_ExceededCharacters(
            bool longMessage,
            int messageLength)
        {
            //Arrange
            var message = NLipsum.Core.LipsumGenerator.Generate(2).Substring(0, messageLength);
            var origin = this._fixture.Generate<string>();
            var destination = this._fixture.Generate<string>();

            //Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await this._client.SendSms(message, origin, destination, longMessage));

            //Assert
            Assert.Equal("caracteres_excedidos", exception.Message);
        }

        [Theory]
        [InlineData(SmsTypeEnum.Flash, SmsMethodEnum.GET)]
        [InlineData(SmsTypeEnum.Flash, SmsMethodEnum.POST)]
        [InlineData(SmsTypeEnum.Normal, SmsMethodEnum.GET)]
        [InlineData(SmsTypeEnum.Normal, SmsMethodEnum.POST)]
        [Trait("Category", "Send Sms")]
        public async Task SendSms_ShouldSendSms(
            SmsTypeEnum type,
            SmsMethodEnum method)
        {
            //Arrange
            var message = NLipsum.Core.LipsumGenerator.Generate(2).Substring(0, 155);
            var origin = this._fixture.Generate<string>();
            var destination = this._fixture.Generate<string>();
            this._httpTest.RespondWith("mensagem_enviada");

            //Act
            await this._client.SendSms(message, origin, destination, false, type, method);

            //Assert
            this.AssertSendSms(
                message,
                origin,
                destination,
                0,
                type,
                method);
        }

        [Theory]
        [InlineData("erro_comunicacao")]
        [InlineData("credito_insuficiente")]
        [InlineData("autenticacao_invalida")]
        [InlineData("sintaxe_invalida")]
        [InlineData("caracteres_excedidos")]
        [Trait("Category", "Send Sms")]
        public async Task SendSms_ShouldThrowException(string returnMessage)
        {
            //Arrange
            var message = NLipsum.Core.LipsumGenerator.Generate(2).Substring(0, 155);
            var origin = this._fixture.Generate<string>();
            var destination = this._fixture.Generate<string>();

            this._httpTest.RespondWith(returnMessage);

            //Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await this._client.SendSms(message, origin, destination));

            //Assert
            this.AssertSendSms(
                message,
                origin,
                destination,
                0,
                SmsTypeEnum.Normal,
                SmsMethodEnum.POST);
            Assert.Equal(exception.Message, returnMessage);
        }

        private void AssertCheckCredit()
        {
            this._httpTest.ShouldHaveCalled(string.Join("/", Client.BaseUrl, "ver_credito_get.php"))
                .WithQueryParamValue("username", this._username)
                .WithQueryParamValue("password", this._password)
                .WithVerb(HttpMethod.Get)
                .Times(1);
        }

        private void AssertSendSms(
            string message,
            string origin,
            string destination,
            ushort longMessage,
            SmsTypeEnum type,
            SmsMethodEnum method)
        {
            var messageEscaped = message.Replace(' ', '+');
            var typeString = type == SmsTypeEnum.Normal ? "normal" : "flash";
            switch (method)
            {
                case SmsMethodEnum.POST:
                    this.AssertPostSms(
                        messageEscaped,
                        origin,
                        destination,
                        longMessage,
                        typeString);
                    break;

                case SmsMethodEnum.GET:
                    this.AssertGetSms(
                        messageEscaped,
                        origin,
                        destination,
                        longMessage,
                        typeString);
                    break;
            }
        }

        private void AssertGetSms(
            string message,
            string origin,
            string destination,
            ushort longMessage,
            string type)
        {
            this._httpTest.ShouldHaveCalled(string.Join("/", Client.BaseUrl, "enviar_sms_get.php"))
                .WithQueryParamValue("username", this._username)
                .WithQueryParamValue("password", this._password)
                .WithQueryParamValue("origem", origin)
                .WithQueryParamValue("destino", destination)
                .WithQueryParamValue("mensagem", message)
                .WithQueryParamValue("mensagemlonga", longMessage)
                .WithQueryParamValue("tipo", type)
                .WithVerb(HttpMethod.Get)
                .Times(1);
        }

        private void AssertPostSms(
            string message,
            string origin,
            string destination,
            ushort longMessage,
            string type)
        {
            this._httpTest.ShouldHaveCalled(string.Join("/", Client.BaseUrl, "enviar_sms_post.php"))
                //.WithRequestBody($"{{\"username\":{this._username},\"password\":{this._password},\"origem\":{origin},\"destino\": {destination},\"mensagem\":{message},\"mensagemlonga\":{longMessage},\"tipo\":{type}}}")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }
    }
}