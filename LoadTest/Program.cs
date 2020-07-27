using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTest
{
    class Program
    {
        static HashSet<string> THREADIDS;
        static async Task Main(string[] args)
        {
            THREADIDS = new HashSet<string>();
            Console.WriteLine("(Index) (ThreadID) (Time)");
         
            var numOfTasks = 100000;

            // var stopwatchOne = new Stopwatch();
            // var dotRunTasks = DoLotsOfTaskDotRunsAsync(stopwatchOne, numOfTasks);

            JoinableTaskContext context = new JoinableTaskContext();

            var stopwatchOne = new Stopwatch();
            var joinableTasksOne = DoLotsOfJoinableTasksAsync(stopwatchOne, numOfTasks, context);

            var stopwatchTwo = new Stopwatch();
            var joinableTasksTwo = DoLotsOfJoinableTasksAsync(stopwatchTwo, numOfTasks, context);

            var stopwatchThree = new Stopwatch();
            var joinableTasksThree = DoLotsOfJoinableTasksAsync(stopwatchThree, numOfTasks, context);

            await Task.WhenAll(joinableTasksOne,joinableTasksTwo,joinableTasksThree);

            ShowAllTreadIds();
            ShowElapsedTime(nameof(joinableTasksOne), stopwatchOne.Elapsed);
            ShowElapsedTime(nameof(joinableTasksTwo), stopwatchTwo.Elapsed);
            ShowElapsedTime(nameof(joinableTasksThree), stopwatchThree.Elapsed);
        }

        static void ShowAllTreadIds()
        {
            Console.WriteLine($"== ThreadIDS ({THREADIDS.Count}) ==");
        }

        static async Task DoLotsOfJoinableTasksAsync(Stopwatch stopwatch, int range = 10, JoinableTaskContext context = null)
        {
            var lotsOfJoinableTasks = Enumerable.Range(0, range).Select(i => {
                if(context == null)
                    context = new JoinableTaskContext();
                var factory = new JoinableTaskFactory(context);
                return factory.RunAsync(() => Task.Run(() => GoSlowAsync(i))).Task;
                }).ToList();

            stopwatch.Start();
            await Task.WhenAll(lotsOfJoinableTasks);
            stopwatch.Stop();
        }

        static async Task DoLotsOfTaskDotRunsAsync(Stopwatch stopwatch, int range = 10)
        {
            var lotsOfTaskDotRuns = Enumerable.Range(0, range).Select(i => Task.Run(() => GoSlowAsync(i))).ToList();
            stopwatch.Start();
            await Task.WhenAll(lotsOfTaskDotRuns);
            stopwatch.Stop();
        }

        static void ShowElapsedTime(string label, TimeSpan ts)
        {
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 2);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{label} RunTime {elapsedTime}");
        }

        static async Task GoSlowAsync(int index)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId.ToString();
            THREADIDS.Add(threadId);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{index}][{threadId}][{DateTime.Now:ss.fff}] == Start");
            await Task.Delay(2000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{index}][{threadId}][{DateTime.Now:ss.fff}] == End");
        }
    }
}
