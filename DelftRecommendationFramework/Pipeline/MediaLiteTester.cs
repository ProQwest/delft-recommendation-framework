using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PipelineDataProcessor;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite;

namespace DRF.Pipeline
{
    public class MediaLiteTester : IProcessor, IProcessorInfo
    {
        private IRecommender _recommender;
        
        public MediaLiteTester(IRecommender recommender)
        {
            _recommender = recommender;
        }
        
        public void Process(PipelineContext context)
        {
            string movieLensTrain = context.GetAsString("MovieLensTrain");
            string movieLensTest = context.GetAsString("MovieLensTest");

            var usersMap = new Mapping();
            var itemsMap = new Mapping();
            
            IRatings trainSet = ReadData(movieLensTrain, usersMap, itemsMap);
            IRatings testSet = ReadData(movieLensTest, usersMap, itemsMap);

            var recom = _recommender as IRatingPredictor;
            recom.Ratings = trainSet;

            Console.WriteLine("Training...");
            recom.Train();

            Console.WriteLine("Testing...");
            var results = recom.Evaluate(testSet);
            
            context["MediaLite_RMSE"] = results["RMSE"];
            context["MediaLite_MAE"] = results["MAE"];
        }

        public string GetDescription()
        {
            return "Testing Matrix Faxtorization method with MyMediaLite.";
        }

        private IRatings ReadData(string path, IMapping usersMap, IMapping itemsMap)
        {
            var ratings = new MyMediaLite.Data.Ratings();
            
            File.ReadAllLines(path).Select(l =>
            {
                var parts = l.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                return new { User = parts[0], Item = parts[1], Rating = float.Parse(parts[2]) };
            }).ToList()
            .ForEach(r => 
                ratings.Add(usersMap.ToInternalID(r.User), itemsMap.ToInternalID(r.Item), r.Rating));

            return ratings;
        }

        public ProcessConfiguration GetConfiguration(PipelineContext context)
        {
            var pc = new ProcessConfiguration();
            pc["Recommender"] = _recommender.GetType().Name;
            pc["TrainFile"] = context.GetAsString("MovieLensTrain");
            pc["TestFile"] = context.GetAsString("MovieLensTest");

            return pc;
        }


        public ProcessResult GetProcessResult(PipelineContext context)
        {
            var pr = new ProcessResult();
            pr["MediaLite_RMSE"] = context.GetAsString("MediaLite_RMSE");
            pr["MediaLite_MAE"] = context.GetAsString("MediaLite_MAE");

            return pr;
        }
    }
}
