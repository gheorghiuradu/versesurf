namespace PlayFabService
{
    public class FreeItemPolicy
    {
        /// <summary>
        /// Frequency in days
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// From PlayFab Catalog
        /// </summary>
        public string FreeItemId { get; set; }

        /// <summary>
        /// How many free items to grant
        /// </summary>
        public int FreeItemGrantCount { get; set; }

        /// <summary>
        /// Key which is used to store when the policy was applied in PlayFab
        /// </summary>
        public string PlayFabInternalDataKey { get; set; }
    }
}