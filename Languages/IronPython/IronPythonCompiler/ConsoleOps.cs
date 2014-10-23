using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronPythonCompiler {
    public static class ConsoleOps {

        public static void Error(string format, params object[] args) {
            Error(false, format, args);
        }

        public static void Error(bool fatal, string format, params object[] args) {
            ConsoleColor origForeground = Console.ForegroundColor;
            ConsoleColor origBackground = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("ERROR: " + format, args);
            Console.ForegroundColor = origForeground;
            Console.BackgroundColor = origBackground;
            if (fatal) {
                Environment.Exit(-1);
            }
        }

        public static void Warning(string format, params object[] args) {
            ConsoleColor origForeground = Console.ForegroundColor;
            ConsoleColor origBackground = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING: " + format, args);
            Console.ForegroundColor = origForeground;
            Console.BackgroundColor = origBackground;
        }

        public static void Info(string format, params object[] args) {
            Console.WriteLine(format, args);
        }

        public static void Usage(bool doExit = false) {
            Console.WriteLine(@"ipyc: The Command-Line IronPython Compiler

Usage: ipyc.exe [options] file [file ...]

Options:
    /out:output_file                          Output file name (default is main_file.<extenstion>)
    /target:dll                               Compile only into dll.  Default
    /target:exe                               Generate CONSOLE executable stub for startup in addition to dll.
    /target:winexe                            Generate WINDOWS executable stub for startup in addition to dll.
    /file_info_version:<version>              Set the file/assembly version
    @<file>                                   Specifies a response file to be parsed for input files and command line options (one per line)
    /? /h                                     This message    

EXE/WinEXE specific options:
    /main:main_file.py                        Main file of the project (module to be executed first)
    /platform:x86                             Compile for x86 only
    /platform:x64                             Compile for x64 only
    /embed                                    Embeds the generated DLL as a resource into the executable which is loaded at runtime
    /standalone                               Embeds the IronPython assemblies into the stub executable.
    /mta                                      Set MTAThreadAttribute on Main instead of STAThreadAttribute, only valid for /target:winexe
    /errfmt:msg                               A string that will be used when showing an error occured, {{0}} will be replaced by the exception message
    /file_info_product:<name>                 Set product name in executable meta information
    /file_info_product_version:<version>      Set product version in executable meta information
    /file_info_company:<name>                 Set company name in executable meta information
    /file_info_copyright:<info>               Set copyright information in executable meta information
    /file_info_trademark:<info>               Set trademark information in executable meta information

Example:
    ipyc.exe /main:Program.py Form.py /target:winexe");

            if (doExit) {
                Environment.Exit(0);
            }
        }
    }
}
