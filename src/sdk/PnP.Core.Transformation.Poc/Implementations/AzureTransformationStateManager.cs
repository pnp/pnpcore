using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.Poc.Implementations
{
    public class AzureTransformationStateManager : ITransformationStateManager
    {
        private readonly CloudTable tableReference;

        private static readonly SemaphoreSlim CreationSemaphoreSlim = new SemaphoreSlim(1, 1);
        private static bool TableCreated;

        public AzureTransformationStateManager(CloudTableClient cloudTableClient)
        {
            if (cloudTableClient == null) throw new ArgumentNullException(nameof(cloudTableClient));

            var stateTableName = Environment.GetEnvironmentVariable("StateTableName");
            tableReference = cloudTableClient.GetTableReference(stateTableName);
        }

        public async Task WriteStateAsync<T>(object key, T state, CancellationToken token = default)
        {
            await EnsureTableAsync();

            ITableEntity entity;
            if (state is TransformationProcessStatus s)
            {
                entity = new StateEntity(s.ProcessId.ToString(), s.ProcessId.ToString(), state);
            }
            else
            {
                throw new NotSupportedException();
            }

            await tableReference.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        public async Task<T> ReadStateAsync<T>(object key, CancellationToken token = default)
        {
            await EnsureTableAsync();

            string partitionKey, rowKey;
            if (typeof(T) == typeof(TransformationProcessStatus))
            {
                partitionKey = key.ToString();
                rowKey = partitionKey;
            }
            else
            {
                throw new NotSupportedException();
            }

            var result = await tableReference.ExecuteAsync(TableOperation.Retrieve<StateEntity>(partitionKey, rowKey));
            return ((StateEntity) result.Result).GetTypedData<T>();
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
            public StateEntity(string partitionKey, string rowKey, object data)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
                Data = JsonConvert.SerializeObject(data);
            }

            public string Data { get; }

            public T GetTypedData<T>()
            {
                return JsonConvert.DeserializeObject<T>(Data);
            }
        }
    }
}
