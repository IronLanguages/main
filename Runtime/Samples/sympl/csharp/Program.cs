using System;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace SymplSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup DLR ScriptRuntime with our languages.  We hardcode them here
            // but a .NET app looking for general language scripting would use
            // an app.config file and ScriptRuntime.CreateFromConfiguration.
            var setup = new ScriptRuntimeSetup();
            string qualifiedname = typeof(SymplSample.Hosting.SymplLangContext).AssemblyQualifiedName;
            setup.LanguageSetups.Add(new LanguageSetup(
                qualifiedname, "Sympl", new[] { "sympl" }, new[] { ".sympl" }));
            setup.LanguageSetups.Add(
                IronPython.Hosting.Python.CreateLanguageSetup(null));
            setup.LanguageSetups.Add(IronRuby.Ruby.CreateRubySetup());
            var dlrRuntime = new ScriptRuntime(setup);
            // Don't need to tell the DLR about the assemblies we want to be
            // available, which the SymplLangContext constructor passes to the
            // Sympl constructor, because the DLR loads mscorlib and System by
            // default.
            //dlrRuntime.LoadAssembly(typeof(object).Assembly);
            
            // Get a Sympl engine and run stuff ...
            var engine = dlrRuntime.GetEngine("sympl");
            string filename = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), 
                    @"..\..\Runtime\Samples\sympl\examples\test.sympl"
                )
            );
            Console.WriteLine("Executing " + filename);
            var feo = engine.ExecuteFile(filename);
            Console.WriteLine("ExecuteExpr ... ");
            engine.Execute("(print 5)", feo);

            // Get Python and Ruby engines
            var pyeng = dlrRuntime.GetEngine("Python");
            var rbeng = dlrRuntime.GetEngine("Ruby");
            // Run some Python and Ruby code in our shared Sympl module scope.
            pyeng.Execute("def pyfoo(): return 1", feo);
            rbeng.Execute("def rbbar; 2; end", feo);
            // Call those objects from Sympl.
            Console.WriteLine("pyfoo returns " + 
                              (engine.Execute("(pyfoo)", feo)).ToString());
            Console.WriteLine("rbbar returns " +
                              (engine.Execute("(rbbar)", feo)).ToString());

            // Consume host supplied globals via DLR Hosting.
            dlrRuntime.Globals.SetVariable("DlrGlobal", new int[] { 3, 7 });
            engine.Execute("(import dlrglobal)", feo);
            engine.Execute("(print (elt dlrglobal 1))", feo);

            // Drop into the REPL ...
            if (args.Length > 0 && args[0] == "norepl") return;
            string input = null;
            string exprstr = "";
            Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
            Console.WriteLine("Enter expressions.  Enter blank line to abort input.");
            Console.WriteLine("Enter 'exit (the symbol) to exit.");
            Console.WriteLine();
            string prompt = ">>> ";
            var s = engine.GetService<Sympl>();
            while (true) {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (input == "") {
                    exprstr = "";
                    prompt = ">>> ";
                    continue;
                } else {
                    exprstr = exprstr + " " + input;
                }
                // See if we have complete input.
                try {
                    var ast = new Parser().ParseExpr(new StringReader(exprstr));
                }
                catch (Exception) {
                    prompt = "... ";
                    continue;
                }
                // We do, so execute.
                try {
                    object res = engine.Execute(exprstr, feo);
                    exprstr = "";
                    prompt = ">>> ";
                    if (res == s.MakeSymbol("exit")) return;
                    Console.WriteLine(res);
                } catch (Exception e) {
                    exprstr = "";
                    prompt = ">>> ";
                    Console.Write("ERROR: ");
                    Console.WriteLine(e);
                }
            }
        }
    }
}
