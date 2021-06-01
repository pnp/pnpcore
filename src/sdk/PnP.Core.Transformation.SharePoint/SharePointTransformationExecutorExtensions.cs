using System;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.SharePoint
{
    /// <summary>
    /// Extensions for <see cref="ITransformationProcess"/>
    /// </summary>
    public static class SharePointTransformationExecutorExtensions
    {

        /// <summary>
        /// Creates a new transformation process for SharePoint
        /// </summary>
        /// <param name="transformationProcess">The process to use</param>
        /// <param name="sourceContext">The source context</param>
        /// <param name="targetContext">The target context</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public static Task StartSharePointProcessAsync(
            this ITransformationProcess transformationProcess,
            PnPContext sourceContext,
            PnPContext targetContext,
            CancellationToken token = default)
        {
            if (transformationProcess == null) throw new ArgumentNullException(nameof(transformationProcess));

            // Create the provider
            var sourceProvider = new SharePointSourceProvider(sourceContext);

            return transformationProcess.StartProcessAsync(sourceProvider, targetContext, token);
        }

        /// <summary>
        /// Creates a new transformation process for SharePoint and wait for its completion
        /// </summary>
        /// <param name="transformationExecutor">The executor to use</param>
        /// <param name="sourceContext">The source context</param>
        /// <param name="targetContext">The target context</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public static Task<TransformationProcessStatus> TransformSharePointAsync(
            this ITransformationExecutor transformationExecutor,
            PnPContext sourceContext,
            PnPContext targetContext,
            CancellationToken token = default)
        {
            if (transformationExecutor == null) throw new ArgumentNullException(nameof(transformationExecutor));

            // Create the provider
            var sourceProvider = new SharePointSourceProvider(sourceContext);

            return transformationExecutor.TransformAsync(sourceProvider, targetContext, token);
        }

        /// <summary>
        /// Creates a new transformation process for SharePoint
        /// </summary>
        /// <param name="transformationProcess">The process to use</param>
        /// <param name="pnpContextFactory">The context factory to use</param>
        /// <param name="sourceName">The source site name</param>
        /// <param name="targetName">The target site name</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public static async Task StartProcessAsync(
            this ITransformationProcess transformationProcess,
            IPnPContextFactory pnpContextFactory,
            string sourceName,
            string targetName,
            CancellationToken token = default)
        {
            if (transformationProcess == null) throw new ArgumentNullException(nameof(transformationProcess));
            if (pnpContextFactory == null) throw new ArgumentNullException(nameof(pnpContextFactory));

            // Create contexts
            var sourceContext = await pnpContextFactory.CreateAsync(sourceName).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            var targetContext = await pnpContextFactory.CreateAsync(targetName).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            await transformationProcess.StartSharePointProcessAsync(sourceContext, targetContext, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new transformation process for SharePoint and wait for its completion
        /// </summary>
        /// <param name="transformationExecutor">The executor to use</param>
        /// <param name="pnpContextFactory">The context factory to use</param>
        /// <param name="sourceName">The source site name</param>
        /// <param name="targetName">The target site name</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public static async Task<TransformationProcessStatus> TransformSharePointAsync(
            this ITransformationExecutor transformationExecutor,
            IPnPContextFactory pnpContextFactory,
            string sourceName,
            string targetName,
            CancellationToken token = default)
        {
            if (transformationExecutor == null) throw new ArgumentNullException(nameof(transformationExecutor));
            if (pnpContextFactory == null) throw new ArgumentNullException(nameof(pnpContextFactory));

            // Create contexts
            var sourceContext = await pnpContextFactory.CreateAsync(sourceName).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            var targetContext = await pnpContextFactory.CreateAsync(targetName).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            return await transformationExecutor.TransformSharePointAsync(sourceContext, targetContext, token).ConfigureAwait(false);
        }
    }
}
