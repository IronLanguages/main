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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.IronStudio.Core;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.Language.StandardClassification;   
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronStudio.Core {
    /// <summary>
    /// Implements classification of text by using a ScriptEngine which supports the
    /// TokenCategorizer service.
    /// 
    /// Languages should subclass this type and override the Engine property. They 
    /// should then export the provider using MEF indicating the content type 
    /// which it is applicable to.
    /// 
    /// Languages can also export this under the the content type "DlrCode" and can then
    /// override the ContentType property to indicate a specific content
    /// type which they provide tokenization for.  This specific content type should be
    /// a sub-type of DlrCode.  This enables the languages to dynamicly register their 
    /// own content type but statically export the provider.
    /// </summary>
    public abstract class DlrClassifierProvider : IDlrClassifierProvider {
        private Dictionary<TokenCategory, IClassificationType> _categoryMap;
        private IClassificationType _comment;
        private IClassificationType _stringLiteral;
        private IClassificationType _keyword;
        private IClassificationType _operator;
        private IClassificationType _openGroupingClassification;
        private IClassificationType _loseGroupingClassification;
        private IClassificationType _dotClassification;
        private IClassificationType _commaClassification;

        /// <summary>
        /// Import the classification registry to be used for getting a reference
        /// to the custom classification type later.
        /// </summary>
        [Import]
        public IClassificationTypeRegistryService _classificationRegistry = null; // Set via MEF

        #region DLR Classification Type Definitions

        [Export]
        [Name(DlrPredefinedClassificationTypeNames.OpenGrouping)]
        [BaseDefinition(DlrPredefinedClassificationTypeNames.ScriptOperator)]
        internal static ClassificationTypeDefinition OpenGroupingClassificationDefinition = null; // Set via MEF

        [Export]
        [Name(DlrPredefinedClassificationTypeNames.CloseGrouping)]
        [BaseDefinition(DlrPredefinedClassificationTypeNames.ScriptOperator)]
        internal static ClassificationTypeDefinition CloseGroupingClassificationDefinition = null; // Set via MEF

        [Export]
        [Name(DlrPredefinedClassificationTypeNames.Dot)]
        [BaseDefinition(DlrPredefinedClassificationTypeNames.ScriptOperator)]
        internal static ClassificationTypeDefinition DotClassificationDefinition = null; // Set via MEF

        [Export]
        [Name(DlrPredefinedClassificationTypeNames.Comma)]
        [BaseDefinition(DlrPredefinedClassificationTypeNames.ScriptOperator)]
        internal static ClassificationTypeDefinition CommaClassificationDefinition = null; // Set via MEF

        #endregion

        #region IDlrClassifierProvider

        public IClassifier GetClassifier(ITextBuffer buffer) {
            IDlrClassifier existing;
            if (buffer.Properties.TryGetProperty<IDlrClassifier>(typeof(IDlrClassifier), out existing)) {
                return existing;
            }
            
            if (_categoryMap == null) {
                _categoryMap = FillCategoryMap(_classificationRegistry);
            }
            var specificContentType = ContentType;

            DlrClassifier res = null;
            if (specificContentType == null || buffer.ContentType == specificContentType) {
                res = new DlrClassifier(this, Engine, buffer);
            } else {

                foreach (var baseType in buffer.ContentType.BaseTypes) {
                    if (baseType == specificContentType) {
                        res = new DlrClassifier(this, Engine, buffer);
                        break;
                    }
                }
            }

            if (res != null) {
                buffer.Properties.AddProperty(typeof(IDlrClassifier), res);
            }

            return res;
        }

        public abstract ScriptEngine Engine {
            get;
        }

        public virtual IContentType ContentType {
            get { return null; }
        }

        public IClassificationType Comment {
            get { return _comment; }
        }

        public IClassificationType StringLiteral {
            get { return _stringLiteral; }
        }

        public IClassificationType Keyword {
            get { return _keyword; }
        }

        public IClassificationType Operator {
            get { return _operator; }
        }

        public IClassificationType OpenGroupingClassification {
            get { return _openGroupingClassification; }
        }

        public IClassificationType CloseGroupingClassification {
            get { return _loseGroupingClassification; }
        }

        public IClassificationType DotClassification {
            get { return _dotClassification; }
        }

        public IClassificationType CommaClassification {
            get { return _commaClassification; }
        }

        #endregion

        internal Dictionary<TokenCategory, IClassificationType> CategoryMap {
            get { return _categoryMap; }
        }

        private Dictionary<TokenCategory, IClassificationType> FillCategoryMap(IClassificationTypeRegistryService registry) {
            var categoryMap = new Dictionary<TokenCategory, IClassificationType>();

            categoryMap[TokenCategory.DocComment] = _comment = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            categoryMap[TokenCategory.LineComment] = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            categoryMap[TokenCategory.Comment] = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            categoryMap[TokenCategory.NumericLiteral] = registry.GetClassificationType(PredefinedClassificationTypeNames.Literal);
            categoryMap[TokenCategory.CharacterLiteral] = registry.GetClassificationType(PredefinedClassificationTypeNames.Character);
            categoryMap[TokenCategory.StringLiteral] = _stringLiteral = registry.GetClassificationType(PredefinedClassificationTypeNames.String);
            categoryMap[TokenCategory.Keyword] = _keyword = registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
            categoryMap[TokenCategory.Directive] = registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
            categoryMap[TokenCategory.Identifier] = registry.GetClassificationType(PredefinedClassificationTypeNames.Identifier);
            categoryMap[TokenCategory.Operator] = _operator = registry.GetClassificationType(DlrPredefinedClassificationTypeNames.ScriptOperator);
            categoryMap[TokenCategory.Delimiter] = _operator;
            categoryMap[TokenCategory.Grouping] = _operator;
            categoryMap[TokenCategory.WhiteSpace] = registry.GetClassificationType(PredefinedClassificationTypeNames.WhiteSpace);
            categoryMap[TokenCategory.RegularExpressionLiteral] = registry.GetClassificationType(PredefinedClassificationTypeNames.Literal);
            _openGroupingClassification = registry.GetClassificationType(DlrPredefinedClassificationTypeNames.OpenGrouping);
            _loseGroupingClassification = registry.GetClassificationType(DlrPredefinedClassificationTypeNames.CloseGrouping);
            _commaClassification = registry.GetClassificationType(DlrPredefinedClassificationTypeNames.Comma);
            _dotClassification = registry.GetClassificationType(DlrPredefinedClassificationTypeNames.Dot);
            
            return categoryMap;
        }
    }

    #region Editor Format Definitions

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DlrPredefinedClassificationTypeNames.OpenGrouping)]
    [Name("Open grouping")]
    [DisplayName("Open grouping character")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class OpenGroupingFormat : ClassificationFormatDefinition { }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DlrPredefinedClassificationTypeNames.CloseGrouping)]
    [Name("Close grouping")]
    [DisplayName("Close grouping character")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class CloseGroupingFormat : ClassificationFormatDefinition { }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DlrPredefinedClassificationTypeNames.Comma)]
    [Name("Comma")]
    [DisplayName("Comma character")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class CommaFormat : ClassificationFormatDefinition { }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DlrPredefinedClassificationTypeNames.Dot)]
    [Name("Dot")]
    [DisplayName("Dot character")]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    internal sealed class DotFormat : ClassificationFormatDefinition { }

    #endregion
}
