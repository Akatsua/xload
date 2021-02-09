namespace XLoad
{
    using Dto;
    using External;
    using Helpers;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Plugin;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    partial class XLoad
    {
        static void Main(string[] args)
        {
            string configFile = "config.json";

            // Initial config file environment / argument check
            {
                configFile = ConfigHelper.GetEnvironmentStr("XLOAD_CONFIG", configFile);
                configFile = ConfigHelper.GetStrFromArgs(args, "-config", configFile);

                if (!File.Exists(configFile))
                {
                    Console.WriteLine("[ERROR] Config file not found! Aborting...");
                    return;
                }
            }

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .Build();

            var config = configuration
                .GetSection("XLoad")
                .Get<Config>();

            // Improve this for plugins (issues with references on IConfiguration)
            var jsonconfig = JsonConvert.DeserializeObject<JToken>(File.ReadAllText(configFile));

            // Environment variables argument parsing
            {
                config.Noise.Scale      = ConfigHelper.GetEnvironmentFloat ( "XLOAD_SCALE"      , config.Noise.Scale      );
                config.Noise.Seed       = ConfigHelper.GetEnvironmentInt   ( "XLOAD_SEED"       , config.Noise.Seed       );
                config.Noise.Resolution = ConfigHelper.GetEnvironmentInt   ( "XLOAD_RESOLUTION" , config.Noise.Resolution );
                config.Load.Time        = ConfigHelper.GetEnvironmentInt   ( "XLOAD_TIME"       , config.Load.Time        );
                config.Load.Requests    = ConfigHelper.GetEnvironmentInt   ( "XLOAD_REQUESTS"   , config.Load.Requests    );
                config.Load.Infinite    = ConfigHelper.GetEnvironmentBool  ( "XLOAD_INFINITE"   , config.Load.Infinite    );
                config.System.MaxTasks  = ConfigHelper.GetEnvironmentInt   ( "XLOAD_MAXTASKS"   , config.System.MaxTasks  );
                config.System.MinTasks  = ConfigHelper.GetEnvironmentInt   ( "XLOAD_MINTASKS"   , config.System.MinTasks  );
                config.System.DryRun    = ConfigHelper.GetEnvironmentBool  ( "XLOAD_DRYRUN"     , config.System.DryRun    );
                config.System.NoStats   = ConfigHelper.GetEnvironmentBool  ( "XLOAD_NOSTATS"    , config.System.NoStats   );
                config.Diagnostic.Image = ConfigHelper.GetEnvironmentStr   ( "XLOAD_IMAGE"      , config.Diagnostic.Image );
            }

            // Command line argument parsing
            {
                config.Noise.Scale      = ConfigHelper.GetFloatFromArgs ( args, "-scale"      , config.Noise.Scale      );
                config.Noise.Seed       = ConfigHelper.GetIntFromArgs   ( args, "-seed"       , config.Noise.Seed       );
                config.Noise.Resolution = ConfigHelper.GetIntFromArgs   ( args, "-resolution" , config.Noise.Resolution );
                config.Load.Time        = ConfigHelper.GetIntFromArgs   ( args, "-time"       , config.Load.Time        );
                config.Load.Requests    = ConfigHelper.GetIntFromArgs   ( args, "-requests"   , config.Load.Requests    );
                config.Load.Infinite    = ConfigHelper.GetBoolFromArgs  ( args, "-infinite"   , config.Load.Infinite    );
                config.System.MaxTasks  = ConfigHelper.GetIntFromArgs   ( args, "-maxTasks"   , config.System.MaxTasks  );
                config.System.MinTasks  = ConfigHelper.GetIntFromArgs   ( args, "-minTasks"   , config.System.MinTasks  );
                config.System.DryRun    = ConfigHelper.GetBoolFromArgs  ( args, "-dryrun"     , config.System.DryRun    );
                config.System.NoStats   = ConfigHelper.GetBoolFromArgs  ( args, "-nostats"    , config.System.NoStats   );
                config.Diagnostic.Image = ConfigHelper.GetStrFromArgs   ( args, "-image"      , config.Diagnostic.Image );
            }

            // Validate configs & apply defaults
            {
                if (config.Noise.Scale      == null) { Console.WriteLine("[ERROR] Scale configuration not found! Aborting..."       ); return; }
                if (config.Noise.Seed       == null) { Console.WriteLine("[ERROR] Seed configuration not found! Aborting..."        ); return; }
                if (config.Noise.Resolution == null) { Console.WriteLine("[ERROR] Resolution configuration not found! Aborting..."  ); return; }
                if (config.Load.Time        == null) { Console.WriteLine("[ERROR] Time configuration not found! Aborting..."        ); return; }
                if (config.Load.Requests    == null) { Console.WriteLine("[ERROR] Requests configuration not found! Aborting..."    ); return; }

                if (config.System.MaxTasks  == null) config.System.MaxTasks = int.MaxValue;
                if (config.System.MaxTasks  < 1    ) config.System.MaxTasks = int.MaxValue;
                if (config.System.MinTasks  == null) config.System.MinTasks = 1;
                if (config.System.MinTasks  < 1    ) config.System.MinTasks = 1;

                if (config.System.DryRun    == null) config.System.DryRun  = false;
                if (config.System.NoStats   == null) config.System.NoStats = false;
                if (config.Load.Infinite    == null) config.Load.Infinite  = false;

                if (config.Noise.Resolution > config.Load.Time)
                {
                    Console.WriteLine("[ERROR] Resolution needs to be smaller than Time! Aborting...");
                    return;
                }

                if (config.System.MaxTasks < config.System.MinTasks)
                {
                    Console.WriteLine("[ERROR] MinTasks must be smaller than MaxTasks! Aborting...");
                    return;
                }
            }

            var state = new State()
            {
                Noise = new Noise(config.Noise.Seed.Value, config.Noise.Scale.Value),
                NumTicks = config.Load.Time.Value / config.Noise.Resolution.Value,
                Shutdown = false,
                CancelSource = new CancellationTokenSource(),
                Plugins = new List<IPlugin>()
            };

            // Check plugins
            {
                foreach (var item in config.System.Plugins)
                {
                    var pluginFile = Directory
                        .EnumerateFiles(Directory.GetCurrentDirectory(), item.Name + ".dll", SearchOption.AllDirectories)
                        .FirstOrDefault();

                    var configJson = jsonconfig
                        .Children()
                        .Select(c => (JProperty)c)
                        .FirstOrDefault(c => c.Name == item.Config)
                        .Value;

                    var pluginConfig   = configuration.GetSection(item.Config);
                    var pluginAssembly = PluginLoader.LoadPlugin(pluginFile);
                    var pluginObject   = PluginLoader.GetPluginFromAssembly(pluginAssembly);

                    pluginObject.Initialize(JsonConvert.SerializeObject(configJson));

                    state.Plugins.Add(pluginObject);
                }
            }

            // Diagnostic Data
            {
                var sample = GenerateTimedData(config.Load.Requests.Value, state.NumTicks, state.Noise);

                ImageHelper.WriteImage(config, sample);
                SummaryHelper.WriteSummary(args, config, state.NumTicks, sample);

                state.Noise.Reset();

                if (config.System.DryRun.Value)
                {
                    return;
                }
            }

            // Setup graceful shutdown https://github.com/aspnet/Hosting/issues/870#issuecomment-257435212
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
            var tasks   = GenerateTasks(backlog, config.System.MinTasks.Value).ToList();

            while (!state.Shutdown)
            {
                var data      = GenerateTimedData(config.Load.Requests.Value, state.NumTicks, state.Noise);
                var startTime = DateTime.Now;

                for (int tickIndex = 0; tickIndex < data.Count; tickIndex++)
                {
                    var timeBetweenRequests  = 1 / ((float)data[tickIndex] / config.Noise.Resolution.Value);

                    var currentIterationTime = startTime.AddSeconds( tickIndex      * config.Noise.Resolution.Value);
                    var nextIterationTime    = startTime.AddSeconds((tickIndex + 1) * config.Noise.Resolution.Value);

                    int taskDelta = ManageTasks(data[tickIndex], config.System.MinTasks.Value, config.System.MaxTasks.Value, backlog, tasks);

                    SummaryHelper.WriteTickSummary(
                        data[tickIndex], backlog, tasks, currentIterationTime,
                        nextIterationTime, taskDelta, DateTime.Now, config.System.NoStats.Value);

                    for (int i = 0; i < data[tickIndex]; i++)
                    {
                        backlog.Add(new Request(currentIterationTime.AddSeconds(timeBetweenRequests * i), state.Plugins));
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

                if (!config.Load.Infinite.Value)
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
                        Task.WaitAll(
                            request.Plugins
                                .Select(p => p.ExecuteAsync())
                                .ToArray());
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
