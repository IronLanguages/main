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

using System;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronStudio {
    public interface IDlrClassifierProvider : IClassifierProvider {

        ScriptEngine Engine {
            get;
        }

        IContentType ContentType {
            get;
        }

        IClassificationType Comment {
            get;
        }
        
        IClassificationType StringLiteral {
            get;
        }

        IClassificationType Keyword {
            get;
        }

        IClassificationType Operator {
            get;
        }

        IClassificationType OpenGroupingClassification {
            get;
        }

        IClassificationType CloseGroupingClassification {
            get;
        }

        IClassificationType DotClassification {
            get;
        }

        IClassificationType CommaClassification {
            get;
        }
    }
}
