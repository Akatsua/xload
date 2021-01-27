namespace XLoad.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Dto;

    public static class SummaryHelper
    {
        public static void WriteTickSummary(
            int currentData, 
            BlockingCollection<Request> backlog, 
            List<RequestTask> tasks, 
            DateTime currentIterationTime, 
            DateTime nextIterationTime, 
            int taskDelta, 
            DateTime currentTimeForLog)
        {
            Console.WriteLine($@"
[{currentTimeForLog}] Iteration time : {currentIterationTime}
[{currentTimeForLog}] Next iteration : {nextIterationTime}
[{currentTimeForLog}] Backlog count  : {backlog.Count}
[{currentTimeForLog}] Backlog to add : {currentData}
[{currentTimeForLog}] Backlog sum    : {backlog.Count + currentData}
[{currentTimeForLog}] Task count     : {tasks.Count}
[{currentTimeForLog}] Task delta     : {taskDelta}");
        }

        public static void WriteSummary(
            string[] args, 
            Dto.Config config,
            int numberOfTicks, 
            List<int> data)
        {
            Console.WriteLine(@$"
|| Input :
||     Arguments : {string.Join(" ", args)}
|| 
|| Settings :
||     Dryrun         : {config.System.DryRun    }
||     Infinite       : {config.Load.Infinite    }
||     Seed           : {config.Noise.Seed       }
||     Scale          : {config.Noise.Scale      }
||     Resolution     : {config.Noise.Resolution }
||     Total Requests : {config.Load.Requests    }
||     Maximum Tasks  : {config.System.MaxTasks  }
||     Minimum Tasks  : {config.System.MinTasks  }
|| 
|| Computed values :
||     Ticks per time              : {numberOfTicks    }
||     Max requests per resolution : {data.Max()       }
||     Min requests per resolution : {data.Min()       }
||     Avg requests per resolution : {data.Average()   }");

            if (!string.IsNullOrWhiteSpace(config.Diagnostic.Image))
            {
                Console.WriteLine(@$"
|| Image settings :
||     File            : {config.Diagnostic.Image}
||     Horizontal Line : {data.Max() / 20} Messages
||     Vertical Line   : 1 hour");
            }
        }
    }
}
