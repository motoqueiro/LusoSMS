namespace LusoSMS.Client.Entities
{
    /// <summary>
    /// Configuration of the available credit packages on the Luso SMS service.
    /// </summary>
    public class CreditPackage
    {
        /// <summary>
        /// Number of credits.
        /// </summary>
        public uint Credits { get; set; }

        /// <summary>
        /// Price to pay without taxes (VAT).
        /// </summary>
        public decimal Price { get; set; }
    }
}