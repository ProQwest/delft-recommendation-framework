using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PipelineDataProcessor
{
    public class PipelineContext
    {
        private Dictionary<string, object> _items;
        private string _configPath;
        public List<ProcessConfiguration> _configs;

        public PipelineContext(string configPath)
        {
            _items = new Dictionary<string, object>();
            _configs = new List<ProcessConfiguration>();
            _configPath = configPath;
            LoadConfigurations();
        }

        public void AddConfiguration(ProcessConfiguration pc)
        {
            _configs.Add(pc);
            SaveConfiguration(pc);
        }

        private void SaveConfiguration(ProcessConfiguration pc)
        {
            var sw = new StreamWriter(new FileStream(_configPath, FileMode.Append));
            sw.WriteLine(pc.ToString());
            sw.Close();
        }

        private void LoadConfigurations()
        {
            if (File.Exists(_configPath))
                _configs.AddRange(File.ReadAllLines(_configPath).Select(l => new ProcessConfiguration(l)));
        }

        public string GetAsString(string name)
        {
            string value = this[name] as string;

            if (String.IsNullOrEmpty(value))
                throw new PipelineException(String.Format("The '{0}' does not exists in the context.", name));

            return value;
        }

        public IEnumerable<ProcessConfiguration> Configurations
        {
            get
            {
                return _configs;
            }
        }

        public object this[string name]
        {
            get 
            {
                return _items[name];
            }
            set 
            {
                _items[name] = value;
            }
        }
    }
}
