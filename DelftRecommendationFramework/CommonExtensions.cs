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

        public static string GetDirectoryPath(this string path)
        {
            return path.Substring(0, path.LastIndexOf('\\'));
        }

        public static string GetFileName(this string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1, path.LastIndexOf('.') - path.LastIndexOf('\\') - 1);
        }

        public static string GetFileExtension(this string path)
        {
            return path.Substring(path.LastIndexOf('.') + 1);
        }
    }

    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Convert a comma separated IEnumerable of lines to field value dictionary
        /// </summary>
        /// <param name="source">IEnumerable of csv including header</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, string>> ToCsvDictionary(this IEnumerable<string> source)
        {
            return ToCsvDictionary(source, ",");
        }

        /// <summary>
        /// Convert a [deliminator] separated IEnumerable of lines to field value dictionary
        /// </summary>
        /// <param name="source">IEnumerable of csv including header</param>
        /// <param name="deliminator">Deliminator character</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, string>> ToCsvDictionary(this IEnumerable<string> source, string deliminator)
        {
            var fields = source.Take(1).FirstOrDefault().Split(new string[] { deliminator }, StringSplitOptions.RemoveEmptyEntries);
            return source.Skip(1)
                .Select(line => line.Split(new string[] { deliminator }, StringSplitOptions.RemoveEmptyEntries)
                    .Zip(fields, (value, field) => new KeyValuePair<string, string>(field, value))
                    .ToDictionary(i => i.Key, i => i.Value));
        }

        public static int GetValue(this Dictionary<string, int> source, string key)
        {
            if (!source.ContainsKey(key))
                source.Add(key, source.Count);
            
            return source[key];
        }

    }
}
