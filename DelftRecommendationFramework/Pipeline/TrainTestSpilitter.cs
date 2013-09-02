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
        
        public TrainTestSpilitter(double trainRatio, bool useCash = true)
            : base(useCash)
        {
            TrainRatio = trainRatio;
        }
        
        public override void Process(PipelineContext context)
        {
            string movieLensFile = context["MovieLensFile"] as string;

            if (String.IsNullOrEmpty(movieLensFile))
                throw new PipelineException("The 'MovieLensFile' does not exists in the context.");

            string trainFile = movieLensFile.GetDirectoryPath() + "\\" + movieLensFile.GetFileName() + "-train" + TrainRatio.ToString() + ".ml";
            string testFile = movieLensFile.GetDirectoryPath() + "\\" + movieLensFile.GetFileName() + "-test" + TrainRatio.ToString() + ".ml";

            if (!File.Exists(trainFile) || !UseCach)
            {
                var lines = File.ReadAllLines(movieLensFile).Shuffle().ToList();
                int trainCount = (int)(TrainRatio * lines.Count());
                File.WriteAllLines(trainFile, lines.Take(trainCount));
                File.WriteAllLines(testFile, lines.Skip(trainCount));
            }
            else
            {
                Console.WriteLine("==> loading cached data");
            }

            context["MovieLensTrain"] = trainFile;
            context["MovieLensTest"] = testFile;
        }

        public override string GetDescription()
        {
            return "Splitting data to train and test portions.";
        }
    }
}
