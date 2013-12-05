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
        private string _testOutput = "";
        public double LearningRate { get; set; } 
        public int Iterations { get; set; }
        public string Dimensions { get; set; }
        public string Regularization { get; set; }
        public FmLearnigAlgorithm LearningAlgorithm { get; set; }
        public bool StopWhenNoImprovment { get; set; }

        public LibFmTester(string additionalArgs = "")
        {
            _additionalArgs = additionalArgs;
    
            //Intialize properties
            LearningRate = 0.05;
            Iterations = 280;
            Dimensions = "1,1,10";
            LearningAlgorithm = FmLearnigAlgorithm.MCMC;
            Regularization = "0,0,0.1";
            StopWhenNoImprovment = true;
        }

        public void Process(PipelineContext context)
        {
            string libFmTrain = context.GetAsString("TrainFile");
            string libFmTest = context.GetAsString("TestFile");

            _testOutput = string.Format("{0}\\{1}.out", libFmTest.GetDirectoryPath(), libFmTest.GetFileName());

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

            double prevRMSE = double.MaxValue;
            int iter = 0;

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
                        libFMTestRMSE = data.Substring(data.IndexOf("Test") + 5);
                        
                        // Stop libFm if there was no improvement in RMSE
                        // Note: stopping LibFm will prevent the output file to be created
                        if (StopWhenNoImprovment)
                        {
                            double current = double.Parse(libFMTestRMSE);

                            if (current > prevRMSE)
                                libFm.Kill();
                            else
                                prevRMSE = current;
                        }
                        iter++;
                    }
                }
            };
            
            libFm.Start();
            libFm.BeginOutputReadLine();
            libFm.WaitForExit();

            context["LibFmTrainRMSE"] = libFMTrainRMSE;
            context["LibFmTestRMSE"] = libFMTestRMSE;
            
            // If this file exists it means the process finished normally and the MAE can be calculated
            if (File.Exists(_testOutput))
                context["LibFmTestMAE"] = CalculateMAE(libFmTest);
            else
                context["LibFmTestMAE"] = "LibFM exited at iter " + iter;
        }

        private string BuildArguments(string trainFile, string testFile)
        {
            return String.Format("-task r -train {0} -test {1} -method {2} -iter {3} -dim {4} -learn_rate {5} -out {6} -regular {7} {8}",
                trainFile, testFile, LearningAlgorithm.ToString().ToLower(), Iterations, Dimensions, LearningRate, _testOutput, Regularization, _additionalArgs);
        }

        private double CalculateMAE(string libFmTestFile)
        {
            var actual = File.ReadAllLines(libFmTestFile).Select(l => (double)Char.GetNumericValue(l[0]));
            var predicted = File.ReadAllLines(_testOutput).Select(l => double.Parse(l)).ToList();

            return actual.Zip(predicted, (a, p) => new { Actual = a, Predicted = p })
                .Select(pair => Math.Abs(pair.Actual - pair.Predicted))
                .Sum() / predicted.Count();
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
            pc["TrainFile"] = context.GetAsString("TrainFile");
            pc["TestFile"] = context.GetAsString("TestFile");
            pc["AdditionalArgs"] = _additionalArgs;

            return pc;
        }


        public ProcessResult GetProcessResult(PipelineContext context)
        {
            var pr = new ProcessResult();
            pr["LibFmTrainRMSE"] = context.GetAsString("LibFmTrainRMSE");
            pr["LibFmTestRMSE"] = context.GetAsString("LibFmTestRMSE");
            pr["LibFmTestMAE"] = context.GetAsString("LibFmTestMAE");

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
