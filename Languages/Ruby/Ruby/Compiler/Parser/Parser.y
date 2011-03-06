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

using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Ast;
            
%%

%namespace IronRuby.Compiler

%union { } 

%SymbolLocationType SourceSpan
%SymbolValueType TokenValue 

%partial  
%visibility public

%token SINGLE_LINE_COMMENT MULTI_LINE_COMMENT WHITESPACE END_OF_LINE INVALID_CHARACTER POUND AT DOLLAR BACKSLASH 
%token WORD_SEPARATOR NEW_LINE
%token CLASS MODULE DEF UNDEF BEGIN RESCUE ENSURE END IF UNLESS THEN ELSIF ELSE
%token CASE WHEN WHILE UNTIL FOR BREAK NEXT REDO RETRY IN DO LOOP_DO BLOCK_DO LAMBDA_DO
%token RETURN YIELD SUPER SELF NIL TRUE FALSE AND OR NOT IF_MOD UNLESS_MOD
%token WHILE_MOD UNTIL_MOD RESCUE_MOD ALIAS DEFINED UPPERCASE_BEGIN UPPERCASE_END LINE FILE ENCODING
%token UNARY_PLUS UNARY_MINUS POW CMP EQUAL STRICT_EQUAL NOT_EQUAL GREATER_OR_EQUAL LESS_OR_EQUAL LOGICAL_AND LOGICAL_OR MATCH NMATCH
%token DOUBLE_DOT TRIPLE_DOT ITEM_GETTER ITEM_SETTER LSHFT RSHFT SEPARATING_DOUBLE_COLON LEADING_DOUBLE_COLON DOUBLE_ARROW
%token SEMICOLON COMMA DOT STAR BLOCK_REFERENCE AMPERSAND BACKTICK 
%token LAMBDA
%token LEFT_PARENTHESIS LEFT_EXPR_PARENTHESIS LEFT_ARG_PARENTHESIS RIGHT_PARENTHESIS
%token LEFT_BRACE LEFT_BLOCK_BRACE LEFT_BLOCK_ARG_BRACE LEFT_LAMBDA_BRACE RIGHT_BRACE
%token LEFT_BRACKET LEFT_INDEXING_BRACKET RIGHT_BRACKET
%token STRING_EMBEDDED_VARIABLE_BEGIN STRING_EMBEDDED_CODE_BEGIN STRING_EMBEDDED_CODE_END 
%token VERBATIM_HEREDOC_BEGIN VERBATIM_HEREDOC_END
%token STRING_BEGIN REGEXP_BEGIN SHELL_STRING_BEGIN WORDS_BEGIN VERBATIM_WORDS_BEGIN SYMBOL_BEGIN STRING_END

%token<String> IDENTIFIER FUNCTION_IDENTIFIER GLOBAL_VARIABLE INSTANCE_VARIABLE CONSTANT_IDENTIFIER CLASS_VARIABLE OP_ASSIGNMENT 
%token<String> LABEL                               // 1.9  
%token CHARACTER STRING_CONTENT                    // (String & Encoding)
%token<Integer1> INTEGER 
%token<BigInteger> BIG_INTEGER
%token<Double> FLOAT 
%token<Integer1> MATCH_REFERENCE
%token<RegExOptions>  REGEXP_END 

%type<AbstractSyntaxTree> program

%type<Expression> stmt
%type<Statements> stmts compstmt lambda_body ensure_opt
%type<JumpStatement> jump_statement jump_statement_with_parameters jump_statement_parameterless
%type<Expression> alias_statement
%type<Expression> conditional_statement

%type<Expression> primary expr expression_statement superclass var_ref singleton case_expression
%type<Expression> arg                              
%type<ArgumentCount> args compound_rhs             // arguments pushed to the argument stack
%type<Expression> block_expression                 // (Expression | BlockExpression | Body)
%type<Expression> definition_expression            // DefinitionExpression
%type<Body> body
%type<Expression> 

%type<CallExpression> method_call block_call command_call block_command command

%type<ElseIfClause> else_opt 
%type<ElseIfClauses> if_tail
%type<ConstructedSymbols> undef_list                                     
%type<BlockReference> block_reference block_reference_opt 
%type<BlockDefinition> cmd_brace_block brace_block do_block 
%type<LambdaDefinition> lambda 

%type<Arguments> array_items                                       // (Arguments! & Block == null)
%type parenthesized_args                                           // (Arguments! & Block?)
%type parenthesized_args_opt call_args call_args_opt command_args  // (Arguments? & Block?)

%type<ConstantVariable> qualified_module_name 

%type<RescueClauses> rescue_clauses rescue_clauses_opt
%type<RescueClause> rescue_clause

%type<WhenClauses> when_clauses 
%type<WhenClause> when_clause                          
 
%type<Maplets> maplets 
%type<Maplet> maplet 

%type<Expression> string_embedded_variable                    // string embedded variable
%type<Expression> string_content                              // string piece - literal, string embedded code/variable
%type<Expressions> string_contents                            // list of string pieces
%type<Expressions> string                                     // quoted string constructor taking list of string pieces "#{foo}bar#{baz}"
%type<Expressions> string_concatenation                       // string constructor taking a list of quoted strings "foo" 'bar' "baz"
%type<Expression> shell_string                                // shell string constructor taking list of string pieces `#{foo}bar#{baz}`
%type<Expressions> word                                       // concatenation of string pieces
%type<Expressions> word_list verbatim_word_list               // list of words separated by space
%type<Expression> words verbatim_words                        // array constructor taking a list of words

%type<Expression> regexp 
%type<Expression> numeric_literal 
%type<ConstructedSymbol> symbol method_name_or_symbol         
%type<RegexMatchReference> match_reference 

%type<String> operation variable sym operation2 operation3 module_name op method_name

%type<Parameters> block_parameters block_parameters_opt lambda_parameters  // Parameters?
%type<Parameters> method_parameters block_parameter_list parameters
%type block_variables block_variables_opt                   // 1.9

%type<SimpleAssignmentExpressions> default_parameter_list default_block_parameter_list
%type<SimpleAssignmentExpression> default_parameter default_block_parameter

%type<CompoundLeftValue> for_parameters                        

%type<LocalVariable> parameter parameter_array block_parameter block_parameter_opt

%type<CompoundLeftValue> compound_parameters                // 1.9
%type<LeftValues> parameter_list 
%type<LeftValues> compound_parameter_list                   // 1.9
%type<LeftValue> compound_parameter parenthesized_parameter // 1.9

%type<CompoundLeftValue> compound_lhs
%type<LeftValues> compound_lhs_nodes
%type<LeftValues> compound_lhs_tail                         // 1.9
%type<LeftValue> compound_lhs_node                          
%type<LeftValue> compound_lhs_leaf                          // 1.9

%type<LeftValue> var_lhs 
%type<LeftValue> lhs 
%type<LeftValue> exc_var

%nonassoc LOWEST
%nonassoc LEFT_BLOCK_ARG_BRACE
%nonassoc  IF_MOD UNLESS_MOD WHILE_MOD UNTIL_MOD
%left  OR AND
%right NOT
%nonassoc DEFINED
%right ASSIGNMENT OP_ASSIGNMENT
%left RESCUE_MOD
%right QUESTION_MARK COLON
%nonassoc DOUBLE_DOT TRIPLE_DOT
%left  LOGICAL_OR
%left  LOGICAL_AND
%nonassoc  CMP EQUAL STRICT_EQUAL NOT_EQUAL MATCH NMATCH
%left  GREATER GREATER_OR_EQUAL LESS LESS_OR_EQUAL
%left  PIPE CARET
%left  AMPERSAND
%left  LSHFT RSHFT
%left  PLUS MINUS
%left  ASTERISK SLASH PERCENT
%right NUMBER_NEGATION UNARY_MINUS
%right POW
%right BANG TILDE UNARY_PLUS

%token LAST_TOKEN


%%


program:
      compstmt
        {
            _ast = new SourceUnitTree(CurrentScope, $1, _initializers, Encoding, _tokenizer.DataOffset);
        }
;

compstmt:
      terms_opt
        {
            $$ = Statements.Empty; 
        } 
    | terms stmts terms_opt
        {
            $$ = $2; 
        } 
    | stmts terms_opt
        {
            $$ = $1; 
        }
;

stmts: 
      stmt 
        {
            $$ = new Statements($1);
        }
    | stmts terms stmt
        {
            ($$ = $1).Add($3);
        }
    | ERROR stmt
        {
            $$ = new Statements($2);
        }
