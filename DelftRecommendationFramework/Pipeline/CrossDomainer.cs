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
    public class CrossDomainer : IProcessor, IProcessorInfo
    {
        string _targetDomainPath;
        string[] _auxDomainsPath;
        
        // This property indicate whether the auxlary data should be considered as seperate domain or same domain
        public bool CrossDomain { get; set; }
        
        public double TrainRatio { get; set; }

        public CrossDomainer(string targetDomainPath, string[] auxDomainsPath, double trainRatio, bool crossDomain)
        {
            _targetDomainPath = targetDomainPath;
            _auxDomainsPath = auxDomainsPath;
            TrainRatio = trainRatio;
            CrossDomain = crossDomain;
        }
        
        public void Process(PipelineContext context)
        {
            var featDic = new Dictionary<string, int>();
            
            // xd: cross domain, nd: no cross domain
            string trainFile = String.Format("{0}\\{1}-train{2}-{3}.libfm",
                _targetDomainPath.GetDirectoryPath(),
                _targetDomainPath.GetFileName(),
                TrainRatio.ToString(),
                CrossDomain ? "xd" : "nd");

            string testFile = String.Format("{0}\\{1}-test{2}-{3}.libfm",
                _targetDomainPath.GetDirectoryPath(),
                _targetDomainPath.GetFileName(),
                TrainRatio.ToString(),
                CrossDomain ? "xd" : "nd");
            
            // Split target doamin to train and test
            var tdLines = GetLibFmLines(_targetDomainPath, featDic).Shuffle().ToList();
            int tdTrainCount = (int)(TrainRatio * tdLines.Count());
            var tdTrain = tdLines.Take(tdTrainCount).ToList();
            var tdTest = tdLines.Skip(tdTrainCount).ToList();

            var axTrain = _auxDomainsPath.SelectMany(path => GetLibFmLines(path, featDic)).ToList();

            File.WriteAllLines(trainFile, tdTrain.Concat(axTrain));
            File.WriteAllLines(testFile, tdTest);

            context["TrainFile"] = trainFile;
            context["TestFile"] = testFile;

            context["TargetDomainTrainCount"] = tdTrain.Count;
            context["TargetDomainTest"] = tdTest.Count;
            context["AuxDomainsCount"] = axTrain.Count;
        }

        private IEnumerable<string> GetLibFmLines(string path, Dictionary<string, int> featDic)
        {
            string domainName = CrossDomain ? path : "SameDomain";

            return File.ReadAllLines(path).ToCsvDictionary()
                .Select(i => new ItemRating() { ItemId = i["ItemId"] + "a", UserId = i["UserId"], Rating = int.Parse(i["Rating"]) })
                .Select(r => String.Format("{0} {1}:1 {2}:1 {3}:1",
                    r.Rating, featDic.GetValue(domainName), featDic.GetValue(r.UserId), featDic.GetValue(r.ItemId + path)));
        }

        public string GetDescription()
        {
            return "Getting additional information from auxiliary domain.";
        }

        public ProcessConfiguration GetConfiguration(PipelineContext context)
        {
            var pc = new ProcessConfiguration();

            pc["TrainRatio"] = TrainRatio.ToString();
            pc["CrossDomainAuxData"] = CrossDomain.ToString();
            pc["AuxDomainFiles"] = _auxDomainsPath.Length > 0 ? _auxDomainsPath.Aggregate((cur, next) => cur + "," + next.GetFileName()) : "";

            return pc;
        }

        public ProcessResult GetProcessResult(PipelineContext context)
        {
            var pr = new ProcessResult();

            pr["TrainFile"] = context.GetAsString("TrainFile");
            pr["TestFile"] = context.GetAsString("TestFile");
            pr["TargetDomainTrainCount"] = context.GetAsString("TargetDomainTrainCount");
            pr["TargetDomainTest"] = context.GetAsString("TargetDomainTest");
            pr["AuxDomainsCount"] = context.GetAsString("AuxDomainsCount");

            return pr;
        }
    }
}
