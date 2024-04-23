using CommandLine;

namespace AElf.Deploy.Cli;

internal class DeployOptions
{
    [Option('e', "endpoint", Default = "127.0.0.1:8000", HelpText = "Endpoint of local aelf node.")]
    public string Endpoint { get; set; }

    [Option('a', "address", HelpText = "Account address to sign transactions.")]
    public string Address { get; set; }

    [Option('p', "password", HelpText = "Password of key store file.")]
    public string Password { get; set; }

    [Option('u', "update", Default = false, HelpText = "Is update contract.")]
    public bool IsUpdate { get; set; }

    [Option('c', "contract", Required = true, HelpText = "The path of the contract's DLL.")]
    public string ContractDllPath { get; set; }

    [Option('i', "approval", Default = false, HelpText = "Is approval needed.")]
    public bool IsApproval { get; set; }

    [Option('s', "salt", Default = "", HelpText =
        "Salt to calculate contract address. It will be the hex decimal of contract code hash if not provided.")]
    public string Salt { get; set; }

    [Option('o', "isproxyaddress", Default = false, HelpText = "Is proxy address.")]
    public bool IsProxyAddress { get; set; }

    [Option('n', "signer", HelpText = "The signer.")]
    public string Signer { get; set; }

    [Option('t', "updateaddress", HelpText = "The contract address to update.")]
    public string UpdateAddress { get; set; }

    [Option('k', "privatekey", HelpText = "Private key")]
    public string PrivateKey { get; set; }
}