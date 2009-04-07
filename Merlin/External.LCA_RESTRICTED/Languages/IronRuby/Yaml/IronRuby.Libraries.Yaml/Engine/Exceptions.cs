/***** BEGIN LICENSE BLOCK *****
 * Version: CPL 1.0
 *
 * The contents of this file are subject to the Common Public
 * License Version 1.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.eclipse.org/legal/cpl-v10.html
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * Copyright (c) Microsoft Corporation.
 * 
 ***** END LICENSE BLOCK *****/

using System;

namespace IronRuby.StandardLibrary.Yaml {

    public class YamlException : Exception {
        public YamlException(string message)
            : base(message) {
        }

        public YamlException(string message, Exception inner)
            : base(message, inner) {
        }
    }

    public class ScannerException : YamlException {
        public ScannerException(string message)
            : base(message) {
        }

        public ScannerException(string message, Exception inner)
            : base(message, inner) {
        }
    }

    public class ParserException : YamlException {
        public ParserException(string message)
            : base(message) {
        }

        public ParserException(string message, Exception inner)
            : base(message, inner) {
        }
    }

    public class ComposerException : YamlException {
        public ComposerException(string message)
            : base(message) {
        }

        public ComposerException(string message, Exception inner)
            : base(message, inner) {
        }
    }

    public class ResolverException : YamlException {
        public ResolverException(string message)
            : base(message) {
        }

        public ResolverException(string message, Exception inner)
            : base(message, inner) {
        }
    }

    public class ConstructorException : YamlException {
        public ConstructorException(string message)
            : base(message) {
        }

        public ConstructorException(string message, Exception inner)
            : base(message, inner) {
        }
    }

    public class RepresenterException : YamlException {
        public RepresenterException(string message)
            : base(message) {
        }

        public RepresenterException(string message, Exception inner)
            : base(message, inner) {
        }
    }

    public class SerializerException : YamlException {
        public SerializerException(string message)
            : base(message) {
        }

        public SerializerException(string message, Exception inner)
            : base(message, inner) {
        }
    }


    public class EmitterException : YamlException {
        public EmitterException(string message)
            : base(message) {
        }

        public EmitterException(string message, Exception inner)
            : base(message, inner) {
        }
    }
}
