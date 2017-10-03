namespace LusoSMS.Client
{
    using System;
    using System.Threading.Tasks;
    using LusoSMS.Client.Entities;
    using LusoSMS.Client.Enums;

    public interface ILusoSMSClient
    {
        /// <summary>
        /// Luso SMS service url.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Luso SMS scheduled messages manager url.
        /// </summary>
        string ScheduledSmsManagerUrl { get; }

        /// <summary>
        /// Calculate the price per SMS based on your bought package credits from a promotional campaign.
        /// </summary>
        /// <param name="destination">Phone number of the SMS destinatary. Used to determine the country by the indicative number.</param>
        /// <param name="creditPackageConfiguration">Credit package configuration of the promotional campaign.</param>
        /// <param name="includeVAT">Include the tax on the final result.</param>
        /// <returns>Price per SMS.</returns>
        decimal CalculatePricePerSms(
            string destination,
            CreditPackage creditPackageConfiguration,
            bool includeVAT = true);

        /// <summary>
        /// Calculate the price per SMS based on your bought package credits from the default package credits configuration (http://www.lusosms.com/preco.php).
        /// </summary>
        /// <param name="destination">Phone number of the SMS destinatary. Used to determine the country by the indicative number.</param>
        /// <param name="packageCredits">Number of credits of your package.</param>
        /// <param name="includeVAT">Include the tax on the final result.</param>
        /// <returns>Price per SMS.</returns>
        decimal CalculatePricePerSms(
            string destination,
            int packageCredits,
            bool includeVAT = true);

        /// <summary>
        /// Get the number of credits you have.
        /// </summary>
        /// <returns>Number of credits.</returns>
        Task<double> CheckCreditAsync();

        /// <summary>
        /// Schedule an SMS to be send at a specific date.
        /// </summary>
        /// <param name="message">The message to be sent. Cannot be longer than 300 characters and if it´s longer than 155 characters the corresponding credits of two SMS will be charged.</param>
        /// <param name="origin">Phone number to be set as origin.</param>
        /// <param name="destination">Phone number of the destinatary.</param>
        /// <param name="sendDate">The date to send the SMS. You can only schedule sending an SMS        before the end of the following year of the current year.</param>
        /// <param name="type">The type of SMS to be sent: Normal or Flash (the message appears open in the phone but it will only be stored if the recipient wishes.)</param>
        Task ScheduleSmsAsync(
            string message,
            string origin,
            string destination,
            DateTime sendDate,
            SmsTypeEnum type = SmsTypeEnum.Normal);

        /// <summary>
        ///
        /// </summary>
        /// <param name="message">The message to be sent. Cannot be longer than 300 characters and if it´s longer than 155 characters the corresponding credits of two SMS will be charged.</param>
        /// <param name="origin">Phone number to be set as origin.</param>
        /// <param name="destination">Phone number of the destinatary.</param>
        /// <param name="type">The type of SMS to be sent: Normal or Flash (the message appears open in the phone but it will only be stored if the recipient wishes.)</param>
        /// <param name="method">The http method to use: GET or POST.</param>
        Task SendSmsAsync(
            string message,
            string origin,
            string destination,
            SmsTypeEnum type = SmsTypeEnum.Normal,
            SmsMethodEnum method = SmsMethodEnum.POST);
    }
}