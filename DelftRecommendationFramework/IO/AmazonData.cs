using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite.Data;
using System.IO;
using DRF.Data;
using LinqLib.Sequence;

namespace DRF.IO
{
    public static class AmazonData
    {

        public static void ConvertToMovieLensFormat(string fileName, string outputFile)
        {
            var reader = new StreamReader(fileName);
            var writer = new StreamWriter(outputFile);

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
                    //int numReviews = Convert.ToInt32(tline.Split(' ')[2]);
                    string rline;

                    while ((rline = reader.ReadLine()) != "")
                    {
                        product.Reviews.Add(new ProductReview(rline.Trim()));
                    }
                }
                else if (tline.StartsWith("discontinued"))
                    product.ASIN = "";
                else if (tline.StartsWith("Id") && !String.IsNullOrEmpty(product.ASIN))
                {
                    if (product.Group == ProductGroup.Book)
                    {
                        foreach (ProductReview pr in product.Reviews)
                        {
                            writer.WriteLine(string.Format("{0}::{1}::{2}::{3}", pr.UserId, product.ASIN, pr.Rating, pr.Date.ToUnixEpoch()));
                        }
                    }
                    product = new AmazonProduct();
                    writer.Flush();
                }

            }


            reader.Close();
            writer.Close();
        }

        public static void ReadTrainTest(string path, float trainPortion, out IRatings trainData, out IRatings testData)
        {
            var lines = File.ReadAllLines(path).Shuffle();

            var ratings = lines.Select(l =>
            {
                var parts = l.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                return new { User = parts[0], Item = parts[1], Rating = float.Parse(parts[2]), Timespan = parts[3] };
            }).ToList();

            var usersMap = new Mapping();
            var itemsMap = new Mapping();

            var trainRatings = new Ratings();
            var testRatings = new Ratings();

            int trainCount = Convert.ToInt32(ratings.Count * trainPortion);
            
            var train = ratings.Take(trainCount);
            var test = ratings.Skip(trainCount);

            foreach (var r in train)
                trainRatings.Add(usersMap.ToInternalID(r.User), itemsMap.ToInternalID(r.Item), r.Rating);

            foreach (var r in test)
                testRatings.Add(usersMap.ToInternalID(r.User), itemsMap.ToInternalID(r.Item), r.Rating);

            trainData = trainRatings;
            testData = testRatings;
            
        }
    }
}
