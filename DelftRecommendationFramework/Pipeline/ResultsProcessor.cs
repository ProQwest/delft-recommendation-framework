using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PipelineDataProcessor;

namespace DRF.Pipeline
{
    public class ResultsProcessor : IProcessor
    {
        string _configPath;
        
        public ResultsProcessor(string configPath)
        {
            _configPath = configPath;
        }
        
        public void Process(PipelineContext context)
        {
            File.ReadAllLines(_configPath).Select(l => new ProcessConfiguration(l))
                .ToList().ForEach(pc => 
                {
                    Console.WriteLine("---------------------------------------------------------");
                    pc.PrintConfiguration();

                    Console.WriteLine();
                    pc.ProcessResult.PrintResults();
                });
        }

        public string GetDescription()
        {
            return "Summerizing the results of all experiments.";
        }
    }
}
