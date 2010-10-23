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
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("IronRuby.Libraries.Yaml")]
[assembly: AssemblyCopyright("Copyright © Microsoft 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a1c8b506-e79a-4013-ae17-2e31618b5baf")]

[assembly: SecurityTransparent]

#if !SILVERLIGHT
[assembly: AssemblyVersion(RubyContext.IronRubyVersionString)]
[assembly: AssemblyFileVersion(RubyContext.IronRubyVersionString)]
[assembly: AllowPartiallyTrustedCallers]
#if !CLR2
[assembly: SecurityRules(SecurityRuleSet.Level1)]
#endif
#endif