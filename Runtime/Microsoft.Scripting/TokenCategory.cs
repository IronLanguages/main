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

namespace Microsoft.Scripting {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum TokenCategory {
        None,

        /// <summary>
        /// A token marking an end of stream.
        /// </summary>
        EndOfStream,

        /// <summary>
        /// A space, tab, or newline.
        /// </summary>
        WhiteSpace,

        /// <summary>
        /// A block comment.
        /// </summary>
        Comment,

        /// <summary>
        /// A single line comment.
        /// </summary>
        LineComment,

        /// <summary>
        /// A documentation comment.
        /// </summary>
        DocComment,

        /// <summary>
        /// A numeric literal.
        /// </summary>
        NumericLiteral,

        /// <summary>
        /// A character literal.
        /// </summary>
        CharacterLiteral,
        
        /// <summary>
        /// A string literal.
        /// </summary>
        StringLiteral,

        /// <summary>
        /// A regular expression literal.
        /// </summary>
        RegularExpressionLiteral,

        /// <summary>
        /// A keyword.
        /// </summary>
        Keyword,
        
        /// <summary>
        /// A directive (e.g. #line).
        /// </summary>
        Directive,
        
        /// <summary>
        /// A punctuation character that has a specific meaning in a language.
        /// </summary>
        Operator,

        /// <summary>
        /// A token that operates as a separator between two language elements.
        /// </summary>
        Delimiter,

        /// <summary>
        /// An identifier (variable, $variable, @variable, @@variable, $variable$, function!, function?, [variable], i'variable', ...)
        /// </summary>
        Identifier,
        
        /// <summary>
        /// Braces, parenthesis, brackets.
        /// </summary>
        Grouping,

        /// <summary>
        /// Errors.
        /// </summary>
        Error,

        LanguageDefined = 0x100
    }

    // not currently used, just for info
    public enum TokenKind {
        Default,

        // Errors:
        Error,

        // Whitespace:
        Whitespace,
        EndOfLine,
        LineJoin,               // Python: \<eoln>
        Indentation,

        // Comments:
        SingleLineComment,      // #..., //..., '...
        MultiLineComment,       // /* ... */
        NestableCommentStart,   // Lua: --[[
        NestableCommentEnd,     // ]]

        // DocComments:
        SingleLineDocComment,   // 
        MultiLineDocComment,    // Ruby: =begin =end PHP: /** */

        // Directives:
        Directive,              // #line, etc.

        // Keywords:
        Keyword,

        // Identifiers:
        Identifier,             // identifier
        VerbatimIdentifier,     // PHP/CLR: i'...', 
        Variable,               // Ruby: @identifier, @@identifier; PHP, Ruby: $identifier, 

        // Numbers:
        IntegerLiteral,
        FloatLiteral,

        // Characters:
        CharacterLiteral,

        // Strings:
        String,
        UnicodeString,
        FormattedString,
        FormattedUnicodeString,

        // Groupings:
        LeftParenthesis,        // (
        RightParenthesis,       // )
        LeftBracket,            // [
        RightBracket,           // ]
        LeftBrace,              // {
        RightBrace,             // }

        // Delimiters:
        Comma,                  // ,
        Dot,                    // .
        Semicolon,              // ;
        Colon,                  // :
        DoubleColon,            // :: 
        TripleColon,            // PHP/CLR: ::: 
        
        // Operators:
        Plus,                   // +
        PlusPlus,               // ++
        PlusEqual,              // +=
        Minus,                  // -
        MinusMinus,             // --
        MinusEqual,             // -=
        Mul,                    // *
        MulEqual,               // *=
        Div,                    // /
        DivEqual,               // /=
        FloorDivide,            // //
        FloorDivideEqual,       // //=
        Mod,                    // %
        ModEqual,               // %=
        Power,                  // Python: **
        PowerEqual,             // Python, Ruby: **=
        LeftShift,              // <<
        LeftShiftEqual,         // <<= 
        RightShift,             // >>
        RightShiftEqual,        // >>=
        BitwiseAnd,             // &
        BitwiseAndEqual,        // &=
        BitwiseOr,              // |
        BitwiseOrEqual,         // |=
        Xor,                    // ^
        XorEqual,               // ^=
        BooleanAnd,             // &&
        BooleanAndEqual,        // Ruby: &&=
        BooleanOr,              // ||
        BooleanOrEqual,         // Ruby: ||=
        Twiddle,                // ~
        TwiddleEqual,           // ~=
        LessThan,               // <
        GreaterThan,            // >
        LessThanOrEqual,        // <=
        GreaterThanOrEqual,     // >=
        Assign,                 // =
        AssignAlias,            // PHP: =&
        AssignColon,            // :=
        Equal,                  // == 
        StrictEqual,            // ===
        Not,                    // !
        NotEqual,               // !=
        StrictNotEqual,         // !==
        Unequal,                // <>         
        CompareEqual,           // Ruby: <=>
        Match,                  // =~
        NotMatch,               // !~
        Arrow,                  // PHP: ->
        DoubleArrow,            // PHP, Ruby: =>
        BackQuote,              // `
        DoubleDot,              // Ruby: ..
        TripleDot,              // Ruby: ...
        At,                     // @
        DoubleAt,               // @@
        Question,               // ?
        DoubleQuestion,         // ??
        Backslash,              // \
        DoubleBackslash,        // \\
        Dollar,                 // $
        DoubleDollar,           // $$

        LanguageDefined,
    }
}