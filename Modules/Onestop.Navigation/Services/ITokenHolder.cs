using System.Collections.Generic;
using Orchard;
using Orchard.Caching;

namespace Onestop.Navigation.Services
{
    public interface ITokenHolder : ISingletonDependency
    {
        bool TryGet<T>(T key, out IVolatileToken token);
        ITokenHolder Set<T>(T key, IVolatileToken token);
        bool TryRemove<T>(T key);
    }

    public class TokenHolder : ITokenHolder
    {
        private readonly IDictionary<object, IVolatileToken> _tokens = new Dictionary<object, IVolatileToken>();

        public bool TryGet<T>(T key, out IVolatileToken token)
        {
            lock (_tokens)
            {
                return _tokens.TryGetValue(key, out token);
            }
        }

        public ITokenHolder Set<T>(T key, IVolatileToken token)
        {
            lock (_tokens)
            {
                _tokens[key] = token;
                return this;
            }
        }

        public bool TryRemove<T>(T key)
        {
            lock (_tokens)
            {
                if (_tokens.ContainsKey(key))
                {
                    _tokens.Remove(key);
                    return true;
                }

                return false;
            }
        }
    }
}