using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using AElf.Standards.ACS3;
using AElf.Contracts.Parliament;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfChain.Common.DtoExtension;
using AElfChain.Common.Helpers;
using AElfChain.Common.Managers;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Spectre.Console;
using Volo.Abp.Threading;

namespace AElfChain.Common.Contracts
{
    public enum ParliamentMethod
    {
        //Action
        Initialize,
        Approve,
        Reject,
        Abstain,
        CreateProposal,
        GetProposal,
        Release,
        CreateOrganization,
        ChangeOrganizationThreshold,
        ChangeOrganizationProposerWhiteList,
        ClearProposal,
        ApproveMultiProposals,
        
        //fee
        ChangeMethodFeeController,
        SetMethodFee,
        GetMethodFee,
        GetMethodFeeController,

        //View
        GetDefaultOrganizationAddress,
        GetOrganization,
        ValidateOrganizationExist,
        CalculateOrganizationAddress,
        ValidateAddressIsParliamentMember,
        GetProposerWhiteList,
        GetEmergencyResponseOrganizationAddress,
        GetReleaseThresholdReachedProposals,
        GetAvailableProposals
    }

    public class ParliamentContract : BaseContract<ParliamentMethod>
    {
        public ParliamentContract(INodeManager nm, string callAddress, string contractAddress) :
            base(nm, contractAddress)
        {
            Logger = Log4NetHelper.GetLogger();
            SetAccount(callAddress);
        }

        public Hash CreateProposal(string contractAddress, string method, IMessage input, Address organizationAddress,
            string caller = null)
        {
            var tester = GetTestStub<ParliamentContractImplContainer.ParliamentContractImplStub>(caller);
            var expiredTime = KernelHelper.GetUtcNow().AddDays(3);
            var createProposalInput = new CreateProposalInput
            {
                ContractMethodName = method,
                ToAddress = contractAddress.ConvertAddress(),
                Params = input.ToByteString(),
                ExpiredTime = expiredTime,
                OrganizationAddress = organizationAddress
            };
            var proposal = AsyncHelper.RunSync(() => tester.CreateProposal.SendAsync(createProposalInput));
            proposal.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined,
                proposal.TransactionResult.TransactionId.ToHex);
            var proposalId = proposal.Output;
            Logger.Info($"Proposal {proposalId} created success by {caller ?? CallAddress}.");

            return proposalId;
        }

        public void ApproveProposal(Hash proposalId, string caller = null)
        {
            var tester = GetTestStub<ParliamentContractImplContainer.ParliamentContractImplStub>(caller);
            var transactionResult = AsyncHelper.RunSync(() => tester.Approve.SendAsync(proposalId));
            transactionResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            Logger.Info($"Proposal {proposalId} approved success by {caller ?? CallAddress}");
        }

        public string Approve(Hash proposalId, string caller)
        {
            SetAccount(caller);
            return ExecuteMethodWithTxId(ParliamentMethod.Approve, proposalId);
        }

        public string Abstain(Hash proposalId, string caller)
        {
            SetAccount(caller);
            return ExecuteMethodWithTxId(ParliamentMethod.Abstain, proposalId);
        }

        public string Reject(Hash proposalId, string caller)
        {
            SetAccount(caller);
            return ExecuteMethodWithTxId(ParliamentMethod.Reject, proposalId);
        }

        public void MinersApproveProposal(Hash proposalId, IEnumerable<string> callers)
        {
            var approveTxIds = new List<string>();
            foreach (var user in callers)
            {
                var tester = GetNewTester(user);
                try
                {
                    var txId = tester.ExecuteMethodWithResult(ParliamentMethod.Approve, proposalId);
                    txId.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteLine(e.Message);
                }
            }

            Thread.Sleep(10000);
        }

        public TransactionResult ReleaseProposal(Hash proposalId, string caller = null)
        {
            var tester = GetTestStub<ParliamentContractImplContainer.ParliamentContractImplStub>(caller);
            var result = AsyncHelper.RunSync(() => tester.Release.SendAsync(proposalId));
            // result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            // Logger.Info($"Proposal {proposalId} release success by {caller ?? CallAddress}");

            return result.TransactionResult;
        }

        public Address GetGenesisOwnerAddress()
        {
            return CallViewMethod<Address>(ParliamentMethod.GetDefaultOrganizationAddress, new Empty());
        }

        public Organization GetOrganization(Address organization)
        {
            return CallViewMethod<Organization>(ParliamentMethod.GetOrganization, organization);
        }
        
        public Address GetEmergencyResponseOrganizationAddress()
        {
            return CallViewMethod<Address>(ParliamentMethod.GetEmergencyResponseOrganizationAddress, new Empty());
        }
        
        public ProposerWhiteList GetProposerWhiteList()
        {
            return CallViewMethod<ProposerWhiteList>(ParliamentMethod.GetProposerWhiteList,new Empty());
        }

        public ProposalOutput CheckProposal(Hash proposalId)
        {
            return CallViewMethod<ProposalOutput>(ParliamentMethod.GetProposal,
                proposalId);
        }

        public ProposalIdList GetReleaseThresholdReachedProposals(ProposalIdList proposalIdList)
        {
            return CallViewMethod<ProposalIdList>(ParliamentMethod.GetReleaseThresholdReachedProposals, proposalIdList);
        }

        public ProposalIdList GetAvailableProposals(ProposalIdList proposalIdList)
        {
            return CallViewMethod<ProposalIdList>(ParliamentMethod.GetAvailableProposals, proposalIdList);
        }
        
        public bool CheckDeployOrUpdateProposal(Hash proposalId, List<string> miners)
        {
            var checkTimes = (miners.Count.Add(1)).Mul(8);
            var proposalInfo = CheckProposal(proposalId);
            Logger.Info("======== Check Proposal Info ========");
            var stopwatch = Stopwatch.StartNew();
            while (!proposalInfo.ToBeReleased && proposalInfo.ExpiredTime != null && checkTimes > 0)
            {
                Thread.Sleep(1000);
                proposalInfo = CheckProposal(proposalId);
                AnsiConsole.WriteLine(
                    $"\r[Processing]: ProposalId={proposalId.ToHex()}, " +
                    $"ToBeReleased: {proposalInfo.ToBeReleased}, " +
                    $"using time:{CommonHelper.ConvertMileSeconds(stopwatch.ElapsedMilliseconds)}");
                checkTimes--;
            }
            Thread.Sleep(10000);
            stopwatch.Stop();
            return proposalInfo.ToBeReleased || proposalInfo.ExpiredTime == null;
        }
    }
}