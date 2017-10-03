namespace LusoSMS.Client.IntegrationTests
{
    using LusoSMS.Client;
    using LusoSMS.Client.Enums;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Trait("Category", "Integration Tests")]
    public class ClientIntegrationTest
    {
        private readonly ILusoSMSClient _client;

        public ClientIntegrationTest()
        {
            this._client = new LusoSMSClient("motoqueiro", "7gw7zrm3kkhJ");
        }

        [Fact]
        [Trait("Category", "Check Credit")]
        public async Task CheckCredit()
        {
            try
            {
                var credits = await this._client.CheckCreditAsync();
                Assert.Equal(17.000, credits);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Theory(Skip = "Spend credits")]
        [InlineData(SmsMethodEnum.GET)]
        [InlineData(SmsMethodEnum.POST)]
        [Trait("Category", "Send Sms")]
        public async Task SendSms(SmsMethodEnum method)
        {
            var message = "Mensagem de teste ao envio de sms utilizando o serviço Luso SMS! (Testes de Integração ao Envio)";
            var origin = "932446190";
            var destination = "932446190";
            try
            {
                await this._client.SendSmsAsync(
                    message,
                    origin,
                    destination,
                    SmsTypeEnum.Normal,
                    method);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        [Trait("Category", "Schedule Sms")]
        public async Task ScheduleSms()
        {
            var message = "Mensagem de teste ao envio de sms utilizando o serviço Luso SMS! (Testes de Integração ao Agendamento)";
            var origin = "932446190";
            var destination = "932446190";
            var sendDate = DateTime.UtcNow.AddDays(7);
            try
            {
                await this._client.ScheduleSmsAsync(
                    message,
                    origin,
                    destination,
                    sendDate,
                    SmsTypeEnum.Normal);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }
    }
}