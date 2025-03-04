using System.Collections.Generic;

namespace Telegraphist.Helpers.Provider
{
    public interface IProvider
    {
        T Get<T>() where T : IInjectable;
        List<T> GetAll<T>() where T : IInjectable;
    }
}