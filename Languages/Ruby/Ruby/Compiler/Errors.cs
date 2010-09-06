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

using System;
using Microsoft.Scripting.Utils;
using System.Diagnostics;

namespace IronRuby.Compiler {
    // TODO: tests
    public struct ErrorInfo {
        public readonly int Code;
        public readonly string/*!*/ ResourceId;

        public ErrorInfo(int code, string/*!*/ id) {
            Assert.NotNull(id);
            Code = code;
            ResourceId = id;
        }

        // TODO: load from resource
        public string/*!*/ GetMessage() {
            return ResourceId;
        }

        public string/*!*/ GetMessage(params object[] args) {
            return String.Format(GetMessage(), args);
        }
    }

    public static class Errors {
        private const int Tokenizer = 0x1000;

        public static readonly ErrorInfo IllegalOctalDigit           = new ErrorInfo(Tokenizer + 0, "Illegal octal digit");
        public static readonly ErrorInfo NumericLiteralWithoutDigits = new ErrorInfo(Tokenizer + 1, "Numeric literal without digits");
        public static readonly ErrorInfo TrailingUnderscoreInNumber  = new ErrorInfo(Tokenizer + 2, "Trailing `_' in number");
        public static readonly ErrorInfo TrailingEInNumber           = new ErrorInfo(Tokenizer + 3, "Trailing `e' in number");
        public static readonly ErrorInfo TrailingPlusInNumber        = new ErrorInfo(Tokenizer + 4, "Trailing `+' in number");
        public static readonly ErrorInfo TrailingMinusInNumber       = new ErrorInfo(Tokenizer + 5, "Trailing `-' in number");
        public static readonly ErrorInfo FloatOutOfRange             = new ErrorInfo(Tokenizer + 6, "Float {0} out of range");
        public static readonly ErrorInfo NoFloatingLiteral           = new ErrorInfo(Tokenizer + 7, "No .<digit> floating literal anymore; put 0 before dot");
        public static readonly ErrorInfo InvalidGlobalVariableName   = new ErrorInfo(Tokenizer + 8, "Identifier {0} is not valid");
        public static readonly ErrorInfo CannotAssignTo              = new ErrorInfo(Tokenizer + 9, "Can't assign to {0}");
        public static readonly ErrorInfo FormalArgumentIsConstantVariable = new ErrorInfo(Tokenizer + 10, "Formal argument cannot be a constant");
        public static readonly ErrorInfo FormalArgumentIsInstanceVariable = new ErrorInfo(Tokenizer + 11, "Formal argument cannot be an instance variable");
        public static readonly ErrorInfo FormalArgumentIsGlobalVariable   = new ErrorInfo(Tokenizer + 12, "Formal argument cannot be a global variable");
        public static readonly ErrorInfo FormalArgumentIsClassVariable    = new ErrorInfo(Tokenizer + 13, "Formal argument cannot be a class variable");
        public static readonly ErrorInfo InvalidInstanceVariableName      = new ErrorInfo(Tokenizer + 14, "`@{0}' is not allowed as an instance variable name");
        public static readonly ErrorInfo InvalidClassVariableName         = new ErrorInfo(Tokenizer + 15, "`@@{0}' is not allowed as a class variable name");
        public static readonly ErrorInfo InvalidCharacterInExpression     = new ErrorInfo(Tokenizer + 16, "Invalid character '{0}' in expression");
        public static readonly ErrorInfo InvalidMultibyteCharacter        = new ErrorInfo(Tokenizer + 17, "Invalid multibyte character: {0} ({1})");
        public static readonly ErrorInfo ForLoopVariableIsConstantVariable = new ErrorInfo(Tokenizer + 18, "For loop variable cannot be a constant");
        public static readonly ErrorInfo ForLoopVariableIsInstanceVariable = new ErrorInfo(Tokenizer + 19, "For loop variable cannot be an instance variable");
        public static readonly ErrorInfo ForLoopVariableIsGlobalVariable   = new ErrorInfo(Tokenizer + 20, "For loop variable cannot be a global variable");
        public static readonly ErrorInfo ForLoopVariableIsClassVariable    = new ErrorInfo(Tokenizer + 21, "For loop variable cannot be a class variable");
        
        public static readonly ErrorInfo MatchGroupReferenceOverflow = new ErrorInfo(Tokenizer + 30, "Match group reference ${0} doesn't fit into Fixnum");
        public static readonly ErrorInfo MatchGroupReferenceReadOnly = new ErrorInfo(Tokenizer + 31, "Can't set variable ${0}");
        public static readonly ErrorInfo CannotAliasGroupMatchVariable = new ErrorInfo(Tokenizer + 32, "Can't make alias for number variable");
        public static readonly ErrorInfo DuplicateParameterName = new ErrorInfo(Tokenizer + 33, "duplicate parameter name");

