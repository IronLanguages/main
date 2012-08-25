using IronRuby;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Reflection;

namespace IronApp
{
    public class DlrHost : ScriptHost
    {
        private readonly PlatformAdaptationLayer pal = new PAL();

        public class PAL : PlatformAdaptationLayer
        {
            public override Assembly LoadAssembly(string name)
            {
                if (name.StartsWith("IronRuby,", StringComparison.OrdinalIgnoreCase))
                {
                    return typeof(Ruby).GetTypeInfo().Assembly;
                }
                else if (name.StartsWith("IronRuby.Libraries,", StringComparison.OrdinalIgnoreCase))
                {
                    return typeof(IronRuby.Builtins.Integer).GetTypeInfo().Assembly;
                }

                return base.LoadAssembly(name);
            }
        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get { return pal; }
        }
    }
}
