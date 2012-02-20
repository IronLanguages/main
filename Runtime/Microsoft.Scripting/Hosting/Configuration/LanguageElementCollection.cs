/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if FEATURE_CONFIGURATION

using System.Configuration;
using System;
using System.Collections.Generic;

namespace Microsoft.Scripting.Hosting.Configuration {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public class LanguageElementCollection : ConfigurationElementCollection {
        public override ConfigurationElementCollectionType CollectionType {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override bool ThrowOnDuplicate {
            get { return false; }
        }

        protected override ConfigurationElement CreateNewElement() {
            return new LanguageElement();
        }

        protected override string ElementName {
            get { return "language"; }
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((LanguageElement)element).Type;
        }
    }
}

#endif
