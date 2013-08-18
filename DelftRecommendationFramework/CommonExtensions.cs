using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRF
{
    public static class CommonExtensions
    {
        public static double ToUnixEpoch(this DateTime datetime)
        {
            return (datetime - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds;
        }
    }
}
