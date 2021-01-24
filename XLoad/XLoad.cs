/* XLoad
 * 
 *   Overview
 *     This code will run a specified code X amount of times using simplex noise generation
 *     to somewhat generate a natural load on the code.
 *     At this point in time, for this to work, insert your code into << INSERT CODE HERE >>
 *     
 *   Future work
 *     - Run .dll code instead of replacing code
 *     - Log to file instead of console
 *     - Better sample image generation
 *     
 *   Arguments:
 *     General
 *       -dryrun
 *         | Do not automatically start the load
 *     Noise Generation
 *       -scale <float>
 *         | Scale for the Simplex Noise generation
 *         | Default : 0.001
 *       -seed <int> 
 *         | Seed for the Simplex Noise generation
 *         | Default : 1337
 *       -resolution <int>
 *         | Frequency (in seconds) for which a new point is generated 
 *         | Default: 60 (one minute)       
 *     Load Generation
 *       -time <int>
 *         | Amount of seconds for the system to run
 *         | Default : 86400 (one day)
 *       -infite
 *         | If this argument is used, the system will continue to operate after the -time
 *       -requests <int>
 *         | Number of requests to be performed in -time
 *         | Default : 1000000
 *       -maxtasks <int>
 *         | Maximum amount of TPL tasks the system will generate
 *       -mintasks <int>
 *         | Minimum amount of TPL tasks the system will generate
 *     Image Generation
 *       -image <file path>
 *         | Path for the creation of a bmp file with the generated graph
 */

namespace XLoad
{
    using Dto;
    using Helpers;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    partial class XLoad
    {
        static void Main(string[] args)
        {
            var config = new Config();

            // Argument Parsing
            {
                args.GetFloatArg( "-scale"      , out config.Scale      , 0.01f         );
                args.GetIntArg  ( "-seed"       , out config.Seed       , 1337          );
                args.GetIntArg  ( "-resolution" , out config.Resolution , 10            );
                args.GetIntArg  ( "-time"       , out config.Time       , 24 * 60 * 60  );
                args.GetIntArg  ( "-requests"   , out config.Requests   , 1000000       );
                args.GetIntArg  ( "-maxTasks"   , out config.MaxTasks   , int.MaxValue  );
                args.GetIntArg  ( "-minTasks"   , out config.MinTasks   , 1             );
                args.GetStrArg  ( "-image"      , out config.Image      , null          );
                
                config.Infinite = args.GetArgExists("-infinite" );
                config.DryRun   = args.GetArgExists("-dryrun"   );

                if (config.Resolution < 1) config.Resolution = 1;
                if (config.Time       < 1) config.Time       = 24 * 60 * 60;
                if (config.MaxTasks   < 1) config.MaxTasks   = int.MaxValue;
                if (config.MinTasks   < 1) config.MinTasks   = 1;

                if (config.Resolution > config.Time)
                {
                    Console.WriteLine("[ERROR] -resolution needs to be smaller than -time");
                }

                config.NumTicks = config.Time / config.Resolution;
            }

            var state = new State()
            {
                Noise        = new Noise(config.Seed, config.Scale),
                Shutdown     = false,
                CancelSource = new CancellationTokenSource()
            };
            
            // Diagnostic Data
            {
                var sample = GenerateTimedData(config.Requests, config.NumTicks, state.Noise);

                Image.WriteImage(config, sample);
                Summary.WriteSummary(args, config, config.NumTicks, sample);

                state.Noise.Reset();

                if (config.DryRun)
                {
                    return;
                }
            }

            // Setup graceful shutdown | Handles SIGINT https://github.com/aspnet/Hosting/issues/870#issuecomment-257435212
            {
                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                {
                    Console.WriteLine();

                    if (!state.Shutdown)
                    { 
                        Console.WriteLine($"[{DateTime.Now}] Attempting graceful shutdown");

                        e.Cancel = true;
                        state.Shutdown = true;
                        state.CancelSource.Cancel();
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now}] Forcing shutdown");
                    }
                };
            }

            Console.WriteLine($"{Environment.NewLine}[{DateTime.Now}] Starting up");

            // Start running
            {
                Task.Factory
                    .StartNew(
                        () => LoadLogicAsync(config, state).Wait(), 
                        TaskCreationOptions.LongRunning)
                    .Wait();
            }

