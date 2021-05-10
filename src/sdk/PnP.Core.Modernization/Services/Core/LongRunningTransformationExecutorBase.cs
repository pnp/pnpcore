using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Implementation of <see cref="ITransformationExecutor"/> which persists state for a long running operation
    /// </summary>
    public abstract class LongRunningTransformationExecutorBase : TransformationExecutorBase
    {
        protected LongRunningTransformationExecutorBase(
            ITransformationDistiller transformationDistiller,
            ITransformationStateManager transformationStateManager)
        {
            TransformationDistiller = transformationDistiller ?? throw new ArgumentNullException(nameof(transformationDistiller));
            TransformationStateManager = transformationStateManager ?? throw new ArgumentNullException(nameof(transformationStateManager));
        }

        protected ITransformationDistiller TransformationDistiller { get; }

        protected ITransformationStateManager TransformationStateManager { get; }

        public override async Task<TransformationExecutionStatus> GetStatusAsync(Guid processId)
        {
            string name = $"{processId}";
            var state = await TransformationStateManager.ReadStateAsync<TransformationExecutionStatus>(name);

            return state ?? new TransformationExecutionStatus(processId);
        }

        public override async Task<Guid> StartTransformAsync(PnPContext sourceContext, PnPContext targetContext)
        {
            // TODO: evaluate the usage of a Process instance
            var processId = Guid.NewGuid();

            // TODO: set status
            var status = new TransformationExecutionStatus(processId);
            await SetStatusAsync(status);
            await RaiseProgressAsync(status);

            var tasks = TransformationDistiller.GetTransformationTasks(sourceContext, targetContext);
            await foreach (var task in tasks)
            {
                await OnNewTaskAsync(processId, task);

                // TODO: set status
                status = new TransformationExecutionStatus(processId);
                await SetStatusAsync(status);
                await RaiseProgressAsync(status);
            }

            return processId;
        }

        public override async Task StopTransformAsync(Guid processId)
        {
            // TODO: change the state
            await SetStatusAsync(new TransformationExecutionStatus(processId));
        }

        protected virtual async Task SetStatusAsync(TransformationExecutionStatus status)
        {
            // TODO: evaluate the usage of object instead of string
            // and delegate the serialization to the state manager
            string name = $"{status.ProcessId}";
            await TransformationStateManager.WriteStateAsync(name, status);
        }

        protected virtual async Task OnNewTaskAsync(Guid processId, PageTransformationTask task)
        {
            // TODO: evaluate the usage of object instead of string
            // and delegate the serialization to the state manager
            string name = $"{processId}:{task.Id}";
            await TransformationStateManager.WriteStateAsync(name, task);
        }
        
    }
}
