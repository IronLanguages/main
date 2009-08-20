using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using IronRuby.Runtime;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("IronRuby.Libraries.Yaml")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("MSIT")]
[assembly: AssemblyProduct("IronRuby.Libraries.Yaml")]
[assembly: AssemblyCopyright("Copyright © MSIT 2008")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a1c8b506-e79a-4013-ae17-2e31618b5baf")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(RubyContext.IronRubyVersionString)]
[assembly: AssemblyFileVersion(RubyContext.IronRubyVersionString)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]

#if CLR4
[assembly: SecurityRules(SecurityRuleSet.Level1)]
#endif