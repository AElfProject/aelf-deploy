﻿using AElfChain.Common.Helpers;
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

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(Run)
            .WithNotParsed(Error);
    }

    private static void Error(IEnumerable<Error> errors)
    {
        Console.WriteLine("error: Failed to parse arguments.");
    }

    private static void Run(Options o)
    {
        var basicService = new Service(o.Endpoint, o.Address, o.Password);
        var service = new DeployAndUpdateService(basicService, Logger);
        service.CheckMinersAndInitAccountBalance();
        var authorInfo = new AuthorInfo
        {
            Author = o.Address,
        };
        if (o.IsUpdate)
        {
            service.UpdateContracts(o.IsApproval, new UpdateInfo(), o.ContractDllPath,
                authorInfo);
        }
        else
        {
            service.DeployContracts(o.IsApproval, o.ContractDllPath, authorInfo);
        }
    }

    private class Options
    {
        [Option('e', "endpoint", Default = "127.0.0.1:8000", HelpText = "Endpoint of local aelf node.")]
        public string Endpoint { get; set; }

        [Option('a', "address", Required = true, HelpText = "BP address of local aelf node.")]
        public string Address { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password of key store file.")]
        public string Password { get; set; }

        [Option('u', "update", Default = false, HelpText = "Is update contract.")]
        public bool IsUpdate { get; set; }

        [Option('c', "contract", Required = true, HelpText = "The path of the contract's DLL.")]
        public string ContractDllPath { get; set; }

        [Option('i', "approval", Default = false, HelpText = "Is approval needed.")]
        public bool IsApproval { get; set; }
    }
}