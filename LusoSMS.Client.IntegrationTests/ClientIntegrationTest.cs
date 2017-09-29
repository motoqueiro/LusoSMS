namespace LusoSMS.Client.IntegrationTests
{
    using LusoSMS.Client;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Trait("Category", "Integration Tests")]
    public class ClientIntegrationTest
    {
        private readonly Client _client;

        public ClientIntegrationTest()
        {
            this._client = new Client("motoqueiro", "7gw7zrm3kkhJ");
        }

        [Fact]
        [Trait("Category", "Check Credit")]
        public async Task CheckCredit()
        {
            try
            {
                var credits = await this._client.CheckCredit();
                Assert.Equal(17.000, credits);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Theory]
        [InlineData(SmsMethodEnum.GET)]
        [InlineData(SmsMethodEnum.POST)]
        [Trait("Category", "Send Sms")]
        public async Task SendSms(SmsMethodEnum method)
        {
            var message = "Mensagem de teste ao envio de sms utilizando o serviço Luso SMS! (Testes de Integração)";
            var origin = "932446190";
            var destination = "932446190";
            try
            {
                await this._client.SendSms(
                    message,
                    origin,
                    destination,
                    false,
                    SmsTypeEnum.Normal,
                    method);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }
    }
}