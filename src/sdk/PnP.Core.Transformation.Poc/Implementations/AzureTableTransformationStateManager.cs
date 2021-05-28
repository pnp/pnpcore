using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.Poc.Implementations
{
    public class AzureTableTransformationStateManager : ITransformationStateManager
    {
        private readonly CloudTable tableReference;

        private static readonly SemaphoreSlim CreationSemaphoreSlim = new SemaphoreSlim(1, 1);
        private static bool TableCreated;

        public AzureTableTransformationStateManager(CloudTableClient cloudTableClient)
        {
            if (cloudTableClient == null) throw new ArgumentNullException(nameof(cloudTableClient));

            var stateTableName = Environment.GetEnvironmentVariable("StateTableName");
            tableReference = cloudTableClient.GetTableReference(stateTableName);
        }

        private string GetKey<T>(string key)
        {
            return $"{typeof(T).Name}:{key}";
        }

        private string GetPartitionKey(string key)
        {
            // Long running process save data always using the process as prefix
            // we use it as partition key
            return key.Split(':')[0];
        }

        public async Task WriteStateAsync<T>(string key, T state, CancellationToken token = default)
        {
            await EnsureTableAsync();

            var entity = new StateEntity(GetPartitionKey(key), GetKey<T>(key), state);

            await tableReference.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        public async IAsyncEnumerable<KeyValuePair<string, T>> ListStateAsync<T>(string prefix, [EnumeratorCancellation] CancellationToken token = default)
        {
            await EnsureTableAsync();

            string partitionKey = GetPartitionKey(prefix);
            // Prepend the type
            prefix = GetKey<T>(prefix);

            var query = new TableQuery<StateEntity>();
            // Apply filter
            query.FilterString = $"{nameof(StateEntity.PartitionKey)} eq '{partitionKey}' and {nameof(StateEntity.RowKey)} ge '{prefix}'";

            TableContinuationToken tableToken = null;
            do
            {
                // Try to load next segment
                var segment = await tableReference.ExecuteQuerySegmentedAsync(query, tableToken);
                token.ThrowIfCancellationRequested();

                foreach (var stateEntity in segment)
                {
                    // Prefix search is not fully supported
                    if (!stateEntity.RowKey.StartsWith(prefix)) continue;

                    // Remove the type from the beginning of the row key
                    string key = stateEntity.RowKey.Split(':', 2)[1];
                    yield return new KeyValuePair<string, T>(key, stateEntity.GetTypedData<T>());
                    token.ThrowIfCancellationRequested();
                }

                tableToken = segment.ContinuationToken;
            } while (tableToken != null);
        }

        public async Task<T> ReadStateAsync<T>(string key, CancellationToken token = default)
        {
            await EnsureTableAsync();

            string rowKey = GetKey<T>(key);
            string partitionKey = GetPartitionKey(key);

            var result = await tableReference.ExecuteAsync(TableOperation.Retrieve<StateEntity>(partitionKey, rowKey), null, null, token);
            // Entity is available only if status code is 200 
            if (result.HttpStatusCode == 200)
            {
                return ((StateEntity)result.Result).GetTypedData<T>();
            }

            return default;
        }

        public async Task<bool> RemoveStateAsync<T>(string key, CancellationToken token = default)
        {
            await EnsureTableAsync();

            string rowKey = GetKey<T>(key);
            string partitionKey = GetPartitionKey(key);
            var entity = new TableEntity(partitionKey, rowKey) { ETag = "*" };
            try
            {
                var result = await tableReference.ExecuteAsync(TableOperation.Delete(entity), null, null, token);

                return result.HttpStatusCode == 200;
            }
            catch (StorageException se) when (se.RequestInformation.HttpStatusCode == 404)
            {
                return false;
            }
        }

        public async Task<bool> RemoveListStateAsync<T>(string prefix, CancellationToken token = default)
        {
            await EnsureTableAsync();

            string partitionKey = GetPartitionKey(prefix);

            const int batchSize = 200;

            bool r = false;

            TableBatchOperation batch = new TableBatchOperation();
            await foreach (var pair in ListStateAsync<T>(prefix, token))
            {
                // Append to the batch
                batch.Add(TableOperation.Delete(new TableEntity(partitionKey, pair.Key) { ETag = "*" }));
                r = true;

                // Batch is ready
                if (batch.Count == batchSize)
                {
                    // Execute batch
                    await tableReference.ExecuteBatchAsync(batch, null, null, token);
                    batch = new TableBatchOperation();
                }
            }

            if (batch.Count > 0)
            {
                await tableReference.ExecuteBatchAsync(batch, null, null, token);
            }

            return r;
        }

        private async Task EnsureTableAsync()
        {
            // Table already created
            if (TableCreated) return;

            // Try to acquire the lock
            await CreationSemaphoreSlim.WaitAsync();
            try
            {
                // Table already created
                if (TableCreated) return;

                await tableReference.CreateIfNotExistsAsync();
                TableCreated = true;
            }
            finally
            {
                CreationSemaphoreSlim.Release();
            }
        }

        private class StateEntity : TableEntity
        {
            public StateEntity()
            {

            }

            public StateEntity(string partitionKey, string rowKey, object data)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
                Data = JsonConvert.SerializeObject(data);
            }

            public string Data { get; set; }

            public T GetTypedData<T>()
            {
                return JsonConvert.DeserializeObject<T>(Data);
            }
        }
    }
}
