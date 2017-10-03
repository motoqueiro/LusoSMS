namespace LusoSMS.Client.UnitTests
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using LusoSMS.Client;
    using LusoSMS.Client.Enums;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal static class TestDataGenerator
    {
        public static Random Random { get; private set; }

        static TestDataGenerator()
        {
            Random = new Random();
        }

        public static IEnumerable<object[]> GetCheckCreditInvalidReturnMessages()
        {
            yield return new object[] { "autenticacao_invalida" };
            yield return new object[] { "sintaxe_invalida" };
        }

        public static IEnumerable<object[]> GetSendSmsInvalidReturnMessages()
        {
            yield return new object[] { "erro_comunicacao" };
            yield return new object[] { "credito_insuficiente" };
            yield return new object[] { "autenticacao_invalida" };
            yield return new object[] { "sintaxe_invalida" };
            yield return new object[] { "caracteres_excedidos" };
        }

        public static IEnumerable<object[]> GetScheduleSmsInvalidMessages()
        {
            yield return new object[] { "autenticacao_invalida" };
            yield return new object[] { "sintaxe_invalida" };
            yield return new object[] { "caracteres_excedidos" };
            yield return new object[] { "data_invalida" };
        }

        public static IEnumerable<object[]> GetSendSmsData()
        {
            yield return new object[] { SmsTypeEnum.Flash, SmsMethodEnum.GET };
            yield return new object[] { SmsTypeEnum.Flash, SmsMethodEnum.POST };
            yield return new object[] { SmsTypeEnum.Normal, SmsMethodEnum.GET };
            yield return new object[] { SmsTypeEnum.Normal, SmsMethodEnum.POST };
        }

        public static IEnumerable<object[]> GetPricesPerSms()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Price Matrix.csv");
            using (var textReader = new StreamReader(path))
            {
                var csvConfiguration = new CsvConfiguration()
                {
                    HasHeaderRecord = true
                };
                using (var csvReader = new CsvReader(textReader, csvConfiguration))
                {
                    while (csvReader.Read())
                    {
                        yield return new object[] {
                            GeneratePhoneNumber(csvReader.GetField<CountriesEnum>(1)),
                            csvReader.GetField<int>(0),
                            csvReader.GetField<bool>(2),
                            csvReader.GetField<decimal>(3)
                        };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetPricesPerSmsCampaign()
        {
            yield return new object[] { GeneratePhoneNumber(CountriesEnum.Portugal), 1000, 40, false, 0.04M };
            yield return new object[] { GeneratePhoneNumber(CountriesEnum.Portugal), 1000, 40, true, 0.05M };
        }

        public static string GeneratePhoneNumber(CountriesEnum country)
        {
            var countryConfiguration = LusoSMSClient.GetCountryConfiguration(country);
            return GeneratePhoneNumber(
                countryConfiguration.Indicative,
                countryConfiguration.NumberDigits);
        }

        public static string GeneratePhoneNumber(
            int indicative,
            int numbers)
        {
            var sb = new StringBuilder();
            sb.Append(indicative);
            for (int i = 0; i < numbers; i++)
            {
                var value = Random.Next(0, 9);
                sb.Append(value);
            }

            return sb.ToString();
        }
    }
}