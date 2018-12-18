using NetTunnel.Library.Win32;

namespace NetTunnel.Client
{
    public static class Singletons
    {
        public static Configuration Config { get; set; }

        private static EventLogging _eventLog = null;
        public static EventLogging EventLog
        {
            get
            {
                if (_eventLog == null)
                {
                    _eventLog = new EventLogging(Library.Constants.TitleCaption, false);
                }

                return _eventLog;
            }
        }
    }
}
