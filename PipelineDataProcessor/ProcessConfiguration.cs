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

        public ProcessConfiguration()
        {
            _items = new Dictionary<string, string>();
        }

        public ProcessConfiguration(string data) : this()
        {
            data.Split('\t').ToList().ForEach(kvp =>
                {
                    var tokens = kvp.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                    _items.Add(tokens[0], tokens[1]);
                });
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
                .Aggregate((cur, next) => cur + "\t" + next);
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
