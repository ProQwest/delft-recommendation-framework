using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineDataProcessor;
using LinqLib.Sequence;
using System.IO;

namespace DRF.Pipeline
{
    public class TrainTestSpilitter : CacheBasedProcessor
    {
        public double TrainRatio { get; set; }
        public String ContextName { get; set; }

        public TrainTestSpilitter(double trainRatio, string contextName, bool useCash = true)
            : base(useCash)
        {
            TrainRatio = trainRatio;
            ContextName = contextName;
        }
        
        public override void Process(PipelineContext context)
        {
            string inputFile = context.GetAsString(ContextName);

            string trainFile = String.Format("{0}\\{1}-train{2}.{3}",
                inputFile.GetDirectoryPath(), inputFile.GetFileName(), TrainRatio.ToString(), inputFile.GetFileExtension());
            string testFile = String.Format("{0}\\{1}-test{2}.{3}",
                inputFile.GetDirectoryPath(), inputFile.GetFileName(), TrainRatio.ToString(), inputFile.GetFileExtension());

            if (!File.Exists(trainFile) || !UseCach)
            {
                Spilit(inputFile, trainFile, testFile, TrainRatio);
            }
            else
            {
                Console.WriteLine("==> loading cached data");
            }

            context["TrainFile"] = trainFile;
            context["TestFile"] = testFile;
        }

        public static void Spilit(string inputFile, string trainFile, string testFile, double trainRatio)
        {
            var lines = File.ReadAllLines(inputFile).Shuffle().ToList();
            int trainCount = (int)(trainRatio * lines.Count());
            File.WriteAllLines(trainFile, lines.Take(trainCount));
            File.WriteAllLines(testFile, lines.Skip(trainCount));
        }

        public override string GetDescription()
        {
            return "Splitting data to train and test portions.";
        }

    }
}
