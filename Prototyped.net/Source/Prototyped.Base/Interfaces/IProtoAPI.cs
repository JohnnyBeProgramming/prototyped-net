using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base.Interfaces
{
    /// <summary>
    /// Define the base app runtime
    /// </summary>
    public interface IProtoAPI
    {
        void Init();
        void Destroy();
    }
}
