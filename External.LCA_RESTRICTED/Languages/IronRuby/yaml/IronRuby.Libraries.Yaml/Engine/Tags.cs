/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace IronRuby.StandardLibrary.Yaml {

    public static class Tags {
        public const string Prefix = "tag:yaml.org,2002:";

        public const string Map = Prefix + "map";
        public const string Str = Prefix + "str";
        public const string Seq = Prefix + "seq";
        public const string Float = Prefix + "float";
        public const string Null = Prefix + "null";
        public const string Bool = Prefix + "bool";
        public const string True = Bool + "#yes";
        public const string False = Bool + "#no";
        public const string Timestamp = Prefix + "timestamp";
        public const string TimestampYmd = Prefix + "timestamp#ymd";
        public const string Int = Prefix + "int";
        public const string Bignum = Int + ":Bignum";
        public const string Fixnum = Int + ":Fixnum";
        public const string Binary = Prefix + "binary";

        public const string RubyPrefix = "tag:ruby.yaml.org,2002:";
        public const string RubySymbol = RubyPrefix + "sym";
        public const string RubyRegexp = RubyPrefix + "regexp";
        public const string RubyRange = RubyPrefix + "range";
        public const string RubyObject = RubyPrefix + "object";
        public const string RubyException = RubyPrefix + "exception";
    }
}
