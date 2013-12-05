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

                if (p is IProcessorInfo)
                {
                    var pi = p as IProcessorInfo;

                    ProcessConfiguration pc = pi.GetConfiguration(context);
                    pc.PrintConfiguration();

                    if (!context.Configurations.Contains(pc, new ConfigurationComparer()))
                    {
                        p.Process(context);
                        // Update the Process configuration with results form process
                        pc.ProcessResult = pi.GetProcessResult(context);

                        context.AddConfiguration(pc);
                    }
                    else
                    {
                        Console.WriteLine("==> this process is already executed in context.");
                    }

                    // Get the Correponding Process configuration from context which also includes results
                    var originalPc = context.Configurations.Single(c => new ConfigurationComparer().Equals(c, pc));
                    originalPc.ProcessResult.UpdateContext(context);
                    originalPc.ProcessResult.PrintResults();
                }
                else
                    p.Process(context);
            }
        }
    }
}
