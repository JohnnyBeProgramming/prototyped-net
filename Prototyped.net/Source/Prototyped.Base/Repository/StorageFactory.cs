using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base.Repository
{
    public class StorageFactory
    {
        public static CachedStorage Cached { get; private set; }

        public static readonly Dictionary<string, ARepository> Repo = new Dictionary<string, ARepository>();

        static StorageFactory()
        {
            Cached = Register<CachedStorage>("Cached in Memory");
        }

        public static TRepository Register<TRepository>(string name) where TRepository : ARepository, new()
        {
            return Register(name, () => new TRepository());
        }
        public static TRepository Register<TRepository>(string name, Func<TRepository> initObj, bool replace = false) where TRepository : ARepository
        {
            var hasRepo = Repo.ContainsKey(name);
            var instance = hasRepo ? Repo[name] : null;
            if ((instance == null) || replace)
            {
                instance = initObj();
            }

            Repo[name] = instance;

            return (TRepository)instance;
        }
    }
}