;

stmt:
       alias_statement
     | UNDEF undef_list
         {
             $$ = new UndefineStatement($2, @$);
         }
     | UPPERCASE_BEGIN           // TODO: 1.9 move to top_stmt
         {
             if (InMethod) {
                 _tokenizer.ReportError(Errors.FileInitializerInMethod);
             }
                         
             EnterFileInitializerScope();
         }
       LEFT_BLOCK_BRACE compstmt RIGHT_BRACE
         {
             $$ = AddInitializer(new FileInitializerStatement(CurrentScope, $4, @$));
             LeaveScope();
         }
     | UPPERCASE_END
         {
             if (InMethod) {
                 _tokenizer.ReportWarning(Errors.ShutdownHandlerInMethod);
             }
             
             // END block behaves like a block definition (allows variable closures, super, etc):
             EnterNestedScope();
         } 
       LEFT_BLOCK_BRACE compstmt RIGHT_BRACE
         {                    
             $$ = new ShutdownHandlerStatement(CurrentScope, $4, @$);
             LeaveScope();
         }        
     | match_reference OP_ASSIGNMENT command_call
         {
             MatchReferenceReadOnlyError($1);
             $$ = new ErrorExpression(@$);
         } 
     | conditional_statement
         {
             $$ = $1;
         }
     | expression_statement
         {
             $$ = $1;
         }
;

alias_statement:
      ALIAS method_name_or_symbol 
        {
            _tokenizer.LexicalState = LexicalState.EXPR_FNAME;
        } 
      method_name_or_symbol
        {
            $$ = new AliasStatement(true, $2, $4, @$);
        }
    | ALIAS GLOBAL_VARIABLE GLOBAL_VARIABLE
        {
            $$ = MakeGlobalAlias($2, $3, @$);
        }
    | ALIAS GLOBAL_VARIABLE match_reference
        {
            $$ = MakeGlobalAlias($2, $3, @$);
        }
;

jump_statement:
      jump_statement_with_parameters
        {
            $$ = $1;
        }
    | jump_statement_parameterless
        {
            $$ = $1;
        }
;

jump_statement_with_parameters:
      RETURN call_args
        {
            $$ = new ReturnStatement(RequireNoBlockArg($2), @$);
        }
    | BREAK call_args
        {
            $$ = new BreakStatement(RequireNoBlockArg($2), @$);
        }
    | NEXT call_args
        {
            $$ = new NextStatement(RequireNoBlockArg($2), @$);
        }
;

jump_statement_parameterless:
      RETURN
        {
            $$ = new ReturnStatement(null, @$);
        }
    | BREAK
        {
            $$ = new BreakStatement(null, @$);
        }
    | NEXT
        {
            $$ = new NextStatement(null, @$);
        }
    | REDO
        {
            $$ = new RedoStatement(@$);
        }
    | RETRY
        {
            $$ = new RetryStatement(@$);
        }
;

