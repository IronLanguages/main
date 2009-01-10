/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if NEW_PARSER
using System;
using System.Collections.Generic;
using System.Text;

// true operators: .. ... ! not && and || or != !~ += *= /= %= ^= etc.

namespace Ruby.Compiler {
    public enum Tokens {
        None,
        Error,
        WhiteSpace,
        EndOfFile,
        UnexpectedCharacter,

        SingleLineComment,
        MultiLineComment,

        // keywords:
        CLASS,MODULE,DEF,UNDEF,BEGIN,RESCUE,ENSURE,END,IF,UNLESS,THEN,ELSIF,ELSE,
        CASE,WHEN,WHILE,UNTIL,FOR,BREAK,NEXT,REDO,RETRY,IN,DO,LOOP_DO,BLOCK_DO,
        RETURN,YIELD,SUPER,SELF,NIL,TRUE,FALSE,AND,OR,NOT,IF_MOD,UNLESS_MOD,
        WHILE_MOD,UNTIL_MOD,RESCUE_MOD,ALIAS,DEFINED,UPPERCASE_BEGIN,UPPERCASE_END,__LINE__,__FILE__,

        // % token<id:string>
        CONSTANT_IDENTIFIER,   // CONSTANT_IDENTIFIER:            [A-Z][_a-zA-Z0-9]*                         
        IDENTIFIER, // IDENTIFIER:          [_a-z][_a-zA-Z0-9]*                        
        FUNCTION_IDENTIFIER,        // FUNCTION-IDENTIFIER: [_a-zA-Z][_a-zA-Z0-9]*[!?=]?               
        
        GVAR,       
        // GLOBAL-VARIABLE:
        // [$][_a-zA-Z][_a-zA-Z0-9]* 
        // [$][~`_\\>=<;:/.+,-*'&$"!]
        // [$]-[0-9_a-zA-Z]
        
        IVAR,       // INSTANCE-VARIABLE:   @[_a-zA-Z][_a-zA-Z0-9]*                  
        CVAR,       // CLASS-VARIABLE:      @@[_a-zA-Z][_a-zA-Z0-9]*                 
        
        // % token<id:string>
        OP_ASGN,    // =, !=, %=, &=, *=, +=, -=, /=, ^=, |=, ||=, &&=, <<=, >>=, **=

        INTEGER,              
        // [1-9]([0-9_]*[1-9])?
        // 0([0-7_]*[0-7])?
        // 0[xX][0-9a-fA-F]([0-9a-fA-F_]*[0-9a-fA-F])?
        // 0[dD][0-9]([0-9_]*[0-9])?
        // 0[bB][01]([01_]*[01])?
        // 0[oO][0-7]([0-7_]*[0-7])?
        // [?]{char}
        
        // {char} is
        //   (\\(c|C-|M-))*([^\\]|\\[abefnrstv]|\\[0-7]{1,3}|\\x[0-9a-fA-F]{1-2})
        //
        //   ... \c, \C- mean & 0x9f, \M- means | 0x80
        //   ... [abefnrstv] are escaped special characters
        //   ... \000 octal
        //   ... \xFF hex
        //
        // [-2**30, 2*30-1] -> Fixnum -- define CLR as 32bit platform?
        //        otherwise -> Bignum

        FLOAT,
        // (0|[1-9]([0-9_]*[0-9])?)[.][0-9_]*[0-9]([eE][+-]?[0-9]([0-9_]*[0-9])?)
        
        NTH_REF,                // [$][0-9]+
        BACK_REF,               // [$][`'+&]

        STRING_BEG,             // ["']|%[qQ]?[:open:]
        STRING_END,             // ["']|[:close:]

        STRING_EMBEDDED_CODE_BEGIN,            // #          in string
        STRING_EMBEDDED_VARIABLE_BEGIN,            // #[{]       in string 
        
        STRING_CONTENT,         // string content w/o #
        
        SHELL_STRING_BEGIN,            // `|%x[:open:]
        
        REGEXP_BEG,             // (%r[:open:]|[/])
        REGEXP_END,             // ([:close:]|[/])[eimnosux]* 

        SYMBEG,                 // [:]["]?

        WORDS_BEG,              // %W[:open:]
        VERBATIM_WORDS_BEGIN,             // %w[:open:]

        AREF,                   // []  (?)
        ASET,                   // []= (?)
        ASSOC,                  // =>
        
        AMPER,                  // &
        CMP,                    // <=>
        EQ,                     // ==  (?)
        EQQ,                    // === (?)
        MATCH,                  // ~=
		GreaterThen,             // >
		LessThen,                // <
		LEQ,                    // <=
		LSHFT,                  // <<
		RSHFT,                  // >>

		UPLUS,                  // +@ (unary plus method on Numeric class)

        UMINUS,                 // -@ (unary minus method on Numeric class)
        UMINUS_NUM,             // - (unary minus ???)

        POW,                    // **
        NEQ,                    // !=
        GEQ,                    // >=
        ANDOP,                  // &&
        OROP,                   // ||
        NMATCH,                 // !~
		Mul,                     // *
		STAR,		             // * in arguments
		Div,                     // /
		Mod,                     // %
		Twiddle,                 // ~
		BackQuote,               // `
        Comma,                   // ,
        DOT2,                   // ..
        DOT3,                   // ...
        SEPARATING_DOUBLE_COLON,                 // ::
        LEADING_DOUBLE_COLON,                 // :::

        LBRACK,                 // [

        LBRACE,                 // {
        LBRACE_ARG,             // ' {'
        
        LPAREN,                 // (
        LPAREN_ARG,             // ' (' 
    }
}
#endif