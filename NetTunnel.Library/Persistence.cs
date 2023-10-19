using Newtonsoft.Json;

namespace NetTunnel.Library
{
    public static class Persistence
    {
        public static void SaveToDisk<T>(T obj)
        {
            var commonAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var dataFolder = Path.Combine(commonAppDataFolder, Library.Constants.FriendlyName);

            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            var type = typeof(T);
            string typeName = type.IsGenericType ? type.GetGenericArguments()[0].Name : type.Name;

            var jsonText = JsonConvert.SerializeObject(obj, Formatting.Indented);
            string dataFilePath = Path.Combine(dataFolder, $"{typeName}.json");
            File.WriteAllText(dataFilePath, jsonText);
        }

        public static T? LoadFromDisk<T>()
        {
            var commonAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var dataFolder = Path.Combine(commonAppDataFolder, Library.Constants.FriendlyName);

            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            var type = typeof(T);
            string typeName = type.IsGenericType ? type.GetGenericArguments()[0].Name : type.Name;

            string dataFilePath = Path.Combine(dataFolder, $"{typeName}.json");
            if (File.Exists(dataFilePath))
            {
                var jsonText = File.ReadAllText(dataFilePath);
                var obj = JsonConvert.DeserializeObject<T>(jsonText);
                return obj;
            }
            return default;
        }
    }
}
