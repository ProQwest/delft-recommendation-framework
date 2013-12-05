using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PipelineDataProcessor;

namespace DRF.Pipeline
{
    public class CsvToMovieLensFormatter : IProcessor
    {
        private string _csvFile;

        public CsvToMovieLensFormatter(string fileName)
        {
            _csvFile = fileName;
        }


        public void Process(PipelineContext context)
        {
            var output = File.ReadAllLines(_csvFile).ToCsvDictionary()
                .Select(i => new { UserId = i["UserId"], ItemId = i["ItemId"], Rating = i["Rating"] })
                .Select(i => String.Format("{0}::{1}::{2}", i.UserId, i.ItemId, i.Rating));

            string outputFile = String.Format("{0}\\{1}.ml", _csvFile.GetDirectoryPath(), _csvFile.GetFileName());

            File.WriteAllLines(outputFile, output);
            
            context["MovieLensFile"] = outputFile;
        }

        public string GetDescription()
        {
            return "Convert csv rating file to MovieLens format.";
        }
    }
}
