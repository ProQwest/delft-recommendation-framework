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

        public static void GetHighRatedUser(string inputFile, string outputFile, int minNumRatings = 30)
        {
            var ratings = File.ReadAllLines(inputFile).Select(l => new ItemRating(l))
                .GroupBy(r => r.UserId)
                .Where(g => g.Count() >= minNumRatings)
                .SelectMany(g => g.Select(r => r.ToString()));

            File.WriteAllLines(outputFile, ratings.Shuffle());
        }

        public static void PrintNoBooks(string file = @"D:\Data\Datasets\Amazon\amazon-books-ml-format.txt", 
            string file2 = @"D:\Data\Datasets\Amazon\amazon-musics-ml-format.txt")
        {

            var topMusicRators = File.ReadAllLines(file2).Select(l => new ItemRating(l))
                .GroupBy(ir => ir.UserId)
                .Where(g => g.Count() >= 30)
                .Select(g => g.Key);
            
           /* var count = File.ReadAllLines(file).Select(l => new ItemRating(l))
                .GroupBy(ir => ir.UserId)
                .Where(g => g.Count() >= 50)
                .Select(g => g.Key)
                .Join(topMusicRators, o => o, i => i, (o, i) => i)
                .Distinct().Count();
            */
            //Console.WriteLine(count);
            Console.WriteLine(topMusicRators.Distinct().Count());
        }
    }
}