        public static readonly ErrorInfo IncompleteCharacter = new ErrorInfo(Tokenizer + 37, "Incomplete character syntax");
        public static readonly ErrorInfo UnterminatedEmbeddedDocument = new ErrorInfo(Tokenizer + 38, "Embedded document meets end of file");
        public static readonly ErrorInfo UnterminatedString = new ErrorInfo(Tokenizer + 39, "Unterminated string meets end of file");
        public static readonly ErrorInfo UnterminatedHereDocIdentifier = new ErrorInfo(Tokenizer + 41, "Unterminated here document identifier");
        public static readonly ErrorInfo UnterminatedHereDoc = new ErrorInfo(Tokenizer + 42, "can't find string \"{0}\" anywhere before end-of-file");
        public static readonly ErrorInfo FileInitializerInMethod = new ErrorInfo(Tokenizer + 43, "BEGIN in method");
        
        public static readonly ErrorInfo UnknownQuotedStringType = new ErrorInfo(Tokenizer + 50, "Unknown type of quoted string");
        public static readonly ErrorInfo UnknownRegexOption = new ErrorInfo(Tokenizer + 51, "Unknown RegEx option '{0}'");
        public static readonly ErrorInfo TooLargeUnicodeCodePoint = new ErrorInfo(Tokenizer + 52, "Invalid Unicode codepoint (too large)");
        public static readonly ErrorInfo InvalidEscapeCharacter = new ErrorInfo(Tokenizer + 53, "Invalid escape character syntax");
        public static readonly ErrorInfo EmptySymbolLiteral = new ErrorInfo(Tokenizer + 55, "empty symbol literal");
        public static readonly ErrorInfo EncodingsMixed = new ErrorInfo(Tokenizer + 56, "{0} mixed within {1} source");
        public static readonly ErrorInfo UntermintedUnicodeEscape = new ErrorInfo(Tokenizer + 57, "Unterminated Unicode escape");
        public static readonly ErrorInfo InvalidUnicodeEscape = new ErrorInfo(Tokenizer + 58, "Invalid Unicode escape");

        public static readonly ErrorInfo ModuleNameNotConstant = new ErrorInfo(Tokenizer + 66, "Class/module name must be a constant");
        public static readonly ErrorInfo ConstantReassigned = new ErrorInfo(Tokenizer + 67, "Constant re-assignment");
        public static readonly ErrorInfo BothBlockDefAndBlockRefGiven = new ErrorInfo(Tokenizer + 68, "both block arg and actual block given");
        public static readonly ErrorInfo BlockGivenToYield = new ErrorInfo(Tokenizer + 69, "block given to yield");
        

        // level 1 warnings:
        private const int WarningLevel1 = 0x2000;
        internal const int RuntimeWarning = WarningLevel1;
        public static readonly ErrorInfo ParenthesizeArguments = new ErrorInfo(WarningLevel1 + 1, "parenthesize argument(s) for future version");
        public static readonly ErrorInfo WhitespaceBeforeArgumentParentheses = new ErrorInfo(WarningLevel1 + 2, "don't put space before argument parentheses");
        public static readonly ErrorInfo InvalidCharacterSyntax = new ErrorInfo(WarningLevel1 + 3, "invalid character syntax; use ?\\{0}");
        public static readonly ErrorInfo ShutdownHandlerInMethod = new ErrorInfo(WarningLevel1 + 4, "END in method; use at_exit");    
        
        // level 2 warnings:
        private const int WarningLevel2 = 0x3000;
        internal const int RuntimeVerboseWarning = WarningLevel2;
        public static readonly ErrorInfo InterpretedAsGroupedExpression = new ErrorInfo(WarningLevel2 + 1, "(...) interpreted as grouped expression");
        public static readonly ErrorInfo AmbiguousFirstArgument = new ErrorInfo(WarningLevel2 + 2, "Ambiguous first argument; put parentheses or even spaces");
        public static readonly ErrorInfo AmpersandInterpretedAsProcArgument = new ErrorInfo(WarningLevel2 + 3, "`&' interpreted as argument prefix"); // TODO: level 1?
        public static readonly ErrorInfo AmpersandInVoidContext = new ErrorInfo(WarningLevel2 + 4, "Useless use of & in void context"); // TODO: level 1?
        public static readonly ErrorInfo StarInterpretedAsSplatArgument = new ErrorInfo(WarningLevel2 + 5, "`*' interpreted as argument prefix"); // TODO: level 1?
        public static readonly ErrorInfo ShadowingOuterLocalVariable = new ErrorInfo(WarningLevel2 + 7, "shadowing outer local variable - {0}.");
        
        internal static bool IsVerboseWarning(int errorCode) {
            return errorCode >= WarningLevel2;
        }
    }
}
