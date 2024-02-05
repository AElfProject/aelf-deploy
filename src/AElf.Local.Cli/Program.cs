using AElfChain.Common.Helpers;
using CommandLine;
using DeployAndUpdateContract;
using log4net;

namespace AElf.Local.Cli;

public class Program
{
    private static readonly ILog Logger = Log4NetHelper.GetLogger();

    private static void Main(string[] args)
    {
        Log4NetHelper.LogInit("AElfLocalNodeCli");

        Parser.Default.ParseArguments<DeployOptions>(args)
            .WithParsed(Run)
            .WithNotParsed(Error);
    }

    private static void Error(IEnumerable<Error> errors)
    {
        Console.WriteLine("error: Failed to parse arguments.");
    }

    private static void Run(DeployOptions options)
    {
        var basicService = new Service(options.Endpoint, options.PrivateKey);
        var service = new DeployAndUpdateService(basicService, Logger);
        service.CheckMinersAndInitAccountBalance();
        var authorInfo = new AuthorInfo
        {
            Author = options.Address,
            isProxyAddress = options.IsProxyAddress,
            Signer = options.Signer
        };
        if (options.IsUpdate)
        {
            service.UpdateContracts(options.IsApproval, new UpdateInfo { ContractAddress = options.UpdateAddress },
                options.ContractDllPath, authorInfo, options.Salt);
        }
        else
        {
            service.DeployContracts(options.IsApproval, options.ContractDllPath, authorInfo, options.Salt);
        }
    }
}