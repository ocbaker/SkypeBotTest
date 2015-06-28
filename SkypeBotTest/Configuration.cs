using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SkypeBotTest
{
    public class Configuration
    {
        private string _path;

        // ReSharper disable once UnusedMember.Local
        private Configuration() { }

        public Dictionary<string, int> AccessLevels { get; set; } = new Dictionary<string, int>()
        {
            {"oliver_c.baker", 255}
        };

        public List<string> RegisteredChats { get; set; } = new List<string>();
        public List<string> RegisteredAdminChats { get; set; } = new List<string>();
        public int Delay { get; set; } = 1000;

        private Configuration(string path)
        {
            _path = path;
        }
        
        #region Load & Save
        public static Configuration LoadConfig(string path)
        {
            if (!File.Exists(path))
            {
                var config = CreateDefaultConfig(path);
                return config;
            }
            else
            {
                var config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(path));
                config._path = path;
                return config;
            }
        }

        private static Configuration CreateDefaultConfig(string path)
        {
            var config = new Configuration(path);

            config.Save();

            return config;
        }

        public void Save()
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(this));
        }


        #endregion
    }
}