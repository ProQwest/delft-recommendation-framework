using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineDataProcessor
{
    public class PipelineContext
    {
        private Dictionary<string, object> _items;

        public PipelineContext()
        {
            _items = new Dictionary<string, object>();
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
