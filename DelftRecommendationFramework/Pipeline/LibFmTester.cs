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
    public class LibFmTester : IProcessor
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
        }

        public void Process(PipelineContext context)
        {
            string libFmTrain = context["LibFmTrain"] as string;
            string libFmTest = context["LibFmTest"] as string;

            if (String.IsNullOrEmpty(libFmTrain))
                throw new PipelineException("The 'LibFmTrain' does not exists in the context.");

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

            libFm.OutputDataReceived += (p, dataLine) =>
            {
                var data = dataLine.Data;

                if (data != null && (data.StartsWith("Loading") || data.StartsWith("#")))
                {
                    Console.WriteLine(dataLine.Data);

                    // Store the results of this apprach in pipeline context
                    if (data.StartsWith("#Iter"))
                    {
                        context["LibFmTrainRMSE"] = data.Substring(data.IndexOf("Train") + 6, 6);
                        context["LibFmTestRMSE"] = data.Substring(data.IndexOf("Train") + 6, 6);
                    }
                }
            };
            
            libFm.Start();
            libFm.BeginOutputReadLine();
            libFm.WaitForExit();
        }

        private string BuildArguments(string trainFile, string testFile)
        {
            string method = LearningAlgorithm != FmLearnigAlgorithm.MCMC ? LearningAlgorithm.ToString().ToLower() : "mcmc";
            string learningRate = LearningRate > 0 ? LearningRate.ToString() : "0.1";
            string iterations = Iterations > 0 ? Iterations.ToString() : "50";
            string dim = Dimensions ?? "1,1,8";

            return String.Format("-task r -train {0} -test {1} -method {2} -iter {3} -dim {4} -learn_rate {5} {6}", 
                trainFile, testFile, method, iterations, dim, learningRate, _additionalArgs);

        }

        public string GetDescription()
        {
            return "Testing Factorization Machines approach with LibFM.";
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
