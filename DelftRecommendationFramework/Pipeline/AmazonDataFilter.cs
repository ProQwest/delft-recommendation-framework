using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PipelineDataProcessor;
using DRF.Data;
using LinqLib.Sequence;

namespace DRF.Pipeline
{
    public class AmazonDataFilter : CacheBasedProcessor, IProcessorInfo
    {
        private int _minNumRatings;

        public AmazonDataFilter(int minNumRatings, bool useCache = true) : base(useCache)
        {
            _minNumRatings = minNumRatings;
        }

        public override void Process(PipelineContext context)
        {
            string movieLensFile = context.GetAsString("MovieLensFile");

            var ratings = File.ReadAllLines(movieLensFile).Select(l => new ItemRating(l))
                .GroupBy(r => r.UserId)
                .Where(g => g.Count() >= _minNumRatings)
                .SelectMany(g => g.Select(r => r.ToString()));

            string outputFile = string.Format("{0}\\{1}-filter{2}.ml", 
                movieLensFile.GetDirectoryPath(), movieLensFile.GetFileName(), _minNumRatings);
            
            File.WriteAllLines(outputFile, ratings.Shuffle());

            context["MovieLensFile"] = outputFile;
        }

        public override string GetDescription()
        {
            return "Filtering the Amazon data and selecting a subset of it.";
        }

        public ProcessConfiguration GetConfiguration(PipelineContext context)
        {
            var pc = new ProcessConfiguration();
            pc["MinNumberRatings"] = _minNumRatings.ToString();

            return pc;
        }

        public ProcessResult GetProcessResult(PipelineContext context)
        {
            var pr = new ProcessResult();
            pr["MovieLensFile"] = context.GetAsString("MovieLensFile");

            return pr;
        }
    }
}
