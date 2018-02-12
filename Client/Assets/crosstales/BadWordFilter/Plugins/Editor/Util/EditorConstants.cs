namespace Crosstales.BWF.EditorUtil
{
    /// <summary>Collected editor constants of very general utility for the asset.</summary>
    public static class EditorConstants
    {

        #region Constant variables

        // Keys for the configuration of the asset
        public const string KEY_UPDATE_CHECK = Util.Constants.KEY_PREFIX + "UPDATE_CHECK";
        public const string KEY_UPDATE_OPEN_UAS = Util.Constants.KEY_PREFIX + "UPDATE_OPEN_UAS";
        public const string KEY_REMINDER_CHECK = Util.Constants.KEY_PREFIX + "REMINDER_CHECK";
        public const string KEY_TELEMETRY = Util.Constants.KEY_PREFIX + "TELEMETRY";
        public const string KEY_PREFAB_AUTOLOAD = Util.Constants.KEY_PREFIX + "PREFAB_AUTOLOAD";

        public const string KEY_HIERARCHY_ICON = Util.Constants.KEY_PREFIX + "HIERARCHY_ICON";

        public const string KEY_UPDATE_DATE = Util.Constants.KEY_PREFIX + "UPDATE_DATE";

        public const string KEY_REMINDER_DATE = Util.Constants.KEY_PREFIX + "REMINDER_DATE";
        public const string KEY_REMINDER_COUNT = Util.Constants.KEY_PREFIX + "REMINDER_COUNT";

        public const string KEY_LAUNCH = Util.Constants.KEY_PREFIX + "LAUNCH";

        public const string KEY_TELEMETRY_DATE = Util.Constants.KEY_PREFIX + "TELEMETRY_DATE";

        // Default values
        public const string DEFAULT_ASSET_PATH = "/crosstales/BadWordFilter/";
        public const bool DEFAULT_UPDATE_CHECK = true;
        public const bool DEFAULT_UPDATE_OPEN_UAS = false;
        public const bool DEFAULT_REMINDER_CHECK = true;
        public const bool DEFAULT_TELEMETRY = true;
        public const bool DEFAULT_PREFAB_AUTOLOAD = false;
        public const bool DEFAULT_HIERARCHY_ICON = true;

        #endregion


        #region Changable variables

        /// <summary>Sub-path to the prefabs.</summary>
        public static string PREFAB_SUBPATH = "Prefabs/";

        #endregion


        #region Properties

        /// <summary>Returns the URL of the asset in UAS.</summary>
        /// <returns>The URL of the asset in UAS.</returns>
        public static string ASSET_URL
        {
            get
            {

                if (Util.Constants.isPro)
                {
                    return Util.Constants.ASSET_PRO_URL;
                }
                else
                {
                    return "https://www.assetstore.unity3d.com/#!/content/48397?aid=1011lNGT&pubref=" + Util.Constants.ASSET_NAME;
                }
            }
        }

        /// <summary>Returns the UID of the asset.</summary>
        /// <returns>The UID of the asset.</returns>
        public static System.Guid ASSET_UID
        {
            get
            {
                if (Util.Constants.isPro)
                {
                    return new System.Guid("b11eebc0-525a-4d58-b33d-c0a9a728f3a9");
                }
                else
                {
                    return new System.Guid("61a28d26-56a8-4109-8131-45161614ee9d");
                }
            }
        }

        #endregion

    }
}
// © 2015-2017 crosstales LLC (https://www.crosstales.com)