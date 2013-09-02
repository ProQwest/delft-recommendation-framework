using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineDataProcessor
{
    public abstract class CacheBasedProcessor : IProcessor
    {
        public bool UseCach { get; set; }

        public CacheBasedProcessor(bool useCach)
        {
            UseCach = useCach;
        }

        public abstract void Process(PipelineContext context);

        public abstract string GetDescription();
    }
}
