namespace LusoSMS.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;
    using LusoSMS.Client.Entities;
    using LusoSMS.Client.Enums;
    using LusoSMS.Client.Exceptions;

    /// <summary>
    /// Http client for the <see cref="http://www.lusosms.com/index.php">Luso SMS</see> API.
    /// </summary>
    public class LusoSMSClient
        : ILusoSMSClient
    {
        private readonly string _username;

        private readonly string _password;

        private static string PhoneNumberPattern;

        private static List<CountryConfiguration> CountryConfigurations;

        private static List<CreditPackage> CreditPackages;

        static LusoSMSClient()
        {
            LusoSMSClient.CountryConfigurations = LoadCountryConfigurations().ToList();
            LusoSMSClient.CreditPackages = LoadCreditPackages().ToList();
            LusoSMSClient.PhoneNumberPattern = LusoSMSClient.GetPhoneNumberPattern();
        }

        private static IEnumerable<CreditPackage> LoadCreditPackages()
        {
            yield return new CreditPackage
            {
                Credits = 20,
                Price = 1.90M
            };
            yield return new CreditPackage
            {
                Credits = 50,
                Price = 4.20M
            };
            yield return new CreditPackage
            {
                Credits = 100,
                Price = 8.20M
            };
            yield return new CreditPackage
            {
                Credits = 200,
                Price = 16.00M
            };
            yield return new CreditPackage
            {
                Credits = 400,
                Price = 29.00M
            };
            yield return new CreditPackage
            {
                Credits = 1000,
                Price = 70.00M
            };
            yield return new CreditPackage
            {
                Credits = 2000,
                Price = 140.00M
            };
            yield return new CreditPackage
            {
                Credits = 3000,
                Price = 210.00M
            };
            yield return new CreditPackage
            {
                Credits = 5000,
                Price = 350.00M
            };
            yield return new CreditPackage
            {
                Credits = 10000,
                Price = 500.00M
            };
            yield return new CreditPackage
            {
                Credits = 20000,
                Price = 800.00M
            };
        }

        private static IEnumerable<CountryConfiguration> LoadCountryConfigurations()
        {
            yield return new CountryConfiguration
            {
                Country = CountriesEnum.Portugal,
                Operators = new[] { "Optimus", "TMN", "Vodafone", "Phone-ix" },
                Indicative = 351,
                NumberDigits = 9,
                CreditsPerSms = 1.000M
            };
            yield return new CountryConfiguration
            {
                Country = CountriesEnum.Brazil,
                Operators = new[] { "Brasil Telecom Celular", "Claro", "Amazonia Celular", "TIM Cel", "Vivo", "Telemig", "TNL", "Sercomtel", "Triangulo", "Unicel" },
                Indicative = 55,
                NumberDigits = 10,
                CreditsPerSms = 0.857M
            };
            yield return new CountryConfiguration
            {
                Country = CountriesEnum.Spain,
                Operators = new[] { "Orange", "Movistar", "Vodafone", "Yoigo" },
                Indicative = 34,
                NumberDigits = 9,
                CreditsPerSms = 1.214M
            };
            yield return new CountryConfiguration
            {
                Country = CountriesEnum.Mozambique,
                Operators = new[] { "mCel", "Vodacom Mozambique" },
                Indicative = 258,
                NumberDigits = 9,
                CreditsPerSms = 0.714M
            };
            yield return new CountryConfiguration
            {
                Country = CountriesEnum.Angola,
                Operators = new[] { "UNITEL", "MOVICEL" },
                Indicative = 244,
                NumberDigits = 9,
                CreditsPerSms = 1.000M
            };
        }

        /// <summary>
        /// Add a new country configuration or update an existing one by the country name.
        /// </summary>
        /// <param name="countryConfiguration">Country configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddOrUpdateCountryConfiguration(CountryConfiguration countryConfiguration)
        {
            if (countryConfiguration == null)
            {
                throw new ArgumentNullException(nameof(countryConfiguration));
            }

            var existingConfiguration = LusoSMSClient.GetCountryConfiguration(countryConfiguration.Country);
            if (existingConfiguration != null)
            {
                existingConfiguration = countryConfiguration;
            }
            else
            {
                LusoSMSClient.CountryConfigurations.Add(countryConfiguration);
            }

            LusoSMSClient.PhoneNumberPattern = LusoSMSClient.GetPhoneNumberPattern();
        }

        /// <summary>
        /// Gets the country configuration by the country name.
        /// </summary>
        /// <param name="country">Country name.</param>
        /// <returns>Country configuration.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static CountryConfiguration GetCountryConfiguration(CountriesEnum country)
        {
            return LusoSMSClient.CountryConfigurations.SingleOrDefault(cc => cc.Country == country);
        }

        /// <summary>
        /// Constructor of the Luso SMS http client.
        /// </summary>
        /// <param name="username">Username of Luso SMS service.</param>
        /// <param name="password">Password of Luso SMS service.</param>
        /// <exception cref="ArgumentNullException">When username or password are null or empty.</exception>
        public LusoSMSClient(
            string username,
            string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            this._username = username;
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            this._password = password;
        }

        /// <summary>
        /// Luso SMS service url.
        /// </summary>
        public string BaseUrl => "http://www.lusosms.com";

        /// <summary>
        /// Luso SMS scheduled messages manager url.
        /// </summary>
        public string ScheduledSmsManagerUrl => $"{this.BaseUrl}/gerir_agendados.php?username={this._username}&password={this._password}";

        /// <summary>
        /// Get the number of credits you have.
        /// </summary>
        /// <returns>Number of credits.</returns>
        /// <exception cref="Exception">When a message other than number of credits is returned by the service</exception>
        public async Task<double> CheckCreditAsync()
        {
            var result = await this.BaseUrl
                .AppendPathSegment("ver_credito_get.php")
                .SetQueryParam("username", this._username)
                .SetQueryParam("password", this._password)
                .GetStringAsync();

            if (double.TryParse(result, out var credits))
            {
                return Math.Round(credits, 3);
            }

            this.ResolveException(result);
            return -1;
        }

        /// <summary>
        /// Calculate the price per SMS based on your bought package credits from a promotional campaign.
        /// </summary>
        /// <param name="destination">Phone number of the SMS destinatary. Used to determine the country by the indicative number.</param>
        /// <param name="creditPackageConfiguration">Credit package configuration of the promotional campaign.</param>
        /// <param name="includeVAT">Include the tax on the final result.</param>
        /// <returns>Price per SMS.</returns>
        public decimal CalculatePricePerSms(
            string destination,
            CreditPackage creditPackageConfiguration,
            bool includeVAT = true)
        {
            return this.CalculatePricePerSms(
                destination,
                creditPackageConfiguration.Credits,
                creditPackageConfiguration.Price,
                includeVAT);
        }

        /// <summary>
        /// Calculate the price per SMS based on your bought package credits from the <see cref="http://www.lusosms.com/preco.php">default</see> package credits configuration.
        /// </summary>
        /// <param name="destination">Phone number of the SMS destinatary. Used to determine the country by the indicative number.</param>
        /// <param name="packageCredits">Number of credits of your package.</param>
        /// <param name="includeVAT">Include the tax on the final result.</param>
        /// <returns>Price per SMS.</returns>
        public decimal CalculatePricePerSms(
            string destination,
            int packageCredits,
            bool includeVAT = true)
        {
            var package = LusoSMSClient.CreditPackages.SingleOrDefault(cp => cp.Credits == packageCredits);
            if (package == null)
            {
                throw new Exception($"Invalid credit configuration with {package} credits!");
            }

            return this.CalculatePricePerSms(
                destination,
                package.Credits,
                package.Price,
                includeVAT);
        }

        /// <summary>
        /// Send a message using the Luso SMS service.
        /// </summary>
        /// <param name="message">The message to be sent. Cannot be longer than 300 characters and if it´s longer than 155 characters the corresponding credits of two SMS will be charged.</param>
        /// <param name="origin">Phone number to be set as origin.</param>
        /// <param name="destination">Phone number of the destinatary.</param>
        /// <param name="type">The type of SMS to be sent: Normal or Flash (the message appears open in the phone but it will only be stored if the recipient wishes.)</param>
        /// <param name="method">The http method to use: GET or POST.</param>
        public async Task SendSmsAsync(
            string message,
            string origin,
            string destination,
            SmsTypeEnum type = SmsTypeEnum.Normal,
            SmsMethodEnum method = SmsMethodEnum.POST)
        {
            this.ValidateArguments(message, origin, destination);
            this.ValidateMessageLength(message);
            var escapedMessage = this.EscapeMessage(message);
            var longMessage = this.ResolveLongMessage(message);
            var rawType = this.ResolveMessageType(type);
            string result;
            switch (method)
            {
                case SmsMethodEnum.POST:
                    result = await this.PostSms(
                        escapedMessage,
                        origin,
                        destination,
                        longMessage,
                        rawType);
                    break;

                case SmsMethodEnum.GET:
                    result = await this.GetSms(
                        escapedMessage,
                        origin,
                        destination,
                        longMessage,
                        rawType);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }

            if (result != "mensagem_enviada")
            {
                this.ResolveException(result);
            }
        }

        /// <summary>
        /// Schedule an SMS to be send at a specific date.
        /// </summary>
        /// <param name="message">The message to be sent. Cannot be longer than 300 characters and if it´s longer than 155 characters the corresponding credits of two SMS will be charged.</param>
        /// <param name="origin">Phone number to be set as origin.</param>
        /// <param name="destination">Phone number of the destinatary.</param>
        /// <param name="sendDate">The date to send the SMS. You can only schedule sending an SMS        before the end of the following year of the current year.</param>
        /// <param name="type">The type of SMS to be sent: Normal or Flash (the message appears open in the phone but it will only be stored if the recipient wishes.)</param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">When the send date is after the limit date.</exception>
        public async Task ScheduleSmsAsync(
            string message,
            string origin,
            string destination,
            DateTime sendDate,
            SmsTypeEnum type = SmsTypeEnum.Normal)
        {
            this.ValidateArguments(message, origin, destination);
            this.ValidateMessageLength(message);
            var limitDate = new DateTime(DateTime.UtcNow.Year, 12, 31, 23, 59, 59);
            if (sendDate > limitDate)
            {
                throw new InvalidDateException();
            }

            var messageRaw = this.EscapeMessage(message);
            var sendDateRaw = sendDate.ToString("yyyy|MM|dd|HH|mm|ss");
            var longMessage = this.ResolveLongMessage(message);
            var result = await this.BaseUrl
                .AppendPathSegment("agendar_sms_get.php")
                .SetQueryParam("username", this._username)
                .SetQueryParam("password", this._password)
                .SetQueryParam("origem", origin)
                .SetQueryParam("destino", destination)
                .SetQueryParam("mensagem", message)
                .SetQueryParam("mensagemlonga", longMessage)
                .SetQueryParam("tipo", type)
                .SetQueryParam("dataenvio", sendDateRaw)
                .GetStringAsync();
            if (result != "mensagem_agendada")
            {
                this.ResolveException(result);
            }
        }

        private static string GetPhoneNumberPattern()
        {
            var pattern = new StringBuilder();
            pattern.Append("^(");
            foreach (var countryConfiguration in CountryConfigurations)
            {
                pattern.Append(countryConfiguration.Pattern);
                pattern.Append("|");
            }

            pattern.Remove(pattern.Length - 1, 1);
            pattern.Append(")$");
            return pattern.ToString();
        }

        private void ValidateArguments(
            string message,
            string origin,
            string destination)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrEmpty(origin))
            {
                throw new ArgumentNullException(nameof(origin));
            }

            this.ValidatePhoneNumber(origin, nameof(origin));

            if (string.IsNullOrEmpty(destination))
            {
                throw new ArgumentNullException(nameof(destination));
            }

            this.ValidatePhoneNumber(destination, nameof(destination));
        }

        private void ValidatePhoneNumber(
            string origin,
            string paramName)
        {
            if (!Regex.IsMatch(origin, LusoSMSClient.PhoneNumberPattern))
            {
                throw new ArgumentException("Argument should be a supported phone number", paramName);
            }
        }

        private decimal CalculatePricePerSms(
            string destination,
            uint packageCredits,
            decimal packagePrice,
            bool includeVAT)
        {
            this.ValidatePhoneNumber(destination, nameof(destination));
            var configuration = LusoSMSClient.CountryConfigurations.SingleOrDefault(cc => Regex.IsMatch(destination, cc.Pattern));
            if (configuration == null)
            {
                throw new Exception("Unable to resolve sms country destination by the number!");
            }

            var pricePerSms = packagePrice / (packageCredits / configuration.CreditsPerSms);
            if (!includeVAT)
            {
                return Math.Round(pricePerSms, 2);
            }

            return Math.Round(pricePerSms * 1.23M, 2);
        }

        private void ValidateMessageLength(string message)
        {
            if (message.Length > 300)
            {
                throw new ExceededCharactersException();
            }
        }

        private string EscapeMessage(string message)
        {
            var iso = Encoding.GetEncoding("ISO-8859-1");
            var utf8 = Encoding.UTF8;
            var utfBytes = utf8.GetBytes(message);
            var isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            return iso.GetString(isoBytes).Replace(' ', '+');
        }

        private string ResolveMessageType(SmsTypeEnum type)
        {
            return type == SmsTypeEnum.Flash ? "flash" : "normal";
        }

        private ushort ResolveLongMessage(string message)
        {
            return message.Length > 155 ? (ushort)1 : (ushort)0;
        }

        private async Task<string> GetSms(
            string message,
            string origin,
            string destination,
            ushort longMessage,
            string type)
        {
            return await this.BaseUrl
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
            return await this.BaseUrl
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

        private void ResolveException(string errorMessage)
        {
            switch (errorMessage.ToLower())
            {
                case ReturnMessages.CommunicationError:
                    throw new CommunicationErrorException();
                case ReturnMessages.ExceededCharacters:
                    throw new ExceededCharactersException();
                case ReturnMessages.InsufficientCredit:
                    throw new InsufficientCreditsException();
                case ReturnMessages.InvalidAuthentication:
                    throw new InvalidAuthenticationException();
                case ReturnMessages.InvalidDate:
                    throw new InvalidDateException();
                case ReturnMessages.InvalidSintax:
                    throw new InvalidSintaxException();
                default:
                    throw new Exception($"Unexpected return message: {errorMessage}");
            }
        }
    }
}