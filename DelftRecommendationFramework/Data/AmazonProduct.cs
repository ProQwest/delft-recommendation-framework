using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRF.Data
{
    public class AmazonProduct
    {
        public string ASIN { get; set; }

        public string Title { get; set; }

        public ProductGroup Group { get; set; }

        public int SalesRank { get; set; }

        public List<String> SimilarProducts { get; set; }

        public List<ProductReview> Reviews { get; set; }

        public AmazonProduct()
        {
            SimilarProducts = new List<string>();
            Reviews = new List<ProductReview>();
        }
    }

    public class ProductReview : ItemRating
    {
        public DateTime Date { get; set; }

        public int Votes { get; set; }

        public int Helpful { get; set; }

        public ProductReview(string line)
        {
            // Sample line: 2000-7-28  cutomer: A2JW67OY8U6HHK  rating: 5  votes:  10  helpful:   9
            var tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Date = Convert.ToDateTime(tokens[0]);
            UserId = tokens[2];
            Rating = Convert.ToInt32(tokens[4]);
            Votes = Convert.ToInt32(tokens[6]);
            Helpful = Convert.ToInt32(tokens[8]);
        }
    }


    
    public enum ProductGroup
    { 
        Book,
        Video,
        DVD,
        Music,
        Toy,
        Software,
        Baby,
        CE,
        Sports,
        Other
    }
}
