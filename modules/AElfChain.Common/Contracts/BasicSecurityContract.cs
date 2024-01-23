using AElf.Contracts.TestContract.BasicSecurity;
using AElfChain.Common.Managers;
using Google.Protobuf.WellKnownTypes;
using ProtoBuf.WellKnownTypes;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace AElfChain.Common.Contracts;

public enum SecurityMethod
{
    TestBytesState,
    TestProtobufState,
    TestWhileInfiniteLoop,
    TestForInfiniteLoop,
    
    QueryProtobufState
}
    
public class BasicSecurityContract : BaseContract<SecurityMethod>
{
    public BasicSecurityContract(INodeManager nodeManager, string callAddress, string contractAddress)
        : base(nodeManager, contractAddress)
    {
        SetAccount(callAddress);
    }

    public BasicSecurityContract(INodeManager nodeManager, string callAddress)
        : base(nodeManager, ContractFileName, callAddress)
    {
    }
        
    public static string ContractFileName => "AElf.Contracts.TestContract.BasicSecurity";
    
    public ProtobufOutput QueryProtobufState()
    {
        return CallViewMethod<ProtobufOutput>(SecurityMethod.QueryProtobufState, new Empty());
    }
}