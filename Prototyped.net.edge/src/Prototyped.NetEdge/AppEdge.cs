using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdgeJs;

namespace Prototyped.NetEdge
{
    public class AppEdge : IDisposable
    {
        public Exception LastError { get; set; }

        public AppEdge() { }
        public void Dispose() { }

        public async Task<object> Ping(dynamic payload)
        {
            var func = Edge.Func(@"
                return function (data, callback) {
                    try
                    {
                        console.log(' - [ JS ] NodeJS welcomes \'' + data.ident + '\'!');
                        var extend = require('node.extend');
                        callback(null, extend(data, {
                            ended: Date.now(),
                            result: true,
                        }));
                    } catch (ex) {
                        data.error = ex;
                        data.ended = Date.now();
                        callback(ex, data);
                    }
                }
            ");

            return await func(payload);
        }

        public static AppEdge Create()
        {
            try
            {
                // Create a new instance
                var edge = new AppEdge { };

                // return new instance
                return edge;
            }
            catch (Exception ex)
            {
                // Something went wrong
                throw ex;
            }
        }

    }
}