expression_statement: 
      expr 
        {
            $$ = $1;
        } 
    | jump_statement                       
        {
            $$ = $1;
        }
    | lhs ASSIGNMENT command_call
        {
            $$ = new SimpleAssignmentExpression($1, $3, null, @$);
        }
    | compound_lhs ASSIGNMENT command_call
        {
            $$ = new ParallelAssignmentExpression($1, new Expression[] { $3 }, @$);
        }
    | var_lhs OP_ASSIGNMENT command_call
        {
            $$ = new SimpleAssignmentExpression($1, $3, $2, @$);
        }
    | primary LEFT_INDEXING_BRACKET call_args_opt closing_bracket OP_ASSIGNMENT command_call      // 1.9 change
        {                
            $$ = new SimpleAssignmentExpression(MakeArrayItemAccess($1, $3, @2), $6, $5, @$);
        }
    | primary DOT IDENTIFIER OP_ASSIGNMENT command_call
        {
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | primary DOT CONSTANT_IDENTIFIER OP_ASSIGNMENT command_call
        {
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | primary SEPARATING_DOUBLE_COLON CONSTANT_IDENTIFIER OP_ASSIGNMENT command_call
        {
            _tokenizer.ReportError(Errors.ConstantReassigned);
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | primary SEPARATING_DOUBLE_COLON IDENTIFIER OP_ASSIGNMENT command_call
        {
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | lhs ASSIGNMENT compound_rhs
        {
            $$ = new ParallelAssignmentExpression(new CompoundLeftValue(new LeftValue[] { $1 }), PopArguments($3), @$);
        }
    | compound_lhs ASSIGNMENT arg
        {
            $$ = new ParallelAssignmentExpression($1, new Expression[] { $3 }, @$);
        }
    | compound_lhs ASSIGNMENT compound_rhs
        {
            $$ = new ParallelAssignmentExpression($1, PopArguments($3), @$);
        }
;

conditional_statement:
      stmt IF_MOD expr
        {
            $$ = new ConditionalStatement(ToCondition($3), false, $1, null, @$);
        }
    | stmt UNLESS_MOD expr
        {
            $$ = new ConditionalStatement(ToCondition($3), true, $1, null, @$);
        }
    | stmt WHILE_MOD expr
        {
            $$ = MakeLoopStatement($1, ToCondition($3), true, @$);
        }
    | stmt UNTIL_MOD expr
        {
            $$ = MakeLoopStatement($1, ToCondition($3), false, @$);
        }
    | stmt RESCUE_MOD stmt
        {
            $$ = new RescueExpression($1, $3, MergeLocations(@2, @3), @$);
        }
;

expr: 
      command_call
    | expr AND expr
        {
            $$ = new AndExpression($1, $3, @$);
        }
    | expr OR expr
        {
            $$ = new OrExpression($1, $3, @$);
        }
    | expr AND jump_statement
        {
            $$ = new ConditionalJumpExpression($1, $3, false, null, @$);
        }
    | expr OR jump_statement
        {
            $$ = new ConditionalJumpExpression($1, $3, true, null, @$);
        }
    | NOT new_line_opt expr                                      // 1.9 added new_line_opt
        {
            $$ = new NotExpression($3, @$);
        }
    | BANG command_call
        {
            $$ = new NotExpression($2, @$);
        }
    | arg
;

command_call:
      command
        {
            $$ = $1;
        }
    | block_command
        {
            $$ = $1;
        }
;

block_command: 
      block_call
        {
            $$ = $1;
        }
    | block_call DOT operation2 command_args
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
    | block_call SEPARATING_DOUBLE_COLON operation2 command_args
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
;

cmd_brace_block:
      LEFT_BLOCK_ARG_BRACE
        {
            EnterNestedScope();
        }
      block_parameters_opt compstmt RIGHT_BRACE
        {
            $$ = MakeBlockDefinition($3, $4, @$);
            LeaveScope();
        }
;

command:  
      operation command_args                                             %prec LOWEST
        {
            $$ = MakeMethodCall(null, $1, $2, @$);
        }
    | operation command_args cmd_brace_block
        {
            $$ = MakeMethodCall(null, $1, $2, $3, @$);
        }
    | primary DOT operation2 command_args                                %prec LOWEST
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
    | primary DOT operation2 command_args cmd_brace_block
        {
            $$ = MakeMethodCall($1, $3, $4, $5, @$);
        }
    | primary SEPARATING_DOUBLE_COLON operation2 command_args            %prec LOWEST
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
    | primary SEPARATING_DOUBLE_COLON operation2 command_args cmd_brace_block
        {
            $$ = MakeMethodCall($1, $3, $4, $5, @$);
        }
    | SUPER command_args
        {
            $$ = MakeSuperCall($2, @$);
        }
    | YIELD command_args
        {
            $$ = new YieldCall(RequireNoBlockArg($2), @$);
        }
;

compound_lhs:                                                           // 1.9 change
      LEFT_EXPR_PARENTHESIS compound_lhs closing_parenthesis
        {
            $$ = new CompoundLeftValue(new LeftValue[] { $2 });
        }
    | compound_lhs_nodes 
        {
            // the list ends with COMMA:
            $1.Add(Placeholder.Singleton);
            $$ = new CompoundLeftValue($1.ToArray());
        }
    | compound_lhs_nodes compound_lhs_node
        {
            $1.Add($2);
            $$ = new CompoundLeftValue($1.ToArray());
        }
    | compound_lhs_nodes STAR compound_lhs_leaf
        {
            $$ = MakeCompoundLeftValue($1, $3, null);
        }
    | compound_lhs_nodes STAR compound_lhs_leaf COMMA compound_lhs_tail
        {
            $$ = MakeCompoundLeftValue($1, $3, $5);
        }
    | compound_lhs_nodes STAR
        {
            $$ = MakeCompoundLeftValue($1, Placeholder.Singleton, null);
        }
    | compound_lhs_nodes STAR COMMA compound_lhs_tail
        {
            $$ = MakeCompoundLeftValue($1, Placeholder.Singleton, $4);
        }
    | STAR compound_lhs_leaf COMMA compound_lhs_tail
        {
            $$ = MakeCompoundLeftValue(null, $2, $4);
        }
    | STAR COMMA compound_lhs_tail
        {
            $$ = MakeCompoundLeftValue(null, Placeholder.Singleton, $3);
        }
    | STAR compound_lhs_leaf
        {
            $$ = new CompoundLeftValue(new LeftValue[] { $2 }, 0);
        }
    | STAR
        {
            $$ = new CompoundLeftValue(new LeftValue[] { Placeholder.Singleton }, 0);
        }
;

compound_lhs_nodes:
      compound_lhs_nodes compound_lhs_node COMMA
        {
            ($$ = $1).Add($2);
        }
    | compound_lhs_node COMMA
        {
            $$ = CollectionUtils.MakeList($1);
        }
;

compound_lhs_tail:                                                // 1.9
      compound_lhs_tail COMMA compound_lhs_node
        {
            ($$ = $1).Add($3);
        }
     | compound_lhs_node
        {
            $$ = CollectionUtils.MakeList($1);
        }
;

compound_lhs_node:
      compound_lhs_leaf
        {
            $$ = $1;
        }
    | LEFT_EXPR_PARENTHESIS compound_lhs closing_parenthesis     // 1.9 change
        {
            $$ = $2;
        }
;

compound_lhs_leaf:                                               // 1.9, the same as lhs
      variable
        {
            $$ = VariableFactory.MakeLeftValue($<VariableFactory>1, this, $<String>1, @$);
        }
    | primary LEFT_INDEXING_BRACKET call_args_opt closing_bracket        
        {
            $$ = MakeArrayItemAccess($1, $3, @$);
        }
    | primary DOT IDENTIFIER
        {
            $$ = new AttributeAccess($1, $3, @$);
        }
    | primary SEPARATING_DOUBLE_COLON IDENTIFIER
        {
            $$ = new AttributeAccess($1, $3, @$);
        }
    | primary DOT CONSTANT_IDENTIFIER
        {
            $$ = new AttributeAccess($1, $3, @$);
        }
    | primary SEPARATING_DOUBLE_COLON CONSTANT_IDENTIFIER
        {
            $$ = new ConstantVariable($1, $3, @$);
        }
    | LEADING_DOUBLE_COLON CONSTANT_IDENTIFIER
        {
            $$ = new ConstantVariable(null, $2, @$);
        }
    | match_reference
        {
            MatchReferenceReadOnlyError($1);
            $$ = new GlobalVariable(Symbols.Error, @$);
        }
;

lhs: variable
        {
            $$ = VariableFactory.MakeLeftValue($<VariableFactory>1, this, $<String>1, @$);
        }
    | primary LEFT_INDEXING_BRACKET call_args_opt closing_bracket             // 1.9 change
        {
            $$ = MakeArrayItemAccess($1, $3, @$);
        }
    | primary DOT IDENTIFIER
        {
            $$ = new AttributeAccess($1, $3, @$);
        }
    | primary SEPARATING_DOUBLE_COLON IDENTIFIER
        {
            $$ = new AttributeAccess($1, $3, @$);
        }
    | primary DOT CONSTANT_IDENTIFIER
        {
            $$ = new AttributeAccess($1, $3, @$);
        }
    | primary SEPARATING_DOUBLE_COLON CONSTANT_IDENTIFIER
        {
            $$ = new ConstantVariable($1, $3, @$);
        }
    | LEADING_DOUBLE_COLON CONSTANT_IDENTIFIER
        {
            $$ = new ConstantVariable(null, $2, @$);
        }
    | match_reference
        {
            MatchReferenceReadOnlyError($1);
            $$ = new GlobalVariable(Symbols.Error, @$);
        }
    ;

module_name: 
      CONSTANT_IDENTIFIER
      {
          $$ = $1;
      }
    | IDENTIFIER
      {
          _tokenizer.ReportError(Errors.ModuleNameNotConstant);
          $$ = $1;
      }
;

qualified_module_name: 
      LEADING_DOUBLE_COLON module_name
      {
          $$ = new ConstantVariable(null, $2, @$);
      }
    | module_name
      {
          $$ = new ConstantVariable($1, @$);
      }
    | primary SEPARATING_DOUBLE_COLON module_name
      {
          $$ = new ConstantVariable($1, $3, @$);
      }
;

method_name:
      IDENTIFIER
        {
            $$ = $1;    
        }
    | CONSTANT_IDENTIFIER
        {
            $$ = $1;    
        }        
    | FUNCTION_IDENTIFIER
        {
            $$ = $1;    
        }        
    | op
        {
            _tokenizer.LexicalState = LexicalState.EXPR_ENDFN; // 1.8: EXPR_END
            $$ = $1;
        }
    | reswords
        {
            _tokenizer.LexicalState = LexicalState.EXPR_ENDFN; // 1.8: EXPR_END
            $$ = $<String>1;
    }
;

method_name_or_symbol: 
      method_name
        {
            $$ = new ConstructedSymbol($1);
        }
    | symbol
        {
            $$ = $1;
        }
;

symbol:
      SYMBOL_BEGIN sym
        {
            _tokenizer.LexicalState = LexicalState.EXPR_END;
            $$ = new ConstructedSymbol($2);
        }
    | SYMBOL_BEGIN string_contents STRING_END
        {
            $$ = new ConstructedSymbol(MakeSymbolConstructor($2, @$));
        }
;

sym: 
    method_name
  | INSTANCE_VARIABLE
  | GLOBAL_VARIABLE
      {
          $$ = "$" + $1;
      }
  | CLASS_VARIABLE
  | match_reference 
      {
          $$ = $1.FullName;
      }
;

undef_list:
      method_name_or_symbol
        {
            $$ = CollectionUtils.MakeList<ConstructedSymbol>($1);
        }
   | undef_list COMMA 
        {
            _tokenizer.LexicalState = LexicalState.EXPR_FNAME;
        } 
     method_name_or_symbol
        {
            ($$ = $1).Add($4);
        }
;

op:
      PIPE              { $$ = Symbols.BitwiseOr; }
    | CARET             { $$ = Symbols.Xor; }
    | AMPERSAND         { $$ = Symbols.BitwiseAnd; }
    | CMP               { $$ = Symbols.Comparison; }
    | EQUAL             { $$ = Symbols.Equal; }
    | STRICT_EQUAL      { $$ = Symbols.StrictEqual; }
    | MATCH             { $$ = Symbols.Match; }
    | NMATCH            { $$ = Symbols.NotMatch; }      // 1.9
    | GREATER           { $$ = Symbols.GreaterThan; }
    | GREATER_OR_EQUAL  { $$ = Symbols.GreaterEqual; }
    | LESS              { $$ = Symbols.LessThan; }
    | LESS_OR_EQUAL     { $$ = Symbols.LessEqual; }
    | NOT_EQUAL         { $$ = Symbols.NotEqual; }      // 1.9
    | LSHFT             { $$ = Symbols.LeftShift; }
    | RSHFT             { $$ = Symbols.RightShift; }
    | PLUS              { $$ = Symbols.Plus; }
    | MINUS             { $$ = Symbols.Minus; }
    | ASTERISK          { $$ = Symbols.Multiply; }
    | STAR              { $$ = Symbols.Multiply; }
    | SLASH             { $$ = Symbols.Divide; }
    | PERCENT           { $$ = Symbols.Mod; }
    | POW               { $$ = Symbols.Power; }
    | BANG              { $$ = Symbols.Bang; }
    | TILDE             { $$ = Symbols.BitwiseNot; }
    | UNARY_PLUS        { $$ = Symbols.UnaryPlus; }
    | UNARY_MINUS       { $$ = Symbols.UnaryMinus; }
    | ITEM_GETTER       { $$ = Symbols.ArrayItemRead; }
    | ITEM_SETTER       { $$ = Symbols.ArrayItemWrite; }
    | BACKTICK          { $$ = Symbols.Backtick; }
;

reswords: 
      LINE | FILE | ENCODING | UPPERCASE_BEGIN | UPPERCASE_END
    | ALIAS | AND | BEGIN | BREAK | CASE | CLASS | DEF
    | DEFINED | DO | BLOCK_DO | ELSE | ELSIF | END | ENSURE | FALSE
    | FOR | IN | MODULE | NEXT | NIL | NOT
    | OR | REDO | RESCUE | RETRY | RETURN | SELF | SUPER
    | THEN | TRUE | UNDEF | WHEN | YIELD
    | IF_MOD | UNLESS_MOD | WHILE_MOD | UNTIL_MOD | RESCUE_MOD
;

arg:
      lhs ASSIGNMENT arg
        {
            $$ = new SimpleAssignmentExpression($1, $3, null, @$);
        }
    | lhs ASSIGNMENT arg RESCUE_MOD arg
        {
            $$ = new SimpleAssignmentExpression($1, new RescueExpression($3, $5, MergeLocations(@4, @5), MergeLocations(@3, @5)), null, @$);
        }
    | lhs ASSIGNMENT arg RESCUE_MOD jump_statement_parameterless
        {
            $$ = new SimpleAssignmentExpression($1, new RescueExpression($3, $5, MergeLocations(@4, @5), MergeLocations(@3, @5)), null, @$);
        }
    | var_lhs OP_ASSIGNMENT arg
        {
            $$ = new SimpleAssignmentExpression($1, $3, $2, @$);
        }
    | primary LEFT_INDEXING_BRACKET call_args_opt closing_bracket OP_ASSIGNMENT arg    // 1.9 change
        {
            $$ = new SimpleAssignmentExpression(MakeArrayItemAccess($1, $3, @2), $6, $5, @$);
        }
    | primary DOT IDENTIFIER OP_ASSIGNMENT arg
        {
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | primary DOT CONSTANT_IDENTIFIER OP_ASSIGNMENT arg
        {
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | primary SEPARATING_DOUBLE_COLON IDENTIFIER OP_ASSIGNMENT arg
        {
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | primary SEPARATING_DOUBLE_COLON CONSTANT_IDENTIFIER OP_ASSIGNMENT arg
        {
            _tokenizer.ReportError(Errors.ConstantReassigned);
            $$ = new ErrorExpression(@$);
        }
    | LEADING_DOUBLE_COLON CONSTANT_IDENTIFIER OP_ASSIGNMENT arg
        {
            _tokenizer.ReportError(Errors.ConstantReassigned);
            $$ = new ErrorExpression(@$);
        }
    | match_reference OP_ASSIGNMENT arg
        {
            MatchReferenceReadOnlyError($1);
            $$ = new ErrorExpression(@$);
        }
    | arg PLUS arg
        {
            $$ = new MethodCall($1, Symbols.Plus, new Arguments($3), @$);
        }
    | arg MINUS arg
        {
            $$ = new MethodCall($1, Symbols.Minus, new Arguments($3), @$);
        }
    | arg ASTERISK arg
        {
            $$ = new MethodCall($1, Symbols.Multiply, new Arguments($3), @$);
        }
    | arg SLASH arg
        {
            $$ = new MethodCall($1, Symbols.Divide, new Arguments($3), @$);
        }
    | arg PERCENT arg
        {
            $$ = new MethodCall($1, Symbols.Mod, new Arguments($3), @$);
        }
    | arg POW arg
        {
            $$ = new MethodCall($1, Symbols.Power, new Arguments($3), @$);
        }
    | NUMBER_NEGATION INTEGER POW arg
        {
            // ** has precedence over unary minus, hence -number**arg is equivalent to -(number**arg)
            $$ = new MethodCall(new MethodCall(Literal.Integer($2, @2), Symbols.Power, new Arguments($4), @3), Symbols.UnaryMinus, Arguments.Empty, @$);
        }
    | NUMBER_NEGATION BIG_INTEGER POW arg
        {
            $$ = new MethodCall(new MethodCall(Literal.BigInteger($2, @2), Symbols.Power, new Arguments($4), @3), Symbols.UnaryMinus, Arguments.Empty, @$);
        }
    | NUMBER_NEGATION FLOAT POW arg
        {
            $$ = new MethodCall(new MethodCall(Literal.Double($2, @2), Symbols.Power, new Arguments($4), @3), Symbols.UnaryMinus, Arguments.Empty, @$);
        }
    | UNARY_PLUS arg
        {
            $$ = new MethodCall($2, Symbols.UnaryPlus, null, @$);
        }
    | UNARY_MINUS arg
        {
            $$ = new MethodCall($2, Symbols.UnaryMinus, null, @$);
        }
    | arg PIPE arg
        {
            $$ = new MethodCall($1, Symbols.BitwiseOr, new Arguments($3), @$);
        }
    | arg CARET arg
        {
            $$ = new MethodCall($1, Symbols.Xor, new Arguments($3), @$);
        }
    | arg AMPERSAND arg
        {
            $$ = new MethodCall($1, Symbols.BitwiseAnd, new Arguments($3), @$);
        }
    | arg CMP arg
        {
            $$ = new MethodCall($1, Symbols.Comparison, new Arguments($3), @$);
        }
    | arg GREATER arg
        {
            $$ = new MethodCall($1, Symbols.GreaterThan, new Arguments($3), @$);
        }
    | arg GREATER_OR_EQUAL arg
        {
            $$ = new MethodCall($1, Symbols.GreaterEqual, new Arguments($3), @$);
        }
    | arg LESS arg
        {
            $$ = new MethodCall($1, Symbols.LessThan, new Arguments($3), @$);
        }
    | arg LESS_OR_EQUAL arg
        {
            $$ = new MethodCall($1, Symbols.LessEqual, new Arguments($3), @$);
        }
    | arg EQUAL arg
        {
            $$ = new MethodCall($1, Symbols.Equal, new Arguments($3), @$);
        }
    | arg STRICT_EQUAL arg
        {
            $$ = new MethodCall($1, Symbols.StrictEqual, new Arguments($3), @$);
        }
    | arg NOT_EQUAL arg
        {
            $$ = new MethodCall($1, Symbols.NotEqual, new Arguments($3), @$);
        }
    | arg MATCH arg
        {
            // TODO: MRI inconsistent (NMATCH vs MATCH):
            $$ = MakeMatch($1, $3, @$);
        }
    | arg NMATCH arg
        {
            $$ = new MethodCall($1, Symbols.NotMatch, new Arguments($3), @$);
        }
    | BANG arg
        {
            $$ = new NotExpression($2, @$);
        }
    | TILDE arg
        {
            $$ = new MethodCall($2, Symbols.BitwiseNot, Arguments.Empty, @$);
        }
    | arg LSHFT arg
        {
            $$ = new MethodCall($1, Symbols.LeftShift, new Arguments($3), @$);
        }
    | arg RSHFT arg
        {
            $$ = new MethodCall($1, Symbols.RightShift, new Arguments($3), @$);
        }
    | arg LOGICAL_AND arg
        {
            $$ = new AndExpression($1, $3, @$);
        }
    | arg LOGICAL_OR arg
        {
            $$ = new OrExpression($1, $3, @$);
        }
    | arg LOGICAL_AND jump_statement_parameterless
        {
            $$ = new ConditionalJumpExpression($1, $3, false, null, @$);
        }
    | arg LOGICAL_OR jump_statement_parameterless
        {
            $$ = new ConditionalJumpExpression($1, $3, true, null, @$);
        }
    | arg DOUBLE_DOT arg
        {
            $$ = new RangeExpression($1, $3, false, @$);
        }
    | arg TRIPLE_DOT arg
        {
            $$ = new RangeExpression($1, $3, true, @$);
        }
    | DEFINED new_line_opt arg
        {
            $$ = new IsDefinedExpression($3, @$);
        }
    | arg QUESTION_MARK arg new_line_opt COLON arg
        {
            $$ = new ConditionalExpression(ToCondition($1), $3, $6, @$);
        }
    | arg QUESTION_MARK jump_statement_parameterless new_line_opt COLON arg
        {
            $$ = new ConditionalJumpExpression(ToCondition($1), $3, false, $6, @$);
        }
    | arg QUESTION_MARK arg new_line_opt COLON jump_statement_parameterless
        {
            $$ = new ConditionalJumpExpression(ToCondition($1), $6, true, $3, @$);
        }
    | arg QUESTION_MARK jump_statement_parameterless new_line_opt COLON jump_statement_parameterless
        {
            $$ = new ConditionalStatement(ToCondition($1), false, $3, $6, @$);
        }
    | primary
        {
            $$ = $1;
        }
;
        
array_items:                              // 1.9 changes
      /* empty */  
        {
            SetArguments();
        }
    | args trailer
        {
            PopAndSetArguments($1, null);
        }
    | args COMMA maplets trailer      
        {
            PushArgument($1, new HashConstructor($3.ToArray(), @3));
            PopAndSetArguments($1 + 1, null);
        }
    | maplets trailer
        {
            SetArguments(new HashConstructor($1.ToArray(), @1), null);
        }
;

parenthesized_args:                      // 1.9 changes
      LEFT_PARENTHESIS call_args_opt closing_parenthesis
        {
            $<Arguments>$ = $<Arguments>2 ?? Arguments.Empty;
            $<Block>$ = $<Block>2;
        }
;

parenthesized_args_opt: 
      /* empty */
        {
            SetNoArguments(null);
        }
    | parenthesized_args
        {
            $$ = $1;
        }
;

call_args_opt:                    // 1.9 changes
      /* empty */
        {
            SetNoArguments(null);
        }
    | call_args
        {
            $$ = $1;
        }
;

call_args:                        // 1.9 changes
      args block_reference_opt
        {
            PopAndSetArguments($1, $2);
        }
    | args COMMA maplets block_reference_opt
        {
            PushArgument($1, new HashConstructor($3.ToArray(), @3));
            PopAndSetArguments($1 + 1, $4);
        }
    | maplets block_reference_opt
        {
            SetArguments(new HashConstructor($1.ToArray(), @1), $2);
        }
    | block_reference
        {
            SetArguments($1);
        }
    | command
        {
            SetArguments($1);
        }
;

command_args:                    // 1.9 changes
        {
            $<Integer1>$ = _tokenizer.EnterCommandArguments();
        }
      call_args
        {
            _tokenizer.LeaveCommandArguments($<Integer1>1);
            $$ = $2;
        }
;

block_reference:
      BLOCK_REFERENCE arg
        {
            $$ = new BlockReference($2, @$);
        }
;

block_reference_opt:
      COMMA block_reference
        {
            $$ = $2;
        }
    | COMMA                      // 1.9
        {
            $$ = null;
        }
    | /* empty */
        {
            $$ = null;
        }
;

args: 
      arg
        {
            PushArgument(0, $1);
        }
    | compound_rhs
        {
            $$ = $1;
        }
;

compound_rhs: 
      STAR arg
        {
            PushArgument(0, new SplattedArgument($2));
        }
    | args COMMA arg
        {
            PushArgument($1, $3);
        }
    | args COMMA STAR arg
        {
            PushArgument($1, new SplattedArgument($4));
        }
;

primary: 
      numeric_literal
    | symbol
        {
            $$ = MakeSymbolLiteral($1, @1);
        }
    | string_concatenation
        {
            $$ = new StringConstructor($1, StringKind.Mutable, @1);
        }
    | shell_string
    | regexp
    | words
    | verbatim_words
    | var_ref
    | match_reference
        {
            $$ = $1;
        }
    | FUNCTION_IDENTIFIER
        {
            $$ = new MethodCall(null, $1, null, @1);
        }
    | primary SEPARATING_DOUBLE_COLON CONSTANT_IDENTIFIER
        {
            $$ = new ConstantVariable($1, $3, @$);
        }
    | LEADING_DOUBLE_COLON CONSTANT_IDENTIFIER
        {
            $$ = new ConstantVariable(null, $2, @$);
        }
    | LEFT_BRACKET array_items RIGHT_BRACKET
        {
            $$ = new ArrayConstructor($2, @$);
        }
    | LEFT_BRACE RIGHT_BRACE
        {
            $$ = new HashConstructor(Maplet.EmptyArray, @$);
        }
    | LEFT_BRACE maplets trailer RIGHT_BRACE
        {
            $$ = new HashConstructor($2.ToArray(), @$);
        }                      
    | YIELD LEFT_PARENTHESIS call_args closing_parenthesis
        {
            $$ = new YieldCall(RequireNoBlockArg($3), @$);
        }
    | YIELD LEFT_PARENTHESIS closing_parenthesis
        {
            $$ = new YieldCall(Arguments.Empty, @$);
        }
    | YIELD
        {
            $$ = new YieldCall(null, @1);
        }
    | DEFINED new_line_opt LEFT_PARENTHESIS expr closing_parenthesis
        {
            $$ = new IsDefinedExpression($4, @$);
        }
    | NOT LEFT_PARENTHESIS expr closing_parenthesis
        { 
            $$ = new NotExpression($3, @$);
        }
    | NOT LEFT_PARENTHESIS closing_parenthesis
        { 
            $$ = new NotExpression(null, @$);
        }
    | operation brace_block
        {
            $$ = new MethodCall(null, $1, null, $2, @1);
        }
    | method_call
    | method_call brace_block
        {    
            SetBlock($1, $2);
            $$ = $1;
        }
    | IF expr then compstmt if_tail END
        {
            $$ = MakeIfExpression(ToCondition($2), $4, $5, @$);
        }
    | UNLESS expr then compstmt else_opt END
        {
            $$ = new UnlessExpression(ToCondition($2), $4, $5, @$);
        }
    | WHILE
        {
            _tokenizer.EnterLoopCondition();
        }
      expr do
        {
            _tokenizer.LeaveLoopCondition();
        }
      compstmt END
        {
            $$ = new WhileLoopExpression(ToCondition($3), true, false, $6, @$);
        }
    | UNTIL
        {
            _tokenizer.EnterLoopCondition();
        }
      expr do
        {
            _tokenizer.LeaveLoopCondition();
        }
      compstmt END
        {
            $$ = new WhileLoopExpression(ToCondition($3), false, false, $6, @$);
        }
    | case_expression
    | FOR for_parameters IN
        {
            _tokenizer.EnterLoopCondition();
        }
      expr do
        {
            _tokenizer.LeaveLoopCondition();
            EnterPaddingScope();
        }
      compstmt END
        {
            $$ = MakeForLoopExpression($2, $5, $8, @$);
            LeaveScope();
        }
    | block_expression
        {
            $$ = $1;
        }
    | definition_expression
        {
            $$ = $1;
        }
;
        
block_expression:
      LEFT_ARG_PARENTHESIS expr 
        {
            _tokenizer.LexicalState = LexicalState.EXPR_ENDARG;
        } 
      closing_parenthesis
        {
            _tokenizer.ReportWarning(Errors.InterpretedAsGroupedExpression);            
            // BlockExpression behaves like an expression, so we don't need to create one here:
            $$ = $2;
        }
    | LEFT_EXPR_PARENTHESIS compstmt RIGHT_PARENTHESIS
        {
            $$ = MakeBlockExpression($2, @$);
        }
    | BEGIN body END
        {
            $$ = $2;
        }
;

definition_expression:
      CLASS qualified_module_name superclass
        {                
            EnterClassDefinitionScope();
        }
      body END
        {
            if (InMethod) {
                ErrorSink.Add(_sourceUnit, "class definition in method body", @1, -1, Severity.Error);
            }
            $$ = new ClassDefinition(CurrentScope, $2, $3, $5, @$);
            LeaveScope();
        }
    | CLASS LSHFT expr
        {
            $<Integer1>$ = _inInstanceMethodDefinition;
            _inInstanceMethodDefinition = 0;
        }
      term
        {
            $<Integer1>$ = _inSingletonMethodDefinition;
            _inSingletonMethodDefinition = 0;
            EnterSingletonClassDefinitionScope();
        }
      body END
        {
            _inInstanceMethodDefinition = $<Integer1>4;
            _inSingletonMethodDefinition = $<Integer1>6;
            $$ = new SingletonDefinition(LeaveScope(), $3, $7, @$);
        }
    | MODULE qualified_module_name
        {
            EnterModuleDefinitionScope();
        }
      body END
        {
            if (InMethod) {
                ErrorSink.Add(_sourceUnit, "module definition in method body", @1, -1, Severity.Error);
            }
            $$ = new ModuleDefinition(CurrentScope, $2, $4, @$);
            LeaveScope();
        }
    | DEF method_name
        {
            _inInstanceMethodDefinition++;
            EnterMethodDefinitionScope();
        }
      method_parameters body END
        {
            _inInstanceMethodDefinition--;
            $$ = new MethodDefinition(CurrentScope, null, $2, $4, $5, @$);
            LeaveScope();
        }
    | DEF singleton dot_or_colon
        {
            _tokenizer.LexicalState = LexicalState.EXPR_FNAME;
        }
      method_name
        {
            _inSingletonMethodDefinition++;
            _tokenizer.LexicalState = LexicalState.EXPR_ENDFN; // 1.8: EXPR_END
            EnterSingletonMethodDefinitionScope();
        }
      method_parameters body END
        {
            _inSingletonMethodDefinition--;
            $$ = new MethodDefinition(CurrentScope, $2, $5, $7, $8, @$);
            LeaveScope();
        }
     | LAMBDA lambda                       // 1.9
        {
            $$ = $2;
        }
;

body: 
      compstmt rescue_clauses_opt else_opt ensure_opt
        {
            $$ = MakeBody($1, $2, $3, @3, $4, @$);
        }
;

case_expression:
      CASE expr terms_opt when_clauses else_opt END
        {
            $$ = new CaseExpression($2, $4.ToArray(), $5, @$);
        }
    | CASE terms_opt when_clauses else_opt END
        {
            $$ = new CaseExpression(null, $3.ToArray(), $4, @$);
        }  
;

then:                    // 1.9: COLON removed
      term
    | THEN
    | term THEN
;

do:                      // 1.9: COLON removed
      term
    | LOOP_DO
;

if_tail: 
      else_opt
        {
            $$ = MakeListAddOpt($1);
        }
    | ELSIF expr then compstmt if_tail
        {
            $5.Add(new ElseIfClause($2, $4, @$));
            $$ = $5;
        }
;

else_opt: /* empty */
            {
                $$ = null;
            }
        | ELSE compstmt
            {
                $$ = new ElseIfClause(null, $2, @$);
            }
        ;

for_parameters:
      lhs 
        { 
            $$ = new CompoundLeftValue(new LeftValue[] { $1 }); 
        }
    | compound_lhs 
        { 
            $$ = $1; 
        }
;

parameter_list:                                  // 1.9 change
      parenthesized_parameter
        {
            $$ = CollectionUtils.MakeList<LeftValue>($1);
        }
    | parameter_list COMMA parenthesized_parameter
        {
            ($$ = $1).Add($3);
        }
;

parenthesized_parameter:                         // 1.9
      parameter
        {
            $$ = $1;
        }
    | LEFT_EXPR_PARENTHESIS compound_parameters closing_parenthesis
        {
            $$ = $2;
        }
;

compound_parameters:                             // 1.9
      compound_parameter_list
        {
            $$ = new CompoundLeftValue($1.ToArray());
        }
    | compound_parameter_list COMMA STAR parameter
        {
            $$ = MakeCompoundLeftValue($1, $4, null);
        }
    | compound_parameter_list COMMA STAR
        {
            $$ = MakeCompoundLeftValue($1, Placeholder.Singleton, null);
        }
    | compound_parameter_list COMMA STAR parameter COMMA compound_parameter_list
        {
            $$ = MakeCompoundLeftValue($1, $4, $6);
        }
    | compound_parameter_list COMMA STAR COMMA compound_parameter_list
        {
            $$ = MakeCompoundLeftValue($1, Placeholder.Singleton, $5);
        }
    | STAR parameter COMMA compound_parameter_list
        {
            $$ = MakeCompoundLeftValue(null, $2, $4);
        }
    | STAR COMMA compound_parameter_list
        {
            $$ = MakeCompoundLeftValue(null, Placeholder.Singleton, $3);
        }
    | STAR parameter
        {
            $$ = new CompoundLeftValue(new LeftValue[] { $2 }, 0);
        }
    | STAR
        {
            $$ = new CompoundLeftValue(new LeftValue[] { Placeholder.Singleton }, 0);
        }
;

compound_parameter_list:                         // 1.9
      compound_parameter
        {
            $$ = CollectionUtils.MakeList<LeftValue>($1);
        }
    | compound_parameter_list COMMA compound_parameter
        {
            ($$ = $1).Add($3);
        }
;

compound_parameter:                              // 1.9
      parameter
        {
            $$ = $1;
        }
    | LEFT_EXPR_PARENTHESIS compound_parameters closing_parenthesis
        {
            $$ = $2;
        }
;

block_parameter_list:
      parameter_list COMMA default_block_parameter_list COMMA parameter_array block_parameter_opt
        {
            $$ = new Parameters($1.ToArray(), $1.Count, $3.ToArray(), $6, $6, @$);
        }
    | parameter_list COMMA default_block_parameter_list COMMA parameter_array COMMA parameter_list block_parameter_opt
        {
            $$ = new Parameters(MakeArray($1, $7), $1.Count, $3.ToArray(), $5, $8, @$);
        }
    | parameter_list COMMA default_block_parameter_list block_parameter_opt
        {
            $$ = new Parameters($1.ToArray(), $1.Count, $3.ToArray(), null, $4, @$);
        }
    | parameter_list COMMA default_block_parameter_list COMMA parameter_list block_parameter_opt
        {
            $$ = new Parameters(MakeArray($1, $5), $1.Count, $3.ToArray(), null, $6, @$);
        }
    | parameter_list COMMA parameter_array block_parameter_opt
        {
            $$ = new Parameters($1.ToArray(), $1.Count, null, $3, $4, @$);
        }
    | parameter_list COMMA                                                            // missing from method parameters
        {
            $$ = new Parameters(MakeArray($1, Placeholder.Singleton), $1.Count, null, null, null, @$);
        }
    | parameter_list COMMA parameter_array COMMA parameter_list block_parameter_opt   
        {
            $$ = new Parameters(MakeArray($1, $5), $1.Count, null, $3, $6, @$);
        }
    | parameter_list block_parameter_opt
        {
            $$ = new Parameters($1.ToArray(), $1.Count, null, null, $2, @$);
        }
    | default_block_parameter_list COMMA parameter_array block_parameter_opt
        {
            $$ = new Parameters(null, 0, $1.ToArray(), $3, $4, @$);
        }
    | default_block_parameter_list COMMA parameter_array COMMA parameter_list block_parameter_opt
        {
            $$ = new Parameters($5.ToArray(), 0, $1.ToArray(), $3, $6, @$);
        }
    | default_block_parameter_list block_parameter_opt
        {
            $$ = new Parameters(null, 0, $1.ToArray(), null, $2, @$);
        }
    | default_block_parameter_list COMMA parameter_list block_parameter_opt
        {
            $$ = new Parameters($3.ToArray(), 0, $1.ToArray(), null, $4, @$);
        }
    | parameter_array block_parameter_opt
        {
            $$ = new Parameters(null, 0, null, $1, $2, @$);
        }
    | parameter_array COMMA parameter_list block_parameter_opt
        {
            $$ = new Parameters($3.ToArray(), 0, null, $1, $4, @$);
        }
    | block_parameter
        {
            $$ = new Parameters(null, 0, null, null, $1, @$);
        }
    | /* empty */
        {
            $$ = Parameters.Empty;
        }
;

block_parameters_opt:                         // 1.9
      /* empty */
        {
            $$ = null;
        }
    | block_parameters
        {
            _tokenizer.CommandMode = true;
            $$ = $1;
        }
;

block_parameters:                                 // 1.9
      LOGICAL_OR
        {
            $<Parameters>$ = null;
        }
    | PIPE block_parameter_list block_variables_opt PIPE
        {
            $<Parameters>$ = $2;
        }
;

block_variables_opt:                             // 1.9
      /* empty */
    | SEMICOLON block_variables
;

block_variables:                                // 1.9
      parameter
    | block_variables COMMA parameter
;

lambda:                                         // 1.9
        {
            $<Integer1>$ = _tokenizer.EnterLambdaDefinition();
            EnterNestedScope();
        }
      lambda_parameters lambda_body
        {
            $$ = MakeLambdaDefinition($2, $3, @$);
            _tokenizer.LeaveLambdaDefinition($<Integer1>$);
            LeaveScope();
        }
;

lambda_parameters:                              // 1.9
      LEFT_PARENTHESIS parameters block_variables_opt closing_parenthesis
		{
            $$ = $2;
		}
	| parameters
		{
            $$ = $1;
		}
;

lambda_body:                             // 1.9
      LEFT_LAMBDA_BRACE compstmt RIGHT_BRACE
        {
            $$ = $2;
        }
    | LAMBDA_DO compstmt END
        {
            $$ = $2;
        }
;

do_block:                                // 1.9 change
      BLOCK_DO
        {
            EnterNestedScope();
        }
      block_parameters_opt compstmt END           
        {
            $$ = MakeBlockDefinition($3, $4, @$);
            LeaveScope();
        }
;

block_call: 
      command do_block
        {      
            if ($1 is YieldCall) {
                _tokenizer.ReportError(Errors.BlockGivenToYield);
            }
			
            SetBlock($$ = $1, $2);
        }
    | block_call DOT operation2 parenthesized_args_opt
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
    | block_call SEPARATING_DOUBLE_COLON operation2 parenthesized_args_opt
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
;

method_call: 
      operation parenthesized_args
        {
            $$ = MakeMethodCall(null, $1, $2, @$);
        }
    | primary DOT operation2 parenthesized_args_opt
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
    | primary SEPARATING_DOUBLE_COLON operation2 parenthesized_args
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
    | primary SEPARATING_DOUBLE_COLON operation3
        {
            $$ = new MethodCall($1, $3, null, @3);
        }
    | primary DOT parenthesized_args                                         // 1.9
        {
            $$ = MakeMethodCall($1, Symbols.Call, $3, @$);
        }
    | primary SEPARATING_DOUBLE_COLON parenthesized_args                     // 1.9
        {
            $$ = MakeMethodCall($1, Symbols.Call, $3, @$);
        }
    | primary LEFT_INDEXING_BRACKET call_args_opt closing_bracket            // 1.9
        {
            $$ = MakeMethodCall($1, Symbols.ArrayItemRead, $3, @$);
        }
    | SUPER parenthesized_args
        {
            $$ = MakeSuperCall($2, @1);
        }
    | SUPER
        {
            $$ = new SuperCall(null, null, @1);
        }
;

brace_block: 
      LEFT_BLOCK_BRACE
        {
            EnterNestedScope();
        }
      block_parameters_opt compstmt RIGHT_BRACE
        {
            $$ = MakeBlockDefinition($3, $4, @$);
            LeaveScope();
        }
    | DO
        {
            EnterNestedScope();    
        }
      block_parameters_opt compstmt END
        {
            $$ = MakeBlockDefinition($3, $4, @$);
            LeaveScope();
        }
;

when_clauses: 
      when_clause 
        {
            $$ = CollectionUtils.MakeList<WhenClause>($1); 
        }
    | when_clauses when_clause 
        {
            ($$ = $1).Add($2);
        }
;

when_clause:
      WHEN args then compstmt 
        {
            $$ = new WhenClause(PopArguments($2), $4, @4);
        }
;

rescue_clauses_opt:  
      /* empty */
        {
            $$ = null;
        }
    | rescue_clauses
;

rescue_clauses: 
      rescue_clause 
        {
            $$ = CollectionUtils.MakeList<RescueClause>($1);
        }
    | rescue_clauses rescue_clause 
        {
            ($$ = $1).Add($2);
        }
;

rescue_clause: 
      RESCUE exc_var then compstmt
        {
            $$ = new RescueClause(Expression.EmptyArray, $2, $4, @$);        
        }
    | RESCUE args exc_var then compstmt
        {
            $$ = new RescueClause(PopArguments($2), $3, $5, @$);        
        }
;

exc_var: 
      /* empty */
        {
            $$ = null;
        }
    | DOUBLE_ARROW lhs
        {
            $$ = $2;
        }
;

ensure_opt: 
      /* empty */
        {
            $$ = null;
        }
    | ENSURE compstmt
        {
            $$ = $2;
        }
;
        
string_concatenation: 
      CHARACTER                                     // 1.9
        {
            $$ = CollectionUtils.MakeList<Expression>(MakeStringLiteral($1, @1));
        }
    | string
        {
            $$ = $1;
        }
    | string_concatenation string
        {
            ($$ = $1).AddRange($2);
        }
;

string:
      STRING_BEGIN string_contents STRING_END
        {
            $$ = $2;
        }
;

shell_string:
      SHELL_STRING_BEGIN string_contents STRING_END
        {
            $$ = new StringConstructor($2, StringKind.Command, @$);
        }
;

regexp:
      REGEXP_BEGIN string_contents REGEXP_END
        {
            $$ = new RegularExpression($2, $3, @$);
        }
;

words: 
      WORDS_BEGIN STRING_END
        {
            $$ = new ArrayConstructor(null, @$);
        }
    | WORDS_BEGIN word_list word STRING_END
        {
            $2.Add(new StringConstructor($3, StringKind.Mutable, @3));
            $$ = new ArrayConstructor(new Arguments($2.ToArray()), @$);
        }
;

word_list: 
      /* empty */
        {
            $$ = new List<Expression>();
        }
    | word_list word WORD_SEPARATOR
        {
            ($$ = $1).Add(new StringConstructor($2, StringKind.Mutable, @2));
        }
;

word: 
      string_content
        {
            $$ = CollectionUtils.MakeList<Expression>($1);
        }
    | word string_content
        {
            ($$ = $1).Add($2);
        }
;

verbatim_words: 
      VERBATIM_WORDS_BEGIN STRING_END
        {
            $$ = new ArrayConstructor(null, @$);
        }
    | VERBATIM_WORDS_BEGIN verbatim_word_list STRING_CONTENT STRING_END
        {
            $2.Add(MakeStringLiteral($3, @3));
            $$ = MakeVerbatimWords($2, @$);
        }
;

verbatim_word_list: 
      /* empty */
        {
            $$ = new List<Expression>();
        }
    | verbatim_word_list STRING_CONTENT WORD_SEPARATOR
        {
            ($$ = $1).Add(MakeStringLiteral($2, @2));
        }
;

string_contents: 
      /* empty */
        {
            $$ = new List<Expression>();
        }
    | string_contents string_content
        {
            ($$ = $1).Add($2);
        }
;


string_content: 
      STRING_CONTENT
        {
            $$ = MakeStringLiteral($1, @$);
        }
    | STRING_EMBEDDED_VARIABLE_BEGIN string_embedded_variable
        {
            $$ = $2;
        }
    | STRING_EMBEDDED_CODE_BEGIN compstmt STRING_EMBEDDED_CODE_END
        {
            // STRING_EMBEDDED_CODE_END leaves parenthesised expression, but command_args in compstmt restores it back to the state after 
            // STRING_EMBEDDED_CODE_BEGIN. So we need to leave it again.
            _tokenizer.PopParenthesisedExpressionStack();
            $$ = MakeBlockExpression($2, @2);
        }
;

string_embedded_variable:
      GLOBAL_VARIABLE
      { 
          $$ = new GlobalVariable($1, @$); 
      }
    | match_reference
      { 
          $$ = $1; 
      }
    | INSTANCE_VARIABLE
      { 
          $$ = new InstanceVariable($1, @$); 
      }
    | CLASS_VARIABLE 
      { 
          $$ = new ClassVariable($1, @$); 
      }
;

numeric_literal: 
      INTEGER
        {
            // unsigned integer:
            $$ = Literal.Integer($1, @$);
        }
    | BIG_INTEGER
        {
            $$ = Literal.BigInteger($1, @$);
        }
    | FLOAT
        {
            $$ = Literal.Double($1, @$);
        }
    | NUMBER_NEGATION INTEGER                      %prec LOWEST
        {
            // cannot overflow INTEGER is unsigned and Int32.MaxValue < |Int32.MinValue|
            $$ = Literal.Integer(-$2, @$);
        }
    | NUMBER_NEGATION BIG_INTEGER                  %prec LOWEST
        {
            // TODO: -|Int32.MinValue| actually ends up here (converted to bigint) instead of being Int32. We should fix that.
            $$ = Literal.BigInteger(-$2, @$);
        }
    | NUMBER_NEGATION FLOAT                        %prec LOWEST
        {
            $$ = Literal.Double(-$2, @$);
        }
;

variable: 
      IDENTIFIER          { $<VariableFactory>$ = VariableFactory.Identifier; $<String>$ = $1; }
    | INSTANCE_VARIABLE   { $<VariableFactory>$ = VariableFactory.Instance; $<String>$ = $1; }
    | GLOBAL_VARIABLE     { $<VariableFactory>$ = VariableFactory.Global; $<String>$ = $1; }
    | CONSTANT_IDENTIFIER { $<VariableFactory>$ = VariableFactory.Constant; $<String>$ = $1; }
    | CLASS_VARIABLE      { $<VariableFactory>$ = VariableFactory.Class; $<String>$ = $1; }
    | NIL                 { $<VariableFactory>$ = VariableFactory.Nil; $<String>$ = null; }
    | SELF                { $<VariableFactory>$ = VariableFactory.Self; $<String>$ = null; }
    | TRUE                { $<VariableFactory>$ = VariableFactory.True; $<String>$ = null; }
    | FALSE               { $<VariableFactory>$ = VariableFactory.False; $<String>$ = null; }
    | FILE                { $<VariableFactory>$ = VariableFactory.File; $<String>$ = null; }
    | LINE                { $<VariableFactory>$ = VariableFactory.Line; $<String>$ = null; }
    | ENCODING            { $<VariableFactory>$ = VariableFactory.Encoding; $<String>$ = null; }
;

var_ref:
      variable
        {
            $$ = VariableFactory.MakeRead($<VariableFactory>1, this, $<String>1, @$);
        }
;
        
var_lhs: 
      variable
        {
            $$ = VariableFactory.MakeLeftValue($<VariableFactory>1, this, $<String>1, @$);
        }
;

match_reference: 
      MATCH_REFERENCE 
        { 
            $$ = new RegexMatchReference($1, @1); 
        }
;

superclass: 
      term
        {
            $$ = null;
        }
    | LESS 
        {
            _tokenizer.LexicalState = LexicalState.EXPR_BEG;
        }
      expr term
        {
            $$ = $3;
        }
    | ERROR term
        {
            StopErrorRecovery();
            $$ = null;
        }
;

method_parameters:
      LEFT_PARENTHESIS parameters closing_parenthesis
        {
            $$ = $2;
            _tokenizer.LexicalState = LexicalState.EXPR_BEG;
            _tokenizer.CommandMode = true;
        }
    | parameters term
        {
            $$ = $1;
        }
;

parameters: 
      parameter_list COMMA default_parameter_list COMMA parameter_array block_parameter_opt
        {
            $$ = new Parameters($1.ToArray(), $1.Count, $3.ToArray(), $5, $6, @$);
        }
    | parameter_list COMMA default_parameter_list COMMA parameter_array COMMA parameter_list block_parameter_opt   // 1.9
        { 
            $$ = new Parameters(MakeArray($1, $7), $1.Count, $3.ToArray(), $5, $8, @$);
        }
    | parameter_list COMMA default_parameter_list block_parameter_opt
        {
            $$ = new Parameters($1.ToArray(), $1.Count, $3.ToArray(), null, $4, @$);
        }
    | parameter_list COMMA default_parameter_list COMMA parameter_list block_parameter_opt                         // 1.9
        {
            $$ = new Parameters(MakeArray($1, $5), $1.Count, $3.ToArray(), null, $6, @$);
        }
    | parameter_list COMMA parameter_array block_parameter_opt
        {
            $$ = new Parameters($1.ToArray(), $1.Count, null, $3, $4, @$);
        }
    | parameter_list COMMA parameter_array COMMA parameter_list block_parameter_opt
        {
            $$ = new Parameters(MakeArray($1, $5), $1.Count, null, $3, $6, @$);
        }
    | parameter_list block_parameter_opt
        {
            $$ = new Parameters($1.ToArray(), $1.Count, null, null, $2, @$);
        }
    | default_parameter_list COMMA parameter_array block_parameter_opt
        {
            $$ = new Parameters(null, 0, $1.ToArray(), $3, $4, @$);
        }
    | default_parameter_list COMMA parameter_array COMMA parameter_list block_parameter_opt                        // 1.9
        {
            $$ = new Parameters($5.ToArray(), 0, $1.ToArray(), $3, $6, @$);
        }
    | default_parameter_list block_parameter_opt
        {
            $$ = new Parameters(null, 0, $1.ToArray(), null, $2, @$);
        }
    | default_parameter_list COMMA parameter_list block_parameter_opt                                              // 1.9
        {
            $$ = new Parameters($3.ToArray(), 0, $1.ToArray(), null, $4, @$);
        }
    | parameter_array block_parameter_opt
        {
            $$ = new Parameters(null, 0, null, $1, $2, @$);
        }
    | parameter_array COMMA parameter_list block_parameter_opt                                                     // 1.9
        {
            $$ = new Parameters($3.ToArray(), 0, null, $1, $4, @$);
        }
    | block_parameter
        {
            $$ = new Parameters(null, 0, null, null, $1, @$);
        }
    | /* empty */ 
        {
            $$ = Parameters.Empty;
        }
;

parameter: 
      CONSTANT_IDENTIFIER
        {    
            _tokenizer.ReportError(Errors.FormalArgumentIsConstantVariable);
            $$ = DefineParameter(GenerateErrorConstantName(), @$);
        }
    | INSTANCE_VARIABLE
        {
            _tokenizer.ReportError(Errors.FormalArgumentIsInstanceVariable);
            $$ = DefineParameter(GenerateErrorConstantName(), @$);
        }
    | GLOBAL_VARIABLE
        {
            _tokenizer.ReportError(Errors.FormalArgumentIsGlobalVariable);
            $$ = DefineParameter(GenerateErrorConstantName(), @$);
        }
    | CLASS_VARIABLE
        {
            _tokenizer.ReportError(Errors.FormalArgumentIsClassVariable);
            $$ = DefineParameter(GenerateErrorConstantName(), @$);
        }                
    | IDENTIFIER
        {           
            $$ = DefineParameter($1, @$);
        }
;

default_parameter:
      IDENTIFIER ASSIGNMENT arg
        {        
            $$ = new SimpleAssignmentExpression(DefineParameter($1, @$), $3, null, @$);
        }
;

// Default block parameter is different from default method parameter
// Block parameter doesn't allow using binary expressions etc. defined by arg due to ambiguity with binary OR operator: lambda { |x=1| }
default_block_parameter:
      IDENTIFIER ASSIGNMENT primary                     
        {        
            $$ = new SimpleAssignmentExpression(DefineParameter($1, @$), $3, null, @$);
        }
;

default_parameter_list: 
      default_parameter
        {
            $$ = CollectionUtils.MakeList($1);
        }
    | default_parameter_list COMMA default_parameter
        {
            ($$ = $1).Add($3);
        }
;

default_block_parameter_list: 
      default_block_parameter
        {
            $$ = CollectionUtils.MakeList($1);
        }
    | default_block_parameter_list COMMA default_block_parameter
        {
            ($$ = $1).Add($3);
        }
;

array_parameter_mark: 
      ASTERISK
    | STAR
;

parameter_array: 
      array_parameter_mark parameter
        {    
            $$ = $2;
        }
    | array_parameter_mark
        {
            $$ = DefineParameter(Symbols.RestArgsLocal, @1);
        }
;

block_parameter_mark: 
      AMPERSAND
    | BLOCK_REFERENCE
;

block_parameter: 
      block_parameter_mark parameter
        {
            $$ = $2;
        }
;

block_parameter_opt: 
     /* empty */
       {
           $$ = null;
       }
   | COMMA block_parameter
       {
           $$ = $2;
       }
;

singleton: 
     var_ref
   | LEFT_PARENTHESIS
       {
           _tokenizer.LexicalState = LexicalState.EXPR_BEG;
       }
     expr closing_parenthesis
       {                        
           $$ = $3;
       }
;

maplets: 
     maplet 
       {
           $$ = CollectionUtils.MakeList<Maplet>($1);
       }
   | maplets COMMA maplet
       {
           ($$ = $1).Add($3);
       }
;

maplet: 
     arg DOUBLE_ARROW arg
       {
           $$ = new Maplet($1, $3, @$);
       }
   | LABEL arg                                      // 1.9
       {
           $$ = new Maplet(MakeSymbolLiteral($1, @1), $2, @$);
       }
;

operation: 
     IDENTIFIER
   | CONSTANT_IDENTIFIER
   | FUNCTION_IDENTIFIER
;

operation2: 
     IDENTIFIER
   | CONSTANT_IDENTIFIER
   | FUNCTION_IDENTIFIER
   | op
;

operation3: 
     IDENTIFIER
   | FUNCTION_IDENTIFIER
   | op
;

dot_or_colon: 
     DOT
   | SEPARATING_DOUBLE_COLON
;

terms_opt: 
      /* empty */
    | terms
;

new_line_opt:
      /* empty */
    | NEW_LINE
;

closing_parenthesis:
      new_line_opt RIGHT_PARENTHESIS
;

closing_bracket:
      new_line_opt RIGHT_BRACKET
;

trailer: 
      /* empty */
    | NEW_LINE
    | COMMA
;

term: 
      SEMICOLON        { StopErrorRecovery(); }
    | NEW_LINE
;

terms: 
      term
    | terms SEMICOLON  { StopErrorRecovery(); }
;
    
%%
