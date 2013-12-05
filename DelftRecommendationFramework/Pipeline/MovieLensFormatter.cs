using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineDataProcessor;
using DRF.Data;
using System.IO;

namespace DRF.Pipeline
{
    public class MovieLensFormatter : CacheBasedProcessor
    {
        private string _amazonDataFile;
        public string MovieLensFile { get; private set; }
        private IList<AmazonProduct> _products;

        public MovieLensFormatter(string amazonDataFile, bool useCache = true) : base(useCache)
        {
            _amazonDataFile = amazonDataFile;
            _products = new List<AmazonProduct>();
            MovieLensFile = _amazonDataFile.GetDirectoryPath() + "\\" + _amazonDataFile.GetFileName() + "-mb.ml";
        }

        public override void Process(PipelineContext context)
        {
            if (!File.Exists(MovieLensFile) || !UseCach)
                ConvertToMovieLensFormat();
            else
                Console.WriteLine("==> loading cached data");

            //context["MovieLensFile"] = MovieLensFile;
            context["AmazonProducts"] = _products;
            context["AmazonDataFile"] = _amazonDataFile;
        }

        public override string GetDescription()
        {
            return "Converting amazon data file to the MovieLens format.";
        }

        private void ConvertToMovieLensFormat()
        {
            var reader = new StreamReader(_amazonDataFile);
            var writer = new StreamWriter(MovieLensFile);

            string line;
            var product = new AmazonProduct();

            while ((line = reader.ReadLine()) != null)
            {
                var tline = line.Trim();

                if (tline.StartsWith("ASIN"))
                    product.ASIN = tline.Split(' ')[1];
                else if (tline.StartsWith("title"))
                    product.Title = tline.Split(' ')[1];
                else if (tline.StartsWith("group"))
                {
                    ProductGroup pg;
                    if (Enum.TryParse<ProductGroup>(tline.Split(' ')[1], out pg))
                        product.Group = pg;
                    else
                    {
                        Console.WriteLine(tline.Split(' ')[1]);
                        product.Group = ProductGroup.Other;
                    }
                }
                else if (tline.StartsWith("salerank"))
                    product.SalesRank = Convert.ToInt32(tline.Split(' ')[1]);
                else if (tline.StartsWith("reviews"))
                {
                    string rline;

                    while ((rline = reader.ReadLine()) != "")
                    {
                        product.Reviews.Add(new ProductReview(rline.Trim(), product));
                    }
                }
                else if (tline.StartsWith("discontinued"))
                    product.ASIN = "";
                else if (tline.StartsWith("Id") && !String.IsNullOrEmpty(product.ASIN))
                {
                    //if (product.Group == ProductGroup.Book || product.Group == ProductGroup.Music)
                    //{
                    //    foreach (ProductReview pr in product.Reviews)
                    //    {
                    //        writer.WriteLine(string.Format("{0}::{1}::{2}::{3}", pr.UserId, product.ASIN, pr.Rating, pr.Date.ToUnixEpoch()));
                    //    }
                    //}
                    //writer.Flush();

                    _products.Add(product);
                    
                    product = new AmazonProduct();
                }
            }
            
            reader.Close();
            writer.Close();
        }


    }
}
