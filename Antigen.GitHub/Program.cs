using CommandLine;
using Antigen.GitHub.Args;

namespace Antigen.GitHub;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            return await Parser.Default.ParseArguments(args, typeof(CreateDiscussionsCommand))
                .MapResult(
                    (CreateDiscussionsCommand cmd) => RunDiscussionsCreator.Run(cmd),
                    async _ =>
                    {
                        return -1;
                    }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return -1;
        }
    }
}
