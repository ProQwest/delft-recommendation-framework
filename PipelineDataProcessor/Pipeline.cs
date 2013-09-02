using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineDataProcessor
{
    public class Pipeline
    {
        private List<IProcessor> _processors;

        public Pipeline() 
        {
            _processors = new List<IProcessor>();
        }

        public Pipeline AddProcessor(IProcessor processor)
        {
            _processors.Add(processor);
            return this;
        }

        public void Execute()
        {
            Console.WriteLine("Executing pipeline...");

            PipelineContext context = new PipelineContext();
            
            for (int i = 0; i < _processors.Count; i++)
            {
                var p = _processors[i];
                Console.WriteLine(String.Format("\nProcessor {0}: {1}", i + 1, p.GetDescription()));
                Console.WriteLine("-----------------------------------------------------------------");
                p.Process(context);
            }
        }
    }
}
