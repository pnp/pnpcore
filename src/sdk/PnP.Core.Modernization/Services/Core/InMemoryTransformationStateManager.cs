using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// In memory implementation of <see cref="ITransformationStateManager"/>
    /// <remarks>Class is thread safety</remarks>
    /// </summary>
    public class InMemoryTransformationStateManager : ITransformationStateManager
    {
        private readonly ConcurrentDictionary<Tuple<Type, string>, object> states = new ConcurrentDictionary<Tuple<Type, string>, object>();

        /// <summary>
        /// Allows to write a state variable for a specific Transformation process
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="name">The name of the state variable</param>
        /// <param name="state">The value of the state variable</param>
        /// <returns></returns>
        public Task WriteStateAsync<T>(string name, T state)
        {
            var key = Tuple.Create(typeof(T), name);
            states.AddOrUpdate(key, state, (k, o) => state);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows to read a state variable for a specific Transformation process
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="name">The name of the state variable</param>
        /// <returns>The value of the state variable</returns>
        public Task<T> ReadStateAsync<T>(string name)
        {
            var key = Tuple.Create(typeof(T), name);
            if (states.TryGetValue(key, out var value) && value is T v)
            {
                return Task.FromResult(v);
            }

            return default;
        }
    }
}
