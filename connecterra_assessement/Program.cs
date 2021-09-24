using connecterra_assessement.Models;
using connecterra_assessement.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace connecterra_assessement
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = string.Empty;
            string mergeDistanceString = string.Empty;

            List<FileInput> input = null;
            MergeLogic logic = new MergeLogic();

            while (string.IsNullOrEmpty(filePath) || input is null || !input.Any())
            {
                // Sample files are provided in resources folder
                Console.WriteLine("Enter the file path with input");
                filePath = Console.ReadLine();
                input = logic.GetIntervalsFromFile(filePath);
                if (input == null || !input.Any())
                {
                    Console.WriteLine("No file in filepath");
                }
            }

            while (string.IsNullOrWhiteSpace(mergeDistanceString))
            {
                Console.WriteLine("Please enter the merge distance");
                mergeDistanceString = Console.ReadLine();
                if (!mergeDistanceString.All(a => char.IsDigit(a))) mergeDistanceString = string.Empty;
            }

            var mergeDistance = Convert.ToInt32(mergeDistanceString);
            logic.MergeIntervals(input, mergeDistance);
        }
    }

}
