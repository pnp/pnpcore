using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
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

            public StateEntity(TransformationProcessStatus status)
            {
                PartitionKey = status.ProcessId.ToString();
                RowKey = status.ProcessId.ToString();
                Data = JsonConvert.SerializeObject(status);
                State = status.State.ToString();
            }

            public StateEntity(TransformationProcessTaskStatus status)
            {
                PartitionKey = status.ProcessId.ToString();
                RowKey = status.Id.ToString();
                Data = JsonConvert.SerializeObject(status);
                State = status.State.ToString();
            }

            public string Data { get; set; }

            public string State { get; set; }

            public TransformationProcessStatus GetProcessStatus()
            {
                return JsonConvert.DeserializeObject<TransformationProcessStatus>(Data);
            }

            public TransformationProcessTaskStatus GetTaskStatus()
            {
                return JsonConvert.DeserializeObject<TransformationProcessTaskStatus>(Data);
            }
        }

        public async Task WriteProcessStatusAsync(TransformationProcessStatus status, CancellationToken token = default)
        {
            await EnsureTableAsync();

            var entity = new StateEntity(status);

            await tableReference.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        public async Task WriteTaskStatusAsync(TransformationProcessTaskStatus status, CancellationToken token = default)
        {
            await EnsureTableAsync();

            var entity = new StateEntity(status);

            await tableReference.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        public async IAsyncEnumerable<TransformationProcessTaskStatus> GetProcessTasksStatus(Guid processId, TasksStatusQuery query, CancellationToken token = default)
        {
            await EnsureTableAsync();

            string partitionKey = processId.ToString();
            // Prepend the type

            var tableQuery = new TableQuery<StateEntity>();
            
            if (query.State.HasValue)
            {
                // Apply filter
                tableQuery = tableQuery.Where($"{nameof(StateEntity.PartitionKey)} eq '{partitionKey}' and {nameof(StateEntity.State)} eq '{query.State}'");
            }
            else
            {
                tableQuery = tableQuery.Where($"{nameof(StateEntity.PartitionKey)} eq '{partitionKey}'");
            }

            TableContinuationToken tableToken = null;
            do
            {
                // Try to load next segment
                var segment = await tableReference.ExecuteQuerySegmentedAsync(tableQuery, tableToken);
                token.ThrowIfCancellationRequested();

                foreach (var stateEntity in segment)
                {
                    // Remove the type from the beginning of the row key
                    yield return stateEntity.GetTaskStatus();
                    token.ThrowIfCancellationRequested();
                }

                tableToken = segment.ContinuationToken;
            } while (tableToken != null);
        }

        public async Task<TransformationProcessStatus> ReadProcessStatusAsync(Guid processId, CancellationToken token = default)
        {
            await EnsureTableAsync();

            string rowKey = processId.ToString();
            string partitionKey = processId.ToString();

            var result = await tableReference.ExecuteAsync(TableOperation.Retrieve<StateEntity>(partitionKey, rowKey), null, null, token);
            // Entity is available only if status code is 200 
            if (result.HttpStatusCode == 200)
            {
                return ((StateEntity)result.Result).GetProcessStatus();
            }

            return default;
        }

        public async Task<TransformationProcessTaskStatus> ReadTaskStatusAsync(Guid processId, Guid taskId, CancellationToken token = default)
        {
            await EnsureTableAsync();

            string rowKey = taskId.ToString();
            string partitionKey = processId.ToString();

            var result = await tableReference.ExecuteAsync(TableOperation.Retrieve<StateEntity>(partitionKey, rowKey), null, null, token);
            // Entity is available only if status code is 200 
            if (result.HttpStatusCode == 200)
            {
                return ((StateEntity)result.Result).GetTaskStatus();
            }

            return default;
        }

        public async Task<bool> RemoveProcessStatusAsync(Guid processId, CancellationToken token = default)
        {
            await EnsureTableAsync();

            string rowKey = processId.ToString();
            string partitionKey = processId.ToString();
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
        public async Task<bool> RemoveTaskStatusAsync(Guid processId, Guid taskId, CancellationToken token = default)
        {
            await EnsureTableAsync();

            string rowKey = taskId.ToString();
            string partitionKey = processId.ToString();
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
    }
}
