/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Stores information needed to emit debugging symbol information for a
    /// source file, in particular the file name and unique language identifier
    /// </summary>
    public class SymbolDocumentInfo {
        private readonly string _fileName;

        internal SymbolDocumentInfo(string fileName) {
            ContractUtils.RequiresNotNull(fileName, "fileName");
            _fileName = fileName;
        }

        /// <summary>
        /// The source file name
        /// </summary>
        public string FileName {
            get { return _fileName; }
        }

        /// <summary>
        /// Returns the language's unique identifier, if any
        /// </summary>
        public virtual Guid Language {
            get { return Guid.Empty; }
        }

        /// <summary>
        /// Returns the language vendor's unique identifier, if any
        /// </summary>
        public virtual Guid LanguageVendor {
            get { return Guid.Empty; }
        }

        /// <summary>
        /// Returns the document type's unique identifier, if any
        /// Defaults to the guid for a text file
        /// </summary>
        public virtual Guid DocumentType {
            get { return Compiler.SymbolGuids.DocumentType_Text; }
        }
    }

    internal sealed class SymbolDocumentWithGuids : SymbolDocumentInfo {
        private readonly Guid _language;
        private readonly Guid _vendor;
        private readonly Guid _documentType;

        internal SymbolDocumentWithGuids(string fileName, ref Guid language)
            : base(fileName) {
            _language = language;
        }

        internal SymbolDocumentWithGuids(string fileName, ref Guid language, ref Guid vendor)
            : base(fileName) {
            _language = language;
            _vendor = vendor;
        }

        internal SymbolDocumentWithGuids(string fileName, ref Guid language, ref Guid vendor, ref Guid documentType)
            : base(fileName) {
            _language = language;
            _vendor = vendor;
            _documentType = documentType;
        }

        public override Guid Language {
            get { return _language; }
        }

        public override Guid LanguageVendor {
            get { return _vendor; }
        }

        public override Guid DocumentType {
            get { return _documentType; }
        }
    }

    public partial class Expression {
        public static SymbolDocumentInfo SymbolDocument(string fileName) {
            return new SymbolDocumentInfo(fileName);
        }
        public static SymbolDocumentInfo SymbolDocument(string fileName, Guid language) {
            return new SymbolDocumentWithGuids(fileName, ref language);
        }
        public static SymbolDocumentInfo SymbolDocument(string fileName, Guid language, Guid languageVendor) {
            return new SymbolDocumentWithGuids(fileName, ref language, ref languageVendor);
        }
        public static SymbolDocumentInfo SymbolDocument(string fileName, Guid language, Guid languageVendor, Guid documentType) {
            return new SymbolDocumentWithGuids(fileName, ref language, ref languageVendor, ref documentType);
        }
    }
}
