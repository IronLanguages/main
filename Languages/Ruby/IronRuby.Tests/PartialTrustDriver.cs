#if !WIN8
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Text;

namespace IronRuby.Tests
{
    public static class PartialTrustDriver
    {
        public static int Run(List<string> arguments)
        {
            PermissionSet ps = CreatePermissionSet();
            AppDomainSetup setup = new AppDomainSetup();

            setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain domain = AppDomain.CreateDomain("Tests", null, setup, ps);

            Loader loader = new Loader(arguments, setup.ApplicationBase);
            domain.DoCallBack(new CrossAppDomainDelegate(loader.Run));
            return loader.ExitCode;
        }

        public sealed class Loader : MarshalByRefObject
        {
            public int ExitCode;
            public readonly List<string>/*!*/ Args;
            public readonly string/*!*/ BaseDirectory;

            public Loader(List<string>/*!*/ args, string/*!*/ baseDirectory)
            {
                Args = args;
                BaseDirectory = baseDirectory;
            }

            public void Run()
            {
                ExitCode = Driver.Run(Args, BaseDirectory);
            }
        }

        private static PermissionSet/*!*/ CreatePermissionSet()
        {
#if CLR2
            string name = "Internet";
            bool foundName = false;
            PermissionSet setIntersection = new PermissionSet(PermissionState.Unrestricted);

            // iterate over each policy level
            IEnumerator e = SecurityManager.PolicyHierarchy();
            while (e.MoveNext()) {
                PolicyLevel level = (PolicyLevel)e.Current;
                PermissionSet levelSet = level.GetNamedPermissionSet(name);
                if (levelSet != null) {
                    foundName = true;
                    setIntersection = setIntersection.Intersect(levelSet);
                }
            }

            if (setIntersection == null || !foundName) {
                setIntersection = new PermissionSet(PermissionState.None);
            } else {
                setIntersection = new NamedPermissionSet(name, setIntersection);
            }

            return setIntersection;
#else
            // this functionality is not available on Mono (AddHostEvidence is undefined), use dynamic to resolve it at runtime
            dynamic e = new Evidence();
            e.AddHostEvidence(new Zone(SecurityZone.Internet));
            return SecurityManager.GetStandardSandbox((Evidence)e);
#endif
        }       
    }
}
#endif