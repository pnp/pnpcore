using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            BatchRequest = batch.Requests.First(r => r.Value.Id == batchRequestId).Value;
        }
    }
    
    internal class BatchSingleResult<T> : BatchResult, IBatchSingleResult<T>
    {
        public static IBatchSingleResult<T> None { get; } = new BatchSingleResult<T>(null, Guid.Empty);

        public BatchSingleResult(Batch batch, Guid batchRequestId) : base(batch, batchRequestId)
        {
        }
        
        public object ObjectResult => Result;

        public T Result
        {
            get
            {
                if (!IsAvailable)
                {
                    // TODO: use resources
                    throw new InvalidOperationException("Cannot access the result since batch is not available yet");
                }
                return (T)(object)BatchRequest.Model;
            }
        }
    }
}
