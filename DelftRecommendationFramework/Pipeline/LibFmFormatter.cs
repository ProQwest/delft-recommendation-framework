using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PipelineDataProcessor;
using DRF.Data;

namespace DRF.Pipeline
{
    public class LibFmFormatter : CacheBasedProcessor
    {
        public LibFmFormatter(bool useCach = true) : base(useCach)
        {

        }

        public override void Process(PipelineContext context)
        {
            string movieLensTrain = context.GetAsString("MovieLensTrain");
            string movieLensTest = context.GetAsString("MovieLensTest");

            string libFmTrain = movieLensTrain.GetDirectoryPath() + "\\" + movieLensTrain.GetFileName() + ".libfm";
            string libFmTest = movieLensTest.GetDirectoryPath() + "\\" + movieLensTest.GetFileName() + ".libfm";

            if (!File.Exists(libFmTrain) || !UseCach)
            {
                var featDic = new Dictionary<string, int>();

                ConvertToLibFmFile(movieLensTrain, libFmTrain, featDic);
                ConvertToLibFmFile(movieLensTest, libFmTest, featDic);
            }
            else
                Console.WriteLine("==> loading cached data");

            context["LibFmTrain"] = libFmTrain;
            context["LibFmTest"] = libFmTest;
        }

        private void ConvertToLibFmFile(string inputFile, string outputFile, Dictionary<string, int> featDic)
        {
            var trDomain = File.ReadAllLines(inputFile).Select(l => new ItemRating(l))
                .Select(ir =>
                {
                    return String.Format("{0} {1}:1 {2}:1 {3}:1",
                        ir.Rating,
                        featDic.GetValue("BookDomain"),
                        featDic.GetValue(ir.UserId),
                        featDic.GetValue(ir.ItemId));
                });

            File.WriteAllLines(outputFile, trDomain);
        }

        public override string GetDescription()
        {
            return "Converting the MovieLens formatted data to LibFM formatted.";
        }
    }
}
