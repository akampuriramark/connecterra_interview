using connecterra_assessement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace connecterra_assessement.Logic
{
    public class MergeLogic
    {
        private List<int[]> result = new List<int[]>();
        public List<FileInput> GetIntervalsFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath).Select(line => line.Split(','))
                    .Where(x => x.Length == 4) // only pick valid streams
                    .Select(x => new FileInput()
                    {
                        ArrivalTime = Convert.ToInt32(x[0]),
                        Start = Convert.ToInt32(x[1]),
                        End = Convert.ToInt32(x[2]),
                        Action = x[3].Trim()
                    }).ToList();
            }
            return null;
        }

        public void MergeIntervals(List<FileInput> intervals, int mergeDistance)
        {

            if (intervals.Count == 1)
            {
                result.Add(new int[] { intervals[0].Start, intervals[0].End });
                PrintResult(result);
            }
            else
            {
                // Assumption: Since it's a stream, results arent sorted, coming in one by one processed as they come in
                for (int i = 0; i < intervals.Count; i++)
                {

                    if (intervals[i].Action.Equals("ADDED", StringComparison.CurrentCultureIgnoreCase))
                    {
                        AddInterval(intervals[i], mergeDistance);
                    }
                    else if (intervals[i].Action.Equals("DELETED", StringComparison.CurrentCultureIgnoreCase))
                    {
                        DeleteInterval(result, new int[] { intervals[i].Start, intervals[i].End });
                    }
                    else if (intervals[i].Action.Equals("REMOVED", StringComparison.CurrentCultureIgnoreCase))
                    {
                        RemoveInterval(intervals, intervals[i], mergeDistance);
                    }
                    else
                    {
                        // Do nothing
                        continue;
                    }

                    PrintResult(result);
                }
            }
        }

        private void AddInterval( FileInput intervalToAdd, int mergeDistance)
        {

            // if list is empty or there's no overlap, append
            if (result.Count == 0)
            {
                result.Add(new int[] { intervalToAdd.Start, intervalToAdd.End });
            }
            else
            {
                // Check that start is overlapping but end is not over lapping the first element of list
                var startOverlappingIndex = result.FindIndex(a => IsOverlapping(intervalToAdd.Start, a[0], a[1]));
                if (startOverlappingIndex != -1)
                {
                    if (!result.Any(a => IsOverlapping(intervalToAdd.End, a[0], a[1])))
                    {
                        result[startOverlappingIndex][1] = intervalToAdd.End;
                    }
                }
                else
                {
                    // find index in result where overlap happens in whole list
                    var startIsInMergeRange = result.Any(a => IsOverlapping(intervalToAdd.Start, a[0] - mergeDistance, a[0] + mergeDistance) || IsOverlapping(intervalToAdd.Start, a[1] - mergeDistance, a[1] + mergeDistance));

                    // if not overlapping at start, then find index overlap happens on end (if it exists anyways)
                    var index = startIsInMergeRange ? result.FindIndex(a => IsOverlapping(intervalToAdd.Start, a[0] - mergeDistance, a[0] + mergeDistance) || IsOverlapping(intervalToAdd.Start, a[1] - mergeDistance, a[1] + mergeDistance)) :
                                                      result.FindIndex(a => IsOverlapping(intervalToAdd.End, a[0] - mergeDistance, a[0] + mergeDistance) || IsOverlapping(intervalToAdd.End, a[1] - mergeDistance, a[1] + mergeDistance));

                    if (index != -1)
                    {
                        // take the least start
                        result[index][0] = Math.Min(result[index][0], intervalToAdd.Start);

                        // take the max end
                        result[index][1] = Math.Max(result[index][1], intervalToAdd.End);
                    }
                    else
                    {
                        result.Add(new int[] { intervalToAdd.Start, intervalToAdd.End });
                    }
                }
                result = result.OrderBy(x => x[0]).ToList();
            }
        }
        private void RemoveInterval(List<FileInput> input, FileInput intervalToRemove, int mergeDistance)
        {
            result = new List<int[]>();
            // remove interval from original list
            input = input.Where(x => x.Start != intervalToRemove.Start && x.End != intervalToRemove.End).ToList();
            // add to result again
            foreach (var interval in input)
            {
                AddInterval( interval, mergeDistance);
            }
        }
        private static bool IsOverlapping(int number, int start, int end)
        {
            return number >= start && number <= end;
        }
        private void DeleteInterval(List<int[]> intervals, int[] toBeRemoved)
        {
            result = new List<int[]>();
            for (int i = 0; i < intervals.Count; i++)
            {
                // If there are no overlaps, just add the interval to the list
                if (intervals[i][0] > toBeRemoved[1] || intervals[i][1] < toBeRemoved[0])
                {
                    result.Add(new int[] { intervals[i][0], intervals[i][1] });
                }
                else
                {
                    // If there's a left interval we need to keep
                    if (intervals[i][0] < toBeRemoved[0])
                    {
                        result.Add(new int[] { intervals[i][0], toBeRemoved[0] });
                    }
                    // If there's a right interval we need to keep
                    if (intervals[i][1] > toBeRemoved[1])
                    {
                        result.Add(new int[] { toBeRemoved[1], intervals[i][1] });
                    }
                }
            }
        }

        private static void PrintResult(List<int[]> result)
        {
            var outputString = string.Empty;
            foreach (var interval in result)
            {
                outputString += string.Format("[{0},{1}]", interval[0], interval[1]);
            }
            Console.WriteLine(outputString);
        }
    }
}
