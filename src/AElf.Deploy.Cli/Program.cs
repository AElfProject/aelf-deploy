using AElf.Console;
using AElf.Cryptography;
using AElf.Types;
using AElfChain.Common.Helpers;
using CommandLine;
using DeployAndUpdateContract;
using log4net;
using Spectre.Console;

namespace AElf.Deploy.Cli;

public class Program
{
    private static readonly ILog Logger = Log4NetHelper.GetLogger();

    private static void Main(string[] args)
    {
        Log4NetHelper.LogInit("AElfDeployLog");

        Parser.Default.ParseArguments<DeployOptions>(args)
            .WithParsed(Run)
            .WithNotParsed(Error);
    }

    private static void Error(IEnumerable<Error> errors)
    {
        ConsoleOutput.ErrorAlert("error: Failed to parse arguments.");
    }

    private static void Run(DeployOptions options)
    {
        if (options.IsUpdate)
        {
            ConsoleOutput.StartAlert($"Start to update contract: {options.ContractDllPath}");
        }
        else
        {
            ConsoleOutput.StartAlert($"Start to deploy contract: {options.ContractDllPath}");
        }

        var service = string.IsNullOrEmpty(options.Address)
            ? new Service(options.Endpoint, options.PrivateKey)
            : new Service(options.Endpoint, options.Address, options.Password);

        ConsoleOutput.StandardAlert($"Using account: {service.CallAddress}");

        var deployService = new DeployAndUpdateService(service, Logger);

        AnsiConsole.Progress()
            // .HideCompleted(true)
            .Start(ctx =>
            {
                while(!ctx.IsFinished)
                {
                    deployService.CheckMinersAndInitAccountBalance();
                }
            });
        
        var authorInfo = new AuthorInfo
        {
            Author = options.Address,
            isProxyAddress = options.IsProxyAddress,
            Signer = options.Signer
        };
        if (options.IsUpdate)
        {
            deployService.UpdateContracts(options.IsApproval, new UpdateInfo { ContractAddress = options.UpdateAddress },
                options.ContractDllPath, authorInfo, options.Salt);
        }
        else
        {
            deployService.DeployContracts(options.IsApproval, options.ContractDllPath, authorInfo, options.Salt);
        }
    }
}