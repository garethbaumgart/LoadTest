using Microsoft.VisualStudio.Threading;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("(Index) (ThreadID) (Time)");
         
            var numOfTasks = 1000;

            var stopwatchTwo = new Stopwatch();
            var joinableTasks = DoLotsOfJoinableTasksAsync(stopwatchTwo, numOfTasks);

            var stopwatchOne = new Stopwatch();
            var dotRunTasks = DoLotsOfTaskDotRunsAsync(stopwatchOne, numOfTasks);

            Task.WaitAll(joinableTasks,dotRunTasks);

            ShowElapsedTime(nameof(DoLotsOfTaskDotRunsAsync), stopwatchOne.Elapsed);
            ShowElapsedTime(nameof(DoLotsOfJoinableTasksAsync), stopwatchTwo.Elapsed);
        }

        static async Task DoLotsOfJoinableTasksAsync(Stopwatch stopwatch, int range = 10)
        {
            var factory = new JoinableTaskFactory(new JoinableTaskContext());
            var lotsOfJoinableTasks = Enumerable.Range(0, range).Select(i => factory.RunAsync(() => Task.Run(() => GoSlow(i))).Task).ToList();
            stopwatch.Start();
            await Task.WhenAll(lotsOfJoinableTasks);
            stopwatch.Stop();
        }


        static async Task DoLotsOfTaskDotRunsAsync(Stopwatch stopwatch, int range = 10)
        {
            var lotsOfTaskDotRuns = Enumerable.Range(0, range).Select(i => Task.Run(() => GoSlow(i))).ToList();
            stopwatch.Start();
            await Task.WhenAll(lotsOfTaskDotRuns);
            stopwatch.Stop();
        }

        static void ShowElapsedTime(string label, TimeSpan ts)
        {
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine($"{label} RunTime {elapsedTime}");
        }

        static void GoSlow(int index)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId.ToString();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{index}][{threadId}][{DateTime.Now:ss.fff}] == Start");
            Thread.Sleep(2000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{index}][{threadId}][{DateTime.Now:ss.fff}] == End");
        }
    }
}
