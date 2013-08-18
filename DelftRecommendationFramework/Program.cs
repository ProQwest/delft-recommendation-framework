using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using DRF.IO;

namespace DRF
{
    class Program
    {
        static void Main(string[] args)
        {
            //AmazonData.ConvertToMovieLensFormat(@"D:\Data\Datasets\Amazon\amazon-meta.txt",
            //    @"D:\Data\Datasets\Amazon\amazon-books-ml-format.txt");

            IRatings train;
            IRatings test;

            AmazonData.ReadTrainTest(@"D:\Data\Datasets\Amazon\amazon-books-ml-format.txt", (float) 0.6, out train, out test);

            var recom = new UserItemBaseline();
            recom.Ratings = train;
            recom.Train();

            // measure the accuracy on the test data set
            var results = recom.Evaluate(test);
            Console.WriteLine("RMSE={0} MAE={1}", results["RMSE"], results["MAE"]);
            Console.WriteLine(results);
            
            
            Console.ReadKey();
        }
    }
}
