using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PnP.Core.Model
{
    internal class BatchResult : IBatchResult
    {
        public Batch Batch { get; }

        public bool IsAvailable => Batch.Executed;

        public BatchRequest BatchRequest { get; }

        public BatchResult(Batch batch, Guid batchRequestId)
        {
            Batch = batch;
            BatchRequest = batch.GetRequest(batchRequestId);
        }
    }

    internal class BatchSingleResult<T> : BatchResult, IBatchSingleResult<T>
    {
        private readonly T result;

        public static IBatchSingleResult<T> None { get; } = new BatchSingleResult<T>(null, Guid.Empty);

        public BatchSingleResult(Batch batch, Guid batchRequestId) : base(batch, batchRequestId)
        {
        }

        public BatchSingleResult(Batch batch, Guid batchRequestId, T resultObject) : base(batch, batchRequestId)
        {
            result = resultObject;
        }

        public object ObjectResult => Result;

        public T Result
        {
            get
            {
                if (!IsAvailable)
                {
                    throw new InvalidOperationException(PnPCoreResources.Exception_BatchResult_BatchNotYetExecuted);
                }

                if (BatchRequest.ApiCall.RawRequest)
                {
                    return result;
                }
                else
                {
                    return (T)(object)BatchRequest.Model;
                }
            }
        }
    }

    internal class BatchEnumerableBatchResult<T> : BatchResult, IEnumerableBatchResult<T>
    {
        private readonly IReadOnlyList<T> result;

        public BatchEnumerableBatchResult(Batch batch, Guid batchRequestId, IReadOnlyList<T> result) : base(batch, batchRequestId)
        {
            this.result = result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException(PnPCoreResources.Exception_BatchResult_BatchNotYetExecuted);
            }

            return result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => result.Count;

        public T this[int index] => result[index];
    }
}
