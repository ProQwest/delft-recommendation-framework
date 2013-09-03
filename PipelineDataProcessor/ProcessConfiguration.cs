using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineDataProcessor
{
    public class ProcessConfiguration
    {
        private Dictionary<string, string> _items;
        private ProcessResult _processResult;

        public ProcessConfiguration()
        {
            _items = new Dictionary<string, string>();
            _processResult = new ProcessResult();
        }

        public ProcessConfiguration(string data) : this()
        {
            var parts = data.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            
            parts[0].Split('\t').ToList().ForEach(kvp =>
                {
                    var tokens = kvp.Split(new string[] { "::" }, StringSplitOptions.None);
                    _items.Add(tokens[0], tokens[1]);
                });

            // If the results of configuration is also saved in the file
            if (parts.Length > 1)
                _processResult = new ProcessResult(parts[1]);
        }

        public string this[string name]
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

        public ProcessResult ProcessResult
        {
            get
            {
                return _processResult;
            }
            set
            {
                _processResult = value;
            }
        }

        public Dictionary<string, string> Items
        {
            get 
            {
                return _items;
            }
        }

        public override string ToString()
        {
            return _items.Select(kvp => kvp.Key + "::" + kvp.Value)
                .Aggregate((cur, next) => cur + "\t" + next) + ";;" + _processResult.ToString();
        }

        public void PrintConfiguration()
        {
            Console.WriteLine("==> Process configurations:");
            _items.Select(kvp => String.Format(" -- {0}: {1}", kvp.Key, kvp.Value))
                .ToList().ForEach(Console.WriteLine);
        }
    }

    public class ConfigurationComparer : IEqualityComparer<ProcessConfiguration>
    {

        public bool Equals(ProcessConfiguration x, ProcessConfiguration y)
        {
            if (x.Items.Count != y.Items.Count)
                return false; 

            foreach (var kvp in x.Items)
            {
                string yValue;
                if (!y.Items.TryGetValue(kvp.Key, out yValue))
                    return false; // key missing in b
                if (!Equals(kvp.Value, yValue))
                    return false; // value is different
            }
            return true;
        }

        public int GetHashCode(ProcessConfiguration obj)
        {
            return obj.Items.GetHashCode();
        }
    }
}
