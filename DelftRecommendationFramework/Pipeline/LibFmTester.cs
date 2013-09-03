using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PipelineDataProcessor;
using System.Diagnostics;
using System.Threading;

namespace DRF.Pipeline
{
    public class LibFmTester : IProcessor, IProcessorInfo
    {
        private string _libFmPath = @"D:\Programs\libfm-1.40.windows\libfm.exe";
        private string _additionalArgs = "";

        public double LearningRate { get; set; } 
        public int Iterations { get; set; }
        public string Dimensions { get; set; }
        public FmLearnigAlgorithm LearningAlgorithm { get; set; }

        public LibFmTester(string additionalArgs = "")
        {
            _additionalArgs = additionalArgs;
    
            //Intialize properties
            LearningRate = 0.1;
            Iterations = 50;
            Dimensions = "1,1,8";
            LearningAlgorithm = FmLearnigAlgorithm.MCMC;
        }

        public void Process(PipelineContext context)
        {
            string libFmTrain = context.GetAsString("LibFmTrain");
            string libFmTest = context.GetAsString("LibFmTest");

            var libFm = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _libFmPath,
                    Arguments = BuildArguments(libFmTrain, libFmTest),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            string libFMTrainRMSE = "", libFMTestRMSE = "";

            libFm.OutputDataReceived += (p, dataLine) =>
            {
                var data = dataLine.Data;

                if (data != null && (data.StartsWith("Loading") || data.StartsWith("#")))
                {
                    Console.WriteLine(dataLine.Data);

                    // Store the results of this apprach in pipeline context
                    if (data.StartsWith("#Iter"))
                    {
                        libFMTrainRMSE = data.Substring(data.IndexOf("Train") + 6, 6);
                        libFMTestRMSE = data.Substring(data.IndexOf("Test") + 6, 6);
                    }
                }
            };
            
            libFm.Start();
            libFm.BeginOutputReadLine();
            libFm.WaitForExit();

            context["LibFmTrainRMSE"] = libFMTrainRMSE;
            context["LibFmTestRMSE"] = libFMTestRMSE;
        }

        private string BuildArguments(string trainFile, string testFile)
        {
            return String.Format("-task r -train {0} -test {1} -method {2} -iter {3} -dim {4} -learn_rate {5} {6}",
                trainFile, testFile, LearningAlgorithm.ToString().ToLower(), Iterations, Dimensions, LearningRate, _additionalArgs);
        }

        public string GetDescription()
        {
            return "Testing Factorization Machines approach with LibFM.";
        }

        public ProcessConfiguration GetConfiguration(PipelineContext context)
        {
            var pc = new ProcessConfiguration();

            pc["LearningAlgorithm"] = LearningAlgorithm.ToString();
            pc["Iterations"] = Iterations.ToString();
            pc["Dimensions"] = Dimensions;
            pc["LearningRate"] = LearningRate.ToString();
            pc["TrainFile"] = context.GetAsString("LibFmTrain");
            pc["TestFile"] = context.GetAsString("LibFmTest");
            pc["AdditionalArgs"] = _additionalArgs;

            return pc;
        }


        public ProcessResult GetProcessResult(PipelineContext context)
        {
            var pr = new ProcessResult();
            pr["LibFmTrainRMSE"] = context.GetAsString("LibFmTrainRMSE");
            pr["LibFmTestRMSE"] = context.GetAsString("LibFmTestRMSE");

            return pr;
        }
    }

    public enum FmLearnigAlgorithm
    { 
        MCMC,
        SGD,
        SGDA,
        ALS
    }
}   
