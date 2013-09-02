using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineDataProcessor
{
    public class PipelineException : Exception
    {
        public PipelineException(string msg) : base(msg) 
        { 
        
        }
    }
}
