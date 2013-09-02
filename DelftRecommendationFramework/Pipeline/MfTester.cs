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

namespace DRF.Pipeline
{
    public class MfTester : IProcessor
    {
        
        public void Process(PipelineContext context)
        {
            string movieLensTrain = context["MovieLensTrain"] as string;
            string movieLensTest = context["MovieLensTest"] as string;

            if (String.IsNullOrEmpty(movieLensTrain))
                throw new PipelineException("The 'MovieLensTrain' does not exists in the context.");
            
            var usersMap = new Mapping();
            var itemsMap = new Mapping();
            
            IRatings trainSet = ReadData(movieLensTrain, usersMap, itemsMap);
            IRatings testSet = ReadData(movieLensTest, usersMap, itemsMap);

            var recom = new MatrixFactorization();
            recom.Ratings = trainSet;

            Console.WriteLine("Training...");
            recom.Train();

            Console.WriteLine("Testing...");
            var results = recom.Evaluate(testSet);

            Console.WriteLine(results);
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
    }
}
