using AElf.Standards.ACS5;
using AElfChain.Common.Managers;
using AElfTest.Contract;
using Google.Protobuf.WellKnownTypes;
using Address = AElf.Types.Address;

namespace AElfChain.Common.Contracts;

public enum TestMethod
{
    Initialize,
    TestCreate,
    TestTransfer,
    TransferWithoutParallel,
    GetTestBalance,
    GetTestTokenInfo,
    
    SetMethodCallingThreshold,
    GetMethodCallingThreshold
}

public class AElfTestContract : BaseContract<TestMethod>
{
    public AElfTestContract(INodeManager nodeManager, string callAddress, string contractAddress ) :
        base(nodeManager, contractAddress)
    {
        SetAccount(callAddress);
    }

    public AElfTestContract(INodeManager nodeManager, string callAddress)
        : base(nodeManager, ContractFileName, callAddress)
    {
    }

    public TestTokenInfo GetTestTokenInfo(string symbol)
    {
        return CallViewMethod<TestTokenInfo>(TestMethod.GetTestTokenInfo,
            new GetTestTokenInfoInput { Symbol = symbol });
    }
    
    public TestBalance GetTestBalance(string symbol, string owner)
    {
        return CallViewMethod<TestBalance>(TestMethod.GetTestBalance,
            new GetTestBalanceInput
            {
                Owner = Address.FromBase58(owner),
                Symbol = symbol
            });
    }
    
    public MethodCallingThreshold GetMethodCallingThreshold(string method)
    {
        return CallViewMethod<MethodCallingThreshold>(TestMethod.GetMethodCallingThreshold,
            new StringValue
            {
                Value = method
            });
    }

    public static string ContractFileName => "AElfTest.Contract";
    public static string Salt => "AElfTest.Contract";
}