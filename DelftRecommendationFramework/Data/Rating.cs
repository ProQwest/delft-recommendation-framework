using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRF.Data
{
    public class ItemRating
    {
        public string UserId { get; set; }

        public int Rating { get; set; }

        public string ItemId { get; set; }

        public ItemRating()
        { 
        
        }

        public ItemRating(string line)
            : this(line, "")
        {
        }

        public ItemRating(string line, string itemPrefix)
        {
            string[] tokens = line.Split(new string[] { "::", "," }, StringSplitOptions.RemoveEmptyEntries);

            UserId = tokens[0];
            ItemId = tokens[1] + "d" + itemPrefix;
            Rating = Convert.ToInt32(tokens[2]);
        }

        public override string ToString()
        {
            return String.Format("{0}::{1}::{2}", UserId, ItemId, Rating);
        }
    }
}
