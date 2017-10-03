namespace LusoSMS.Client.Entities
{
    using System.Collections.Generic;
    using LusoSMS.Client.Enums;

    /// <summary>
    /// Configuration of a supported country on the Luso SMS service.
    /// </summary>
    public class CountryConfiguration
    {
        /// <summary>
        /// Country of the configuration.
        /// </summary>
        public CountriesEnum Country { get; set; }

        /// <summary>
        /// List of supported operators.
        /// </summary>
        public IEnumerable<string> Operators { get; set; }

        /// <summary>
        /// Phone number indicative of the country.
        /// </summary>
        public int Indicative { get; set; }

        /// <summary>
        /// Expected number of digits after the indicative.
        /// </summary>
        public int NumberDigits { get; set; }

        /// <summary>
        /// Number of credits spent to send one SMS.
        /// </summary>
        public decimal CreditsPerSms { get; set; }

        /// <summary>
        /// Regex pattern to validate the phone number of the specific country.
        /// </summary>
        public string Pattern
        {
            get
            {
                if (this.Country == CountriesEnum.Portugal)
                {
                    return $@"^({this.Indicative}){{0,1}}\d{{{this.NumberDigits}}}$";
                }

                return $@"^{this.Indicative}\d{{{this.NumberDigits}}}$";
            }
        }
    }
}