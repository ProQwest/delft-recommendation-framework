using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineDataProcessor
{
    public interface IProcessorInfo
    {
        ProcessConfiguration GetConfiguration(PipelineContext context);
        ProcessResult GetProcessResult(PipelineContext context);
    }
}
