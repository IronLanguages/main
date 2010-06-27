Cassini Web Server Sample v3.5 README.TXT
------------------------------------------

This Cassini version requires .NET Framework v3.5

This sample illustrates using the ASP.NET hosting APIs (System.Web.Hosting)
to create a simple managed Web Server with System.Net APIs.

New in Cassini v3.5:
* Runs as a single EXE -- does not require an assembly in GAC
* Supported IPv6-only configurations
* Upgraded to support .NET Framework 3.5
* Includes VS project file
* License changed to Ms-PL

Instructions to Run Cassini
---------------------------

Cassini-v35 <physical-path> <port> <virtual-path>
For example:
    Cassini-v35 c:\ 80 /
