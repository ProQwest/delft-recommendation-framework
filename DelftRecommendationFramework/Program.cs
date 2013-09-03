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
using PipelineDataProcessor;
using DRF.Pipeline;
using System.Diagnostics;

namespace DRF
{
    class Program
    {
        static void Main(string[] args)
        {
            //Execute();
            //AmazonData.PrintNoBooks();

            RunPipeline();
            Console.WriteLine("Finished!");
            Console.ReadKey();
        }

        public static void RunPipeline()
        {
            var p = new PipelineDataProcessor.Pipeline(@"D:\Data\Datasets\Amazon\Configs.txt");

            p.AddProcessor(new MovieLensFormatter(@"D:\Data\Datasets\Amazon\amazon-meta.txt"));
            p.AddProcessor(new TrainTestSpilitter(0.7));
            p.AddProcessor(new MfTester(new MatrixFactorization()));
            //p.AddProcessor(new LibFmFormatter());
            //p.AddProcessor(new LibFmTester());

            p.Execute();
        }

        
        public static void Execute(int num = 2)
        {
            switch (num)
            { 
                case 1:
                    AmazonData.GetHighRatedUser(@"D:\Data\Datasets\Amazon\amazon-books-ml-format.txt",
                        @"D:\Data\Datasets\Amazon\amazon-books-r30.txt");
                    break;
            }
        }

    }
}
