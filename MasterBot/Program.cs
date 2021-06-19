using System;
using System.Threading.Tasks;

namespace MasterBot
{
    class Program
    {
        public static async Task Main(string[] args) =>
            await Startup.RunAsync(args);
    }
}
