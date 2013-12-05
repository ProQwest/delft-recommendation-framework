using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PipelineDataProcessor;
using DRF.Data;
using LinqLib.Sequence;

namespace DRF.Pipeline
{
    public class LibFmFeaturesExpander : IProcessor, IProcessorInfo
    {
        string _targetDomainPath;
        Dictionary<string, int> _featDic;

        // This property indicate whether the auxlary data should be considered as seperate domain or same domain
        public bool CrossDomain { get; set; }

        public double TrainRatio { get; set; }

        public int MaxAuxItems { get; set; }

        public LibFmFeaturesExpander(string targetDomainPath, int maxAuxItems)
        {
            _targetDomainPath = targetDomainPath;
            _featDic = new Dictionary<string, int>();
            MaxAuxItems = maxAuxItems;
        }
        
        public void Process(PipelineContext context)
        {
            // ex: no aux
            // ex2: aux music domain
            // ex3: aux music domain (20)
            // ex4: aux all domains separately
            // ex5: aux all domains separately with 5 items from aux domain
            // ex6: aux all domains separately with 10 items from aux domain
            // ex7: other experiments
            // ex8: no aux
            var outputFile = string.Format("{0}\\{1}-ex7.libfm", _targetDomainPath.GetDirectoryPath(), _targetDomainPath.GetFileName());



            //var axUsersPath = _auxDomainsPath.Select(p => string.Format("{0}\\{1}.users", p.GetDirectoryPath(), p.GetFileName()));

            var targetRatings = File.ReadAllLines(_targetDomainPath).Skip(1).Select(l => new ItemRating(l));

            // Put all data in single domain
            //var targetRatings = new string[] { "books_selected.csv", "music_selected.csv", "video_selected.csv", "dvd_selected.csv" }
            //    .Select(d => @"D:\Data\Datasets\Amazon\" + d).SelectMany((p, ix) => File.ReadAllLines(p)
            //    .Skip(1).Select(l => new ItemRating(l, ix.ToString())));


            //var itemsRatingCount = axRatings.GroupBy(ir => ir.ItemId)
            //    .Select(g => new { ItemId = g.Key, ItemCount = g.Count() });

            var axDomainUsers = new string[] { "music_selected.users", "video_selected.users", "dvd_selected.users", "books_selected.users" }
                .Select(d => @"D:\Data\Datasets\Amazon\" + d).ToList();

            var musicRaters = GetExtendedVector(axDomainUsers[0], MaxAuxItems, 1);
            var videoRaters = GetExtendedVector(axDomainUsers[1], MaxAuxItems, 2);
            var dvdRaters = GetExtendedVector(axDomainUsers[2], MaxAuxItems, 3);

            var samples = targetRatings.Select(r =>
            { 
                string musicVector = "", videoVecotr = "", dvdVecotr = "";
                
                musicRaters.TryGetValue(r.UserId, out musicVector);
                videoRaters.TryGetValue(r.UserId, out videoVecotr);
                dvdRaters.TryGetValue(r.UserId, out dvdVecotr);

                return string.Format("{0} {1}:1 {2}:1{3}{4}{5}", 
                    r.Rating, _featDic.GetValue(r.UserId), _featDic.GetValue(r.ItemId + "i"), musicVector, videoVecotr, dvdVecotr);
            });

            /*
            var usersAxRatingVector = axRatings.GroupBy(ir => ir.UserId)
                .Select(g =>
                {
                    var userTopRatings = g.Join(itemsRatingCount, ui => ui.ItemId, irc => irc.ItemId, (ui, irc) => new { ui, irc.ItemCount })
                        .OrderByDescending(ur => ur.ItemCount)
                        .Take(MaxAuxItems)
                        .Select(ur => ur.ui);

                    int userRatingCount = g.Count();

                    var userAxRatingsVector = userTopRatings
                        .Select(utr => string.Format("{0}:{1}", _featDic.GetValue(utr.ItemId), utr.Rating * 1.0 / userRatingCount))
                        .Aggregate("", (cur, next) => cur + " " + next);

                    return new { UserId = g.Key, AxRatingVector = userAxRatingsVector };
                });


            var targetRatingsExtended = from tr in targetRatings
                                        join uav in usersAxRatingVector on tr.UserId equals uav.UserId
                                        into usersVectorJoin
                                        from uav in usersVectorJoin.DefaultIfEmpty(new { UserId = "", AxRatingVector = "" })
                                        select new { TargetRating = tr, userAxVector = uav.AxRatingVector };

            var samples = targetRatingsExtended.Select(tre => string.Format("{0} {1}:1 {2}:1{3}",
                tre.TargetRating.Rating,
                _featDic.GetValue(tre.TargetRating.UserId),
                _featDic.GetValue(tre.TargetRating.ItemId),
                tre.userAxVector));

            */

            File.WriteAllLines(outputFile, samples);

            context["LibFmFile"] = outputFile;
            return;
        }

        private Dictionary<string, string> GetExtendedVector(string usersFilePath, int maxNumRatings, int domainNo)
        {
            return File.ReadAllLines(usersFilePath).Select(l =>
            {
                var tokens = l.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                string userId = tokens[0];
                int ratingsCount = int.Parse(tokens[1]);

                string userRatingVector = tokens.Skip(2).Take(maxNumRatings).Select(r =>
                {
                    var pair = r.Split(':');
                    return string.Format("{0}:{1}", _featDic.GetValue(pair[0] + "d" + domainNo), double.Parse(pair[1]) / ratingsCount);
                }).Aggregate("", (cur, next) => cur + " " + next);

                return new { UserId = userId, UserRatingVector = userRatingVector };
            }).ToDictionary(i => i.UserId, i => i.UserRatingVector);
        }

        public string GetDescription()
        {
            return "Expand the feature vector with informations from auxuilary domains.";
        }

        public ProcessConfiguration GetConfiguration(PipelineContext context)
        {
            var pc = new ProcessConfiguration();
            pc["MaxAuxItems"] = MaxAuxItems.ToString();
            pc["AuxDomainFiles"] = "Music 5";

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
