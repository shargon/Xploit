namespace Xploit.Core.Rfid.Mifare.Internal
{
    /// <summary>
    /// Class that handles the access conditions to a given sector of the card
    /// </summary>
    public class AccessConditions
    {
        public AccessConditions()
        {
            DataAreas = new DataAreaAccessCondition[3];
            DataAreas[0] = new DataAreaAccessCondition();
            DataAreas[1] = new DataAreaAccessCondition();
            DataAreas[2] = new DataAreaAccessCondition();

            Trailer = new TrailerAccessCondition();

            MADVersion = MADVersionEnum.NoMAD;
            MultiApplicationCard = false;
        }

        #region Properties

        /// <summary>
        /// Version of the MAD supported by the card. The MAD version is written only in the trailer datablock of sector 0.
        /// For all other sector, this value has no meaning
        /// </summary>
        public enum MADVersionEnum
        {
            NoMAD,
            Version1,
            Version2
        }
        public MADVersionEnum MADVersion;

        /// <summary>
        /// True if the card supports multiple applications
        /// </summary>
        public bool MultiApplicationCard;

        /// <summary>
        /// Access conditions for each data area. This array has always 3 elements
        /// </summary>
        public DataAreaAccessCondition[] DataAreas;

        /// <summary>
        /// Access conditions for the trailer datablock
        /// </summary>
        public TrailerAccessCondition Trailer;

        #endregion
    }
}
