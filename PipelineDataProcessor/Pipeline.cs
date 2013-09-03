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
        private string _configPath = "";

        public Pipeline(string configPath)
        {
            _processors = new List<IProcessor>();
            _configPath = configPath;    
        }

        public Pipeline AddProcessor(IProcessor processor)
        {
            _processors.Add(processor);
            return this;
        }

        public void Execute()
        {
            Console.WriteLine("Executing pipeline...");

            PipelineContext context = new PipelineContext(_configPath);
            
            for (int i = 0; i < _processors.Count; i++)
            {
                var p = _processors[i];
                Console.WriteLine(String.Format("\nProcessor {0}: {1}", i + 1, p.GetDescription()));
                Console.WriteLine("-----------------------------------------------------------------");

                if (p is IConfigurationInfo)
                {
                    ProcessConfiguration pc = (p as IConfigurationInfo).GetConfiguration(context);

                    if (!context.Configurations.Contains(pc, new ConfigurationComparer()))
                    {
                        context.AddConfiguration(pc);
                        p.Process(context);
                    }
                    else
                        Console.WriteLine("==> this process is already executed in context.");
                }
                else
                    p.Process(context);
            }
        }
    }
}
