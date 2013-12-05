using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRF.Data;
using PipelineDataProcessor;
using System.IO;

namespace DRF.Pipeline
{
    public class AmazonToLibFmFormatter : IProcessor, IProcessorInfo
    {
        ProductGroup[] _productGroups;

        public AmazonToLibFmFormatter(ProductGroup[] productGroups)
        {
            _productGroups = productGroups;    
        }
        
        public void Process(PipelineContext context)
        {
            var products = context["AmazonProducts"] as IList<AmazonProduct>;
            string amazonDataFile = context.GetAsString("AmazonDataFile");

            var outputFile = string.Format("{0}\\{1}4.libfm", amazonDataFile.GetDirectoryPath(), amazonDataFile.GetFileName());
            var featDic = new Dictionary<string, int>();

            ConvertToLibFmFile(products, outputFile, featDic);

            context["LibFmFile"] = outputFile;
        }

        public string GetDescription()
        {
            return "Converting Amazon data objects to LibFM format.";
        }

        private void ConvertToLibFmFile(IList<AmazonProduct> products, string outputFile, Dictionary<string, int> featDic)
        {
            var output = products.Where(p => _productGroups.Contains(p.Group))
                .SelectMany(p => p.Reviews)
                .Select(pr =>
                {
                    return String.Format("{0} {1}:1 {2}:1 {3}:1 {4}:{5}",
                        pr.Rating,
                        featDic.GetValue(pr.Product.Group.ToString()),
                        featDic.GetValue(pr.UserId),
                        featDic.GetValue(pr.ItemId),
                        featDic.GetValue("SalesRank"),
                        pr.Product.SalesRank);
                });

            File.WriteAllLines(outputFile, output);
        }


        public ProcessConfiguration GetConfiguration(PipelineContext context)
        {
            var pc = new ProcessConfiguration();
            pc["ProductGroups"] = _productGroups.Select(pg => pg.ToString())
                .Aggregate((cur, next) => cur + "," + next);
            pc["AdditionalFeatures"] = "SalesRank";

            return pc;
        }

        public ProcessResult GetProcessResult(PipelineContext context)
        {
            var pr = new ProcessResult();
            pr["LibFmFile"] = context.GetAsString("LibFmFile");

            return pr;
        }
    }
}
