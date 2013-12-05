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
using DRF.Data;

namespace DRF
{
    class Program
    {
        static void Main(string[] args)
        {
            //Execute();
            //AmazonData.PrintNoBooks();

            //RunPreprocessors();
            RunPipeline();
            Console.WriteLine("Finished!");
            Console.ReadKey();
        }

        public static void RunPreprocessors()
        {
            var domainPaths = new string[] { "books_selected.csv", "music_selected.csv", "video_selected.csv", "dvd_selected.csv" }
                .Select(d => @"D:\Data\Datasets\Amazon\" + d).ToList();

            domainPaths.Skip(1).Take(3).ToList()
                .ForEach(d => new DomainPreprocessor(d).CreateTopUserRatingsFile(20));
        }
        
        public static void RunPipeline()
        {
            var domainPaths = new string[] { "books_selected.csv", "music_selected.csv", "video_selected.csv", "dvd_selected.csv" }
                .Select(d => @"D:\Data\Datasets\Amazon\" + d).ToList();

            string trDomain = domainPaths.Skip(1).Take(1).Single();
            //string[] auxDomains = domainPaths.Skip(1).Take(1).ToArray();

            var p = new PipelineDataProcessor.Pipeline(@"D:\Data\Datasets\Amazon\Configs.txt");

            //p.AddProcessor(new CrossDomainer(trDomain, auxDomains, 0.75, false));
            //p.AddProcessor(new MovieLensFormatter(@"D:\Data\Datasets\Amazon\amazon-meta.txt", false));
            //p.AddProcessor(new AmazonDataFilter(50));
            //p.AddProcessor(new AmazonToLibFmFormatter(new ProductGroup[] { ProductGroup.Book }));
            //p.AddProcessor(new CsvToMovieLensFormatter(@"D:\Data\Datasets\Amazon\books_selected.csv"));
            //p.AddProcessor(new CrossDomainer(@"D:\Data\Datasets\Amazon\music_selected.csv"));
            p.AddProcessor(new LibFmFeaturesExpander(trDomain, 1));
            p.AddProcessor(new TrainTestSpilitter(0.75, "LibFmFile", false)); //Can be: MovieLensFile or LibFmFile
            //p.AddProcessor(new MediaLiteTester(new MatrixFactorization()));
            //p.AddProcessor(new MediaLiteTester(new BiasedMatrixFactorization()));
            //p.AddProcessor(new LibFmFormatter());
            p.AddProcessor(new LibFmTester());
            //p.AddProcessor(new ResultsProcessor(@"D:\Data\Datasets\Amazon\Configs.txt"));
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
