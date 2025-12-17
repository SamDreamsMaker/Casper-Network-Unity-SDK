using System;

namespace CasperSDK.Tests.Mocks
{
    /// <summary>
    /// Test response models for mocking RPC responses.
    /// These models mirror the internal service response structures.
    /// </summary>
    
    #region Block Service Responses
    
    [Serializable]
    public class TestBlockResponse
    {
        public string api_version;
        public TestBlockData block;
    }

    [Serializable]
    public class TestBlockData
    {
        public string hash;
        public TestBlockHeader header;
        public TestBlockBody body;
    }

    [Serializable]
    public class TestBlockHeader
    {
        public long height;
        public int era_id;
        public string state_root_hash;
        public string timestamp;
        public string parent_hash;
    }

    [Serializable]
    public class TestBlockBody
    {
        public string proposer;
        public string[] transfer_hashes;
        public string[] deploy_hashes;
    }

    [Serializable]
    public class TestStateRootHashResponse
    {
        public string api_version;
        public string state_root_hash;
    }

    #endregion

    #region Network Info Responses

    [Serializable]
    public class TestStatusResponse
    {
        public string api_version;
        public string chainspec_name;
        public string starting_state_root_hash;
        public string build_version;
        public string uptime;
        public TestPeerInfo[] peers;
        public TestBlockInfo last_added_block_info;
    }

    [Serializable]
    public class TestPeerInfo
    {
        public string node_id;
        public string address;
    }

    [Serializable]
    public class TestBlockInfo
    {
        public string hash;
        public long height;
        public int era_id;
    }

    [Serializable]
    public class TestPeersResponse
    {
        public string api_version;
        public TestPeerInfo[] peers;
    }

    [Serializable]
    public class TestChainspecResponse
    {
        public string api_version;
        public TestChainspecBytes chainspec_bytes;
    }

    [Serializable]
    public class TestChainspecBytes
    {
        public string chainspec_bytes;
    }

    #endregion

    #region Deploy Service Responses

    [Serializable]
    public class TestDeployResponse
    {
        public string api_version;
        public TestDeployData deploy;
        public TestExecutionResultWrapper[] execution_results;
    }

    [Serializable]
    public class TestDeployData
    {
        public string hash;
        public TestDeployHeader header;
    }

    [Serializable]
    public class TestDeployHeader
    {
        public string account;
        public string timestamp;
        public string chain_name;
    }

    [Serializable]
    public class TestExecutionResultWrapper
    {
        public string block_hash;
        public TestExecutionResult result;
    }

    [Serializable]
    public class TestExecutionResult
    {
        public TestSuccessResult Success;
        public TestFailureResult Failure;
    }

    [Serializable]
    public class TestSuccessResult
    {
        public string cost;
    }

    [Serializable]
    public class TestFailureResult
    {
        public string error_message;
        public string cost;
    }

    [Serializable]
    public class TestDeploySubmitResponse
    {
        public string api_version;
        public string deploy_hash;
    }

    #endregion

    #region Validator Service Responses

    [Serializable]
    public class TestAuctionInfoResponse
    {
        public string api_version;
        public TestAuctionState auction_state;
    }

    [Serializable]
    public class TestAuctionState
    {
        public string state_root_hash;
        public long block_height;
        public TestEraValidator[] era_validators;
        public TestBidInfo[] bids;
    }

    [Serializable]
    public class TestEraValidator
    {
        public int era_id;
        public TestValidatorWeight[] validator_weights;
    }

    [Serializable]
    public class TestValidatorWeight
    {
        public string public_key;
        public string weight;
    }

    [Serializable]
    public class TestBidInfo
    {
        public string public_key;
        public TestBidData bid;
    }

    [Serializable]
    public class TestBidData
    {
        public string bonding_purse;
        public string staked_amount;
        public int delegation_rate;
        public bool inactive;
    }

    #endregion

    #region State Service Responses

    [Serializable]
    public class TestQueryGlobalStateResponse
    {
        public string api_version;
        public object stored_value;
        public string merkle_proof;
    }

    [Serializable]
    public class TestDictionaryItemResponse
    {
        public string api_version;
        public string dictionary_key;
        public object stored_value;
        public string merkle_proof;
    }

    #endregion

    #region Account Service Responses

    [Serializable]
    public class TestAccountInfoResponse
    {
        public TestAccountData account;
    }

    [Serializable]
    public class TestAccountData
    {
        public string account_hash;
        public string main_purse;
        public object[] named_keys;
    }

    [Serializable]
    public class TestStatusInfoResponse
    {
        public TestBlockInfo last_added_block_info;
    }

    [Serializable]
    public class TestBalanceResponse
    {
        public string balance_value;
    }

    #endregion
}
