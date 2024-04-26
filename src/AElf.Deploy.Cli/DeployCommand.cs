using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AElf.Console;
using DeployAndUpdateContract;
using Spectre.Console.Cli;

namespace AElf.Deploy.Cli;

public class DeployCommand : Command<DeployCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Endpoint of aelf node. By default it is 127.0.0.1:8000.")]
        [CommandOption("-e|--endpoint")]
        [DefaultValue("127.0.0.1:8000")]
        public string Endpoint { get; set; }

        [Description("Account address to sign transactions.")]
        [CommandOption("-a|--address")]
        public string Address { get; set; }

        [Description("Password of key store file.")]
        [CommandOption("-p|--password")]
        public string Password { get; set; }

        [Description("Is update contract.")]
        [CommandOption("-u|--update")]
        public bool IsUpdate { get; set; }

        [Description("The path of the contract's DLL.")]
        [CommandOption("-c|--contract")]
        [Required]
        public string ContractDllPath { get; set; }

        [Description("Is approval needed.")]
        [CommandOption("-i|--approval")]
        [DefaultValue(false)]
        public bool IsApproval { get; set; }

        [Description(
            "Salt to calculate contract address. It will be the hex decimal of contract code hash if not provided.")]
        [CommandOption("-s|--salt")]
        [DefaultValue("")]
        public string Salt { get; set; }

        [Description("Is proxy address.")]
        [CommandOption("-o|--isproxyaddress")]
        [DefaultValue(false)]
        public bool IsProxyAddress { get; set; }

        [Description("The signer.")]
        [CommandOption("-n|--signer")]
        public string Signer { get; set; }

        [Description("The contract address to update.")]
        [CommandOption("-t|--updateaddress")]
        public string UpdateAddress { get; set; }

        [Description("Private key.")]
        [CommandOption("-k|--privatekey")]
        public string PrivateKey { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var toPrintAtStart = settings.IsUpdate
            ? $"Start to update contract: {settings.ContractDllPath}"
            : $"Start to deploy contract: {settings.ContractDllPath}";
        ConsoleOutput.StartAlert(toPrintAtStart);

        var service = string.IsNullOrEmpty(settings.PrivateKey)
            ? new Service(settings.Endpoint, settings.Address, settings.Password)
            : new Service(settings.Endpoint, settings.PrivateKey);

        ConsoleOutput.StandardAlert($"Using account: {service.CallAddress}");

        var deployService = new DeployAndUpdateService(service);

        ConsoleOutput.Status("Checking miners and initializing account balance...", ctx =>
        {
            deployService.CheckMinersAndInitAccountBalance();
        });

        var authorInfo = new AuthorInfo
        {
            Author = settings.Address,
            isProxyAddress = settings.IsProxyAddress,
            Signer = settings.Signer
        };
        if (settings.IsUpdate)
        {
            deployService.UpdateContracts(settings.IsApproval,
                new UpdateInfo
                {
                    ContractAddress = settings.UpdateAddress
                },
                settings.ContractDllPath, authorInfo,
                settings.Salt);
        }
        else
        {
            deployService.DeployContracts(settings.IsApproval, settings.ContractDllPath, authorInfo, settings.Salt);
        }

        return 0;
    }
}