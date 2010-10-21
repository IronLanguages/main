/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.Project
{
	public class ProjectOptions : System.CodeDom.Compiler.CompilerParameters
	{
		public ModuleKindFlags ModuleKind { get; set; }

		public bool EmitManifest { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public StringCollection DefinedPreprocessorSymbols { get; set; }

		public string XmlDocFileName { get; set; }

		public string RecursiveWildcard { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public StringCollection ReferencedModules { get; set; }

		public string Win32Icon { get; set; }

		public bool PdbOnly { get; set; }

		public bool Optimize { get; set; }

		public bool IncrementalCompile { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public int[] SuppressedWarnings { get; set; }

		public bool CheckedArithmetic { get; set; }

		public bool AllowUnsafeCode { get; set; }

		public bool DisplayCommandLineHelp { get; set; }

		public bool SuppressLogo { get; set; }

		public long BaseAddress { get; set; }

		public string BugReportFileName { get; set; }

		/// <devdoc>must be an int if not null</devdoc>
		public object CodePage { get; set; }

		public bool EncodeOutputInUtf8 { get; set; }

		public bool FullyQualifyPaths { get; set; }

		public int FileAlignment { get; set; }

		public bool NoStandardLibrary { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public StringCollection AdditionalSearchPaths { get; set; }

		public bool HeuristicReferenceResolution { get; set; }

		public string RootNamespace { get; set; }

		public bool CompileAndExecute { get; set; }

		/// <devdoc>must be an int if not null.</devdoc>
		public object UserLocaleId { get; set; }

		public PlatformType TargetPlatform { get; set; }

		public string TargetPlatformLocation { get; set; }

		public ProjectOptions()
		{
			EmitManifest = true;
			ModuleKind = ModuleKindFlags.ConsoleApplication;
		}

		public virtual string GetOptionHelp()
		{
			return null;
		}
	}
}