            Console.WriteLine($"[{DateTime.Now}] Shutdown complete");
        }

        private static async Task LoadLogicAsync(Config config, State state)
        {
            var backlog = new BlockingCollection<Request>();
            var tasks   = GenerateTasks(backlog, config.MinTasks).ToList();

            while (!state.Shutdown)
            {
                var data      = GenerateTimedData(config.Requests, config.NumTicks, state.Noise);
                var startTime = DateTime.Now;

                for (int tickIndex = 0; tickIndex < data.Count; tickIndex++)
                {
                    var timeBetweenRequests  = 1 / ((float)data[tickIndex] / config.Resolution);

                    var currentIterationTime = startTime.AddSeconds( tickIndex      * config.Resolution);
                    var nextIterationTime    = startTime.AddSeconds((tickIndex + 1) * config.Resolution);

                    int taskDelta = ManageTasks(data[tickIndex], config.MinTasks, config.MaxTasks, backlog, tasks);

                    Summary.WriteTickSummary(
                        data[tickIndex], backlog, tasks, currentIterationTime,
                        nextIterationTime, taskDelta, DateTime.Now);

                    for (int i = 0; i < data[tickIndex]; i++)
                    {
                        backlog.Add(new Request(currentIterationTime.AddSeconds(timeBetweenRequests * i)));
                    }

                    // Task.Delay will wait for X amount of milliseconds but its constraint by the systems clock resolution, 
                    // which according to the MSDN documentation is about 15 milliseconds. Having the call be 15ms late is a 
                    // good tradeoff since we don't need to actively way and we have cancellationtoken capabilities.
                    int delay = (int)(nextIterationTime - DateTime.Now).TotalMilliseconds;

                    if (delay > 0)
                    {
                        try
                        {
                            await Task.Delay(delay, state.CancelSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }

                    if (state.Shutdown)
                    {
                        break;
                    }
                }

                if (!config.Infinite)
                {
                    break;
                }
            }

            // Cleanup and wait for cancellation
            {
                backlog.CompleteAdding();

                foreach (var task in tasks)
                {
                    task.Token.Cancel();
                }

                Task.WaitAll(tasks.Select(t => t.Task).ToArray());
            }
        }

        private static int ManageTasks(
            int currentTickValue, 
            int minTasks, 
            int maxTasks, 
            BlockingCollection<Request> backlog, 
            List<RequestTask> tasks)
        {
            if (minTasks == maxTasks)
            {
                return 0;
            }

            var taskDelta = 0;

            if (backlog.Count > 0 && tasks.Count < maxTasks)
            {
                taskDelta = (backlog.Count / currentTickValue) + 1;

                tasks.AddRange(GenerateTasks(backlog, taskDelta));
            }
            else if (backlog.Count == 0 && tasks.Count > minTasks)
            {
                taskDelta = -1;

                tasks.First().Token.Cancel();
                tasks.RemoveAt(0);
            }

            return taskDelta;
        }

        private static List<int> GenerateTimedData(
            int requests, 
            int numTicks, 
            Noise noise)
        {
            List<int> data = noise
                .CalculateNextValues(numTicks)
                .Select(value => (int)value)
                .ToList();

            var rawSum = data.Sum();
            var ratio = requests / (float)rawSum;

            int processedSum = 0;

            for (int i = 0; i < data.Count(); i++)
            {
                data[i] = (int)(data[i] * ratio);
                processedSum += data[i];
            }

            var requestDelta = requests - processedSum;

            var div = requestDelta / data.Count;
            var mod = requestDelta % data.Count;

            for (int i = 0; i < data.Count; i++, mod--)
            {
                data[i] += div + (mod > 0 ? 1 : 0);

                if (div == 0 && mod == 0)
                {
                    break;
                }
            }

            return data;
        }

        private static IEnumerable<RequestTask> GenerateTasks(
            BlockingCollection<Request> backlog, 
            int tasksToAdd)
        {
            var tasks = new List<RequestTask>(tasksToAdd);

            for (int i = 0; i < tasksToAdd; i++)
            {
                var token = new CancellationTokenSource();
                var task = Task.Run(async () =>
                {
                    await RunLoadLogicAsync(backlog, token.Token);
                });

                tasks.Add(new RequestTask(task, token));
            }

            return tasks;
        }

        private static async Task RunLoadLogicAsync(
            BlockingCollection<Request> requests, 
            CancellationToken token)
        {
            try
            {
                foreach (var request in requests.GetConsumingEnumerable(token))
                {
                    int delay = (int)(request.RunAt - DateTime.Now).TotalMilliseconds;

                    if (delay > 0)
                    {
                        await Task.Delay(delay, token);
                    }

                    try
                    {
                        // << INSERT CODE HERE >>
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now}][Error] Something unexpected happened while processing a request ({ex}). Continuing to process...");
                    }
                }
            }
            catch (OperationCanceledException) { }  
        }
    }
}
