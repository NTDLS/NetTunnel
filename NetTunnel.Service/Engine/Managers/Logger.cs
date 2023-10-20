namespace NetTunnel.Service.Engine.Managers
{
    public class Logger
    {
        private readonly EngineCore _core;

        public Logger(EngineCore core)
        {
            _core = core;
        }

        public void Write(string text)
        {
            Console.WriteLine(text);
        }
    }
}
