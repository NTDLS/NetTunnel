namespace NetTunnel.UI
{
    /// <summary>
    /// What the UI remembers between runs (not including the login preferences).
    /// </summary>
    internal class UISettings
    {
        /// <summary>A culture name such as "de" or "pt-BR". Empty follows the Windows language.</summary>
        public string Language { get; set; } = string.Empty;

        public static UISettings Load()
        {
            try
            {
                return NTDLS.Persistence.LocalUserApplicationData.LoadFromDisk(NetTunnel.Library.Constants.FriendlyName, new UISettings());
            }
            catch
            {
                return new UISettings();
            }
        }

        public void Save()
        {
            try
            {
                NTDLS.Persistence.LocalUserApplicationData.SaveToDisk(NetTunnel.Library.Constants.FriendlyName, this);
            }
            catch
            {
            }
        }
    }
}
