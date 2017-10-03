namespace LusoSMS.Client.UnitTests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Flurl.Http.Testing;
    using LusoSMS.Client.Entities;
    using LusoSMS.Client.Enums;
    using LusoSMS.Client.Exceptions;
    using SimpleFixture;
    using Xunit;

    [Trait("Category", "Unit Tests")]
    public class ClientUnitTest
        : IDisposable
    {
        private readonly Fixture _fixture;

        private readonly HttpTest _httpTest;

        private readonly string _username;

        private readonly string _password;

        private readonly LusoSMSClient _client;

        public ClientUnitTest()
        {
            this._fixture = new Fixture();
            this._httpTest = new HttpTest();
            this._username = this._fixture.Generate<string>();
            this._password = this._fixture.Generate<string>();
            this._client = new LusoSMSClient(
                this._username,
                this._password);
        }

        public void Dispose()
        {
            this._httpTest?.Dispose();
        }

        [Fact]
        [Trait("Category", "Constructor")]
        public void Constructor_NullUsernama_ShouldThrowException()
        {
            //Arrange
            var username = this._fixture.Generate<string>();

            //Act
            var exception = Assert.Throws<ArgumentNullException>(() => new LusoSMSClient(null, username));

            //Assert
            Assert.Equal("username", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "Constructor")]
        public void Constructor_NullPassword_ShouldThrowException()
        {
            //Arrange
            var username = this._fixture.Generate<string>();

            //Act
            var exception = Assert.Throws<ArgumentNullException>(() => new LusoSMSClient(username, null));

            //Assert
            Assert.Equal("password", exception.ParamName);
        }

        [Fact]
        public void BaseUrl_ShouldBeLusoSms()
        {
            Assert.Equal("http://www.lusosms.com", this._client.BaseUrl);
        }

        [Fact]
        public void ScheduledSmsManagerUrl_ShoudBeValid()
        {
            var expectedUrl = $"http://www.lusosms.com/gerir_agendados.php?username={this._username}&password={this._password}";
            Assert.Equal(expectedUrl, this._client.ScheduledSmsManagerUrl);
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
            this._httpTest.RespondWith(credits.ToString("0.000"));

            //Act
            var result = await this._client.CheckCreditAsync();

            //Assert
            this.AssertCheckCredit();
            Assert.Equal(Math.Round(credits, 3), result);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetCheckCreditInvalidReturnMessages), MemberType = typeof(TestDataGenerator))]
        [Trait("Category", "Check Credit")]
        public async Task CheckCredit_ShouldThrowException(
            string returnMessage,
            Type expectedExeptionType)
        {
            //Arrange
            this._httpTest.RespondWith(returnMessage);

            //Act
            var exception = await Assert.ThrowsAsync(
                expectedExeptionType,
                async () => await this._client.CheckCreditAsync());

            //Assert
            this.AssertCheckCredit();
            Assert.Equal(returnMessage, ((LusoSMSException)exception).ReturnMessage);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetPricesPerSms), MemberType = typeof(TestDataGenerator))]
        [Trait("Category", "Calculate Price Per SMS")]
        public void CalculatePricePerSms_ShouldReturnCorrectPrice(
            string destination,
            int package,
            bool includeVAT,
            decimal expectedPrice)
        {
            //Act
            var price = this._client.CalculatePricePerSms(
                destination,
                package,
                includeVAT);

            //Assert
            Assert.Equal(expectedPrice, price);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetPricesPerSmsCampaign), MemberType = typeof(TestDataGenerator))]
        [Trait("Category", "Calculate Price Per SMS")]
        public void CalculatePricePerSms_ByCampaign_ShouldReturnCorrectPrice(
            string destination,
            uint packageCredits,
            decimal packagePrice,
            bool includeVat,
            decimal expectedPrice)
        {
            //Arrange
            var creditPackage = new CreditPackage()
            {
                Credits = packageCredits,
                Price = packagePrice
            };

            //Act
            var price = this._client.CalculatePricePerSms(
                destination,
                creditPackage,
                includeVat);

            //Assert
            Assert.Equal(expectedPrice, price);
        }

        [Fact]
        [Trait("Category", "Send Sms")]
        public async Task SendSms_ShouldThrowException_ExceededCharacters()
        {
            //Arrange
            var message = NLipsum.Core.LipsumGenerator
                .Generate(2)
                .Substring(0, 400);
            var origin = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);
            var destination = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);

            //Act
            var exception = await Assert.ThrowsAsync<ExceededCharactersException>(async () => await this._client.SendSmsAsync(message, origin, destination));

            //Assert
            Assert.Equal("caracteres_excedidos", exception.ReturnMessage);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetSendSmsData), MemberType = typeof(TestDataGenerator))]
        [Trait("Category", "Send Sms")]
        public async Task SendSms_ShouldSendSms(
            SmsTypeEnum type,
            SmsMethodEnum method)
        {
            //Arrange
            var message = NLipsum.Core.LipsumGenerator
                .Generate(2)
                .Substring(0, 155);
            var origin = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);
            var destination = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);
            this._httpTest.RespondWith("mensagem_enviada");

            //Act
            await this._client.SendSmsAsync(message, origin, destination, type, method);

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
        [MemberData(nameof(TestDataGenerator.GetSendSmsInvalidReturnMessages), MemberType = typeof(TestDataGenerator))]
        [Trait("Category", "Send Sms")]
        public async Task SendSms_ShouldThrowException(
            string returnMessage,
            Type expectedExceptionType)
        {
            //Arrange
            var message = NLipsum.Core.LipsumGenerator
                .Generate(2)
                .Substring(0, 155);
            var origin = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);
            var destination = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);

            this._httpTest.RespondWith(returnMessage);

            //Act
            var exception = await Assert.ThrowsAsync(
                expectedExceptionType,
                async () => await this._client.SendSmsAsync(message, origin, destination));

            //Assert
            this.AssertSendSms(
                message,
                origin,
                destination,
                0,
                SmsTypeEnum.Normal,
                SmsMethodEnum.POST);
            Assert.Equal(returnMessage, ((LusoSMSException)exception).ReturnMessage);
        }

        [Fact]
        [Trait("Category", "Schedule Sms")]
        public async Task ScheduleSms_ShouldScheduleSms()
        {
            //Arrange
            var message = NLipsum.Core.LipsumGenerator
                .Generate(2)
                .Substring(0, 155);
            var origin = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);
            var destination = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);
            var sendDate = DateTime.UtcNow.AddDays(7);

            this._httpTest.RespondWith("mensagem_agendada");

            //Act
            await this._client.ScheduleSmsAsync(
                message,
                origin,
                destination,
                sendDate);

            //Assert
            this.AssertScheduleSms(
                message,
                origin,
                destination,
                sendDate,
                0,
                SmsTypeEnum.Normal);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetScheduleSmsInvalidMessages), MemberType = typeof(TestDataGenerator))]
        [Trait("Category", "Schedule Sms")]
        public async Task ScheduleSms_ShouldThrowException(
            string returnMessage,
            Type expectedExceptionType)
        {
            //Arrange
            var message = NLipsum.Core.LipsumGenerator
                .Generate(2)
                .Substring(0, 155);
            var origin = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);
            var destination = TestDataGenerator.GeneratePhoneNumber(CountriesEnum.Portugal);
            var sendDate = DateTime.UtcNow.AddDays(7);

            this._httpTest.RespondWith(returnMessage);

            //Act
            var exception = await Assert.ThrowsAsync(
                expectedExceptionType,
                async () => await this._client.ScheduleSmsAsync(
                    message,
                    origin,
                    destination,
                    sendDate));

            //Assert
            this.AssertScheduleSms(
                message,
                origin,
                destination,
                sendDate,
                0,
                SmsTypeEnum.Normal);
            Assert.Equal(returnMessage, ((LusoSMSException)exception).ReturnMessage);
        }

        private void AssertScheduleSms(
            string message,
            string origin,
            string destination,
            DateTime sendDate,
            ushort longMessage,
            SmsTypeEnum type)
        {
            this._httpTest.ShouldHaveCalled(string.Join("/", this._client.BaseUrl, "agendar_sms_get.php"))
                .WithQueryParamValue("username", this._username)
                .WithQueryParamValue("password", this._password)
                .WithQueryParamValue("origem", origin)
                .WithQueryParamValue("destino", destination)
                .WithQueryParamValue("mensagem", message)
                .WithQueryParamValue("mensagemlonga", longMessage)
                .WithQueryParamValue("tipo", type)
                .WithQueryParamValue("dataenvio", sendDate.ToString("yyyy|MM|dd|HH|mm|ss"))
                .WithVerb(HttpMethod.Get)
                .Times(1);
        }

        private void AssertCheckCredit()
        {
            this._httpTest.ShouldHaveCalled(string.Join("/", this._client.BaseUrl, "ver_credito_get.php"))
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
            this._httpTest.ShouldHaveCalled(string.Join("/", this._client.BaseUrl, "enviar_sms_get.php"))
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
            this._httpTest.ShouldHaveCalled(string.Join("/", this._client.BaseUrl, "enviar_sms_post.php"))
                //.WithRequestBody($"{{\"username\":{this._username},\"password\":{this._password},\"origem\":{origin},\"destino\": {destination},\"mensagem\":{message},\"mensagemlonga\":{longMessage},\"tipo\":{type}}}")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }
    }
}