using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineDataProcessor
{
    public class ProcessResult
    {
        private Dictionary<string, string> _items;

        public ProcessResult()
        {
            _items = new Dictionary<string, string>();
        }

        public ProcessResult(string data)
            : this()
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

        public void PrintResults()
        {
            Console.WriteLine("==> Process results:");
            _items.Select(kvp => String.Format(" -- {0}: {1}", kvp.Key, kvp.Value))
                .ToList().ForEach(Console.WriteLine);
        }

    }
}
