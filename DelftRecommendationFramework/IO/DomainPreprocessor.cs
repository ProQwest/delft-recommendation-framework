using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PipelineDataProcessor;
using DRF.Data;
using LinqLib.Sequence;

namespace DRF.IO
{
    public class DomainPreprocessor 
    {
        string _path;

        public DomainPreprocessor(string path)
        {
            _path = path;    
        }

        public void CreateTopUserRatingsFile(int maxNumRatings)
        {
            var ratings = File.ReadAllLines(_path).Skip(1).Select(l => new ItemRating(l));

            var itemsRatingCount = ratings.GroupBy(ir => ir.ItemId)
                .Select(g => new { ItemId = g.Key, ItemCount = g.Count() });

            var usersAxRatingVector = ratings.GroupBy(ir => ir.UserId)
                .Select(g =>
                {
                    return g.Join(itemsRatingCount, ui => ui.ItemId, irc => irc.ItemId, (ui, irc) => new { ui, irc.ItemCount })
                        .OrderByDescending(ur => ur.ItemCount)
                        .Take(maxNumRatings)
                        .Select(ur => ur.ui)
                        .Select(ir => string.Format("{0}:{1}", ir.ItemId, ir.Rating))
                        .Aggregate(g.Key + " " + g.Count(), (cur, next) => cur + " " + next);
                });

            string outputPath = string.Format("{0}\\{1}.topusers", _path.GetDirectoryPath(), _path.GetFileName());
            File.WriteAllLines(outputPath, usersAxRatingVector);
        }
    }
}
