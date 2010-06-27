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
%token CASE WHEN WHILE UNTIL FOR BREAK NEXT REDO RETRY IN DO LOOP_DO BLOCK_DO
%token RETURN YIELD SUPER SELF NIL TRUE FALSE AND OR NOT IF_MOD UNLESS_MOD
%token WHILE_MOD UNTIL_MOD RESCUE_MOD ALIAS DEFINED UPPERCASE_BEGIN UPPERCASE_END LINE FILE ENCODING
%token UNARY_PLUS UNARY_MINUS POW CMP EQUAL STRICT_EQUAL NOT_EQUAL GREATER_OR_EQUAL LESS_OR_EQUAL LOGICAL_AND LOGICAL_OR MATCH NMATCH
%token DOUBLE_DOT TRIPLE_DOT ITEM_GETTER ITEM_SETTER LSHFT RSHFT SEPARATING_DOUBLE_COLON LEADING_DOUBLE_COLON DOUBLE_ARROW
%token SEMICOLON COMMA DOT STAR BLOCK_REFERENCE AMPERSAND BACKTICK
%token LEFT_PARENTHESIS LEFT_EXPR_PARENTHESIS LEFT_ARG_PARENTHESIS RIGHT_PARENTHESIS
%token LEFT_BLOCK_BRACE LEFT_BRACE LEFT_BLOCK_ARG_BRACE RIGHT_BRACE
%token LEFT_BRACKET LEFT_INDEXING_BRACKET RIGHT_BRACKET
%token STRING_EMBEDDED_VARIABLE_BEGIN STRING_EMBEDDED_CODE_BEGIN STRING_EMBEDDED_CODE_END 
%token VERBATIM_HEREDOC_BEGIN VERBATIM_HEREDOC_END
%token STRING_BEGIN REGEXP_BEGIN SHELL_STRING_BEGIN WORDS_BEGIN VERBATIM_WORDS_BEGIN SYMBOL_BEGIN STRING_END

%token<String> IDENTIFIER FUNCTION_IDENTIFIER GLOBAL_VARIABLE INSTANCE_VARIABLE CONSTANT_IDENTIFIER CLASS_VARIABLE OP_ASSIGNMENT 
%token<Integer1> INTEGER 
%token<BigInteger> BIG_INTEGER
%token<Double> FLOAT 
%token STRING_CONTENT                              // (String & StringLiteralEncoding)
%token<Integer1> MATCH_REFERENCE
%token<RegExOptions>  REGEXP_END 

%type<AbstractSyntaxTree> program

%type<Expression> stmt
%type<Statements> stmts compstmt ensure_opt
%type<JumpStatement> jump_statement jump_statement_with_parameters jump_statement_parameterless
%type<Expression> alias_statement
%type<Expression> conditional_statement

%type<Expression> primary expr expression_statement superclass var_ref singleton case_expression
%type<Expression> arg
%type<ArgumentCount> args
%type<Expression> block_expression                 // (Expression | BlockExpression | Body)
%type<Expression> definition_expression            // DefinitionExpression
%type<Body> body
%type<Expression> 

%type<CallExpression> method_call block_call command_call block_command command

%type<ElseIfClause> else_opt 
%type<ElseIfClauses> if_tail
%type<Identifiers> undef_list 
%type<BlockReference> block_reference opt_block_reference 
%type<BlockDefinition> cmd_brace_block brace_block do_block 

%type<Arguments> array_key 
%type when_args                                                             // (ArgumentCount & Expression?)
%type paren_args open_args closed_args command_args command_args_content    // (Arguments & Block?)
%type opt_paren_args                                                        // (Arguments? & Block?)

%type<CompoundRightValue> compound_rhs
%type<ConstantVariable> qualified_module_name 

%type<RescueClauses> rescue_clauses rescue_clauses_opt
%type<RescueClause> rescue_clause

%type<WhenClauses> when_clauses 
%type<WhenClause> when_clause                          
 
%type<Maplets> maplets 
%type<Maplet> maplet 

%type<Parameters> parameters_definition parameters
%type<LocalVariables> parameter_list
%type<LocalVariable> parameter array_parameter block_parameter block_parameter_opt
%type<SimpleAssignmentExpressions> default_parameter_list 
%type<SimpleAssignmentExpression> default_parameter 

%type<Expression> string_embedded_variable              // string embedded variable
%type<Expression> string_content                        // string piece - literal, string embedded code/variable
%type<Expressions> string_contents                      // list of string pieces
%type<Expressions> string                               // quoted string constructor taking list of string pieces "#{foo}bar#{baz}"
%type<Expressions> string_concatenation                 // string constructor taking a list of quoted strings "foo" 'bar' "baz"

%type<Expression> shell_string                          // shell string constructor taking list of string pieces `#{foo}bar#{baz}`

%type<Expressions> word                                 // concatenation of string pieces
%type<Expressions> word_list verbatim_word_list         // list of words separated by space
%type<Expression> words verbatim_words                  // array constructor taking a list of words

%type<Expression> regexp 
%type<Expression> numeric_literal 
%type<Expression> immutable_string 
%type<RegexMatchReference> match_reference 

%type<String> operation variable sym operation2 operation3 module_name op method_name symbol method_name_or_symbol

%type<CompoundLeftValue> compound_lhs
%type<LeftValues> compound_lhs_head
%type<LeftValue> compound_lhs_item compound_lhs_tail
%type<LeftValue> compound_lhs_node

%type<LeftValue> var_lhs 
%type<LeftValue> lhs 
%type<CompoundLeftValue> block_parameters block_parameters_opt 
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
      opt_terms
        {
            $$ = Statements.Empty; 
        } 
    | terms stmts opt_terms
        {
            $$ = $2; 
        } 
    | stmts opt_terms
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

stmt:     alias_statement
        | UNDEF undef_list
            {
                $$ = new UndefineStatement($2, @$);
            }
        | UPPERCASE_BEGIN
            {
                if (InMethod) {
                    _tokenizer.ReportError(Errors.FileInitializerInMethod);
                }
                            
                EnterTopScope();
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
        | jump_statement
            {
                $$ = $1;
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
      RETURN open_args
        {
            $$ = new ReturnStatement(RequireNoBlockArg($2), @$);
        }
    | BREAK open_args
        {
            $$ = new BreakStatement(RequireNoBlockArg($2), @$);
        }
    | NEXT open_args
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
    | lhs ASSIGNMENT command_call
        {
            $$ = new SimpleAssignmentExpression($1, $3, null, @$);
        }
    | compound_lhs ASSIGNMENT command_call
        {
            $$ = new ParallelAssignmentExpression($1, new CompoundRightValue(new Expression[] { $3 }, null), @$);
        }
    | var_lhs OP_ASSIGNMENT command_call
        {
            $$ = new SimpleAssignmentExpression($1, $3, $2, @$);
        }
    | primary LEFT_INDEXING_BRACKET array_key RIGHT_BRACKET OP_ASSIGNMENT command_call
        {                
            $$ = new SimpleAssignmentExpression(new ArrayItemAccess($1, $3, @2), $6, $5, @$);
        }
    | primary DOT IDENTIFIER OP_ASSIGNMENT command_call
        {
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | primary DOT CONSTANT_IDENTIFIER OP_ASSIGNMENT command_call
        {
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | primary SEPARATING_DOUBLE_COLON IDENTIFIER OP_ASSIGNMENT command_call
        {
            $$ = new MemberAssignmentExpression($1, $3, $4, $5, @$);
        }
    | lhs ASSIGNMENT compound_rhs
        {
            $$ = new ParallelAssignmentExpression(new CompoundLeftValue(CollectionUtils.MakeList<LeftValue>($1), null, @1), $3, @$);
        }
    | compound_lhs ASSIGNMENT arg
        {
            $$ = new ParallelAssignmentExpression($1, new CompoundRightValue(new Expression[] { $3 }, null), @$);
        }
    | compound_lhs ASSIGNMENT compound_rhs
        {
            $$ = new ParallelAssignmentExpression($1, $3, @$);
        }
    | arg QUESTION_MARK jump_statement_parameterless COLON arg
        {
            $$ = new ConditionalJumpExpression(ToCondition($1), $3, false, $5, @$);
        }
    | arg QUESTION_MARK arg COLON jump_statement_parameterless
        {
            $$ = new ConditionalJumpExpression(ToCondition($1), $5, true, $3, @$);
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
    | arg QUESTION_MARK jump_statement_parameterless COLON jump_statement_parameterless
        {
            $$ = new ConditionalStatement(ToCondition($1), false, $3, $5, @$);
        }
;

compound_rhs: 
      args COMMA arg
        {
            $$ = new CompoundRightValue(PopArguments($1, $3), null);
        }
    | args COMMA STAR arg
        {
            $$ = new CompoundRightValue(PopArguments($1), $4);
        }
    | STAR arg
        {
            $$ = new CompoundRightValue(Expression.EmptyArray, $2);
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
    | NOT expr
        {
            // TODO: warning: string literal in condition
            $$ = new NotExpression($2, @$);
        }
    | BANG command_call
        {
            // TODO: warning: string literal in condition
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
            $$ = new BlockDefinition(CurrentScope, $3, $4, @$);
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

compound_lhs: 
      compound_lhs_head compound_lhs_item
        {
            $1.Add($2);
            $$ = new CompoundLeftValue($1, null, @$);
        }
    | compound_lhs_head 
        {
              $1.Add(Placeholder.Singleton);
              $$ = new CompoundLeftValue($1, null, @$);
        }
    | LEFT_EXPR_PARENTHESIS compound_lhs RIGHT_PARENTHESIS
        {
            $$ = new CompoundLeftValue(CollectionUtils.MakeList<LeftValue>($2), null, @$);
        }
    | compound_lhs_head compound_lhs_tail
        {
            $$ = new CompoundLeftValue($1, $2, @$);
        }
    | compound_lhs_tail
        {
            $$ = new CompoundLeftValue(LeftValue.EmptyList, $1, @$);
        }
;

compound_lhs_tail:
      STAR compound_lhs_node
        {
            $$ = $2;
        }
    | STAR
        {
            $$ = Placeholder.Singleton;
        }
;

compound_lhs_head:
      compound_lhs_head compound_lhs_item COMMA
        {
            ($$ = $1).Add($2);
        }
    | compound_lhs_item COMMA
        {
            $$ = CollectionUtils.MakeList($1);
        }
;

compound_lhs_item:
      compound_lhs_node
        {
            $$ = $1;
        }
    | LEFT_EXPR_PARENTHESIS compound_lhs RIGHT_PARENTHESIS
        {
            $$ = $2;
        }
;

compound_lhs_node: 
      variable
        {
            $$ = VariableFactory.MakeLeftValue($<VariableFactory>1, this, $<String>1, @$);
        }
    | primary LEFT_INDEXING_BRACKET array_key RIGHT_BRACKET
        {
            $$ = new ArrayItemAccess($1, $3, @$);
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
    | primary LEFT_INDEXING_BRACKET array_key RIGHT_BRACKET
        {
            $$ = new ArrayItemAccess($1, $3, @$);
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
            _tokenizer.LexicalState = LexicalState.EXPR_END;
            $$ = $1;
        }
    | reswords
        {
            _tokenizer.LexicalState = LexicalState.EXPR_END;
            $$ = $<String>1;
    }
;

method_name_or_symbol: 
      method_name
        {
            $$ = $1;
        }
    | symbol
        {
            $$ = $1;
        }
;

undef_list:
      method_name_or_symbol
        {
            $$ = CollectionUtils.MakeList<Identifier>(new Identifier($1, @1));
        }
   | undef_list COMMA 
        {
            _tokenizer.LexicalState = LexicalState.EXPR_FNAME;
        } 
     method_name_or_symbol
        {
            ($$ = $1).Add(new Identifier($4, @4));
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
    | GREATER           { $$ = Symbols.GreaterThan; }
    | GREATER_OR_EQUAL  { $$ = Symbols.GreaterEqual; }
    | LESS              { $$ = Symbols.LessThan; }
    | LESS_OR_EQUAL     { $$ = Symbols.LessEqual; }
    | LSHFT             { $$ = Symbols.LeftShift; }
    | RSHFT             { $$ = Symbols.RightShift; }
    | PLUS              { $$ = Symbols.Plus; }
    | MINUS             { $$ = Symbols.Minus; }
    | ASTERISK          { $$ = Symbols.Multiply; }
    | STAR              { $$ = Symbols.Multiply; }
    | SLASH             { $$ = Symbols.Divide; }
    | PERCENT           { $$ = Symbols.Mod; }
    | POW               { $$ = Symbols.Power; }
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
    | primary LEFT_INDEXING_BRACKET array_key RIGHT_BRACKET OP_ASSIGNMENT arg
        {
            $$ = new SimpleAssignmentExpression(new ArrayItemAccess($1, $3, @2), $6, $5, @$);
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
            $$ = new NotExpression(new MethodCall($1, Symbols.Equal, new Arguments($3), @$), @$);
        }
    | arg MATCH arg
        {
            $$ = MakeMatch($1, $3, @$);
        }
    | arg NMATCH arg
        {
            $$ = new NotExpression(MakeMatch($1, $3, @2), @$);
        }
    | BANG arg
        {
            // TODO: warning: string literal in condition
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
    | DEFINED opt_nl arg
        {
            $$ = new IsDefinedExpression($3, @$);
        }
    | arg QUESTION_MARK arg COLON arg
        {
            $$ = new ConditionalExpression(ToCondition($1), $3, $5, @$);
        }
    | primary
        {
            $$ = $1;
        }
;
        
array_key: 
      /* empty */
        {
            SetArguments();
        }
    | command opt_nl
        {
            _tokenizer.ReportWarning(Errors.ParenthesizeArguments);
            SetArguments($1);
        }
    | args trailer
        {
            PopAndSetArguments($1, null, null, null, @1);
        }
    | args COMMA STAR arg opt_nl
        {
            PopAndSetArguments($1, null, $4, null, MergeLocations(@1, @4));
        }
    | maplets trailer
        {
            SetArguments(null, $1, null, null, @1);
        }
    | STAR arg opt_nl
        {
            SetArguments(null, null, $2, null, MergeLocations(@1, @2));
        }
;

paren_args:   
      LEFT_PARENTHESIS /* empty */ RIGHT_PARENTHESIS
        {
            SetArguments();
        }
    | LEFT_PARENTHESIS open_args opt_nl RIGHT_PARENTHESIS
        {
            Debug.Assert($2.Arguments != null);
            $$ = $2;
        }
    | LEFT_PARENTHESIS block_call opt_nl RIGHT_PARENTHESIS
        {
            _tokenizer.ReportWarning(Errors.ParenthesizeArguments);
            SetArguments($2);
        }
    | LEFT_PARENTHESIS args COMMA block_call opt_nl RIGHT_PARENTHESIS
        {
            _tokenizer.ReportWarning(Errors.ParenthesizeArguments);    
            SetArguments(PopArguments($2, $4), null, null, null, @$);
        }
;

opt_paren_args: 
      /* empty */
        {
            SetNoArguments(null);
        }
    | paren_args
        {
            $$ = $1;
        }
;

open_args: 
      args opt_block_reference
        {
            PopAndSetArguments($1, null, null, $2, @$);
        }
    | args COMMA STAR arg opt_block_reference
        {
            PopAndSetArguments($1, null, $4, $5, @$);
        }
    | maplets opt_block_reference
        {
            SetArguments(null, $1, null, $2, @$);
        }
    | maplets COMMA STAR arg opt_block_reference
        {
            SetArguments(null, $1, $4, $5, @$);
        }
    | args COMMA maplets opt_block_reference
        {
            PopAndSetArguments($1, $3, null, $4, @$);
        }
    | args COMMA maplets COMMA STAR arg opt_block_reference
        {
            PopAndSetArguments($1, $3, $6, $7, @$);
        }
    | STAR arg opt_block_reference
        {
            SetArguments(null, null, $2, $3, @$);
        }
    | block_reference
        {
            SetArguments($1);
        }
    | command
        {
            _tokenizer.ReportWarning(Errors.ParenthesizeArguments);                
            SetArguments($1);
        }
;

closed_args:
      arg COMMA args opt_block_reference
        {
            SetArguments(PopArguments($1, $3), null, null, $4, @$);
        }
    | arg COMMA block_reference
        {
            SetArguments($1, $3);
        }
    | arg COMMA STAR arg opt_block_reference
        {
            SetArguments(new Expression[] { $1 }, null, $4, $5, @$);
        }
    | arg COMMA args COMMA STAR arg opt_block_reference
        {
            SetArguments(PopArguments($1, $3), null, $6, $7, @$);
        }
    | maplets opt_block_reference
        {
            SetArguments(null, $1, null, $2, @$);
        }
    | maplets COMMA STAR arg opt_block_reference
        {
            SetArguments(null, $1, $4, $5, @$);
        }
    | arg COMMA maplets opt_block_reference
        {
            SetArguments(new Expression[] { $1 }, $3, null, $4, @$);
        }
    | arg COMMA args COMMA maplets opt_block_reference
        {
            SetArguments(PopArguments($1, $3), $5, null, $6, @$);
        }
    | arg COMMA maplets COMMA STAR arg opt_block_reference
        {
            SetArguments(new Expression[] { $1 }, $3, $6, $7, @$);
        }
    | arg COMMA args COMMA maplets COMMA STAR arg opt_block_reference
        {
            SetArguments(PopArguments($1, $3), $5, $8, $9, @$);
        }
    | STAR arg opt_block_reference
        {
            SetArguments(Expression.EmptyArray, null, $2, $3, @$);
        }
    | block_reference
        {
            SetArguments($1);
        }
;

command_args:
        {
            $<Integer1>$ = _tokenizer.EnterCommandArguments();
        }
      command_args_content
        {
            _tokenizer.LeaveCommandArguments($<Integer1>1);
            $$ = $2;
        }
;

command_args_content: 
      open_args
        {
            Debug.Assert($1.Arguments != null);
            $$ = $1;
        }
    | LEFT_ARG_PARENTHESIS
        {
            _tokenizer.LexicalState = LexicalState.EXPR_ENDARG;
        }
      RIGHT_PARENTHESIS
        {
            _tokenizer.ReportWarning(Errors.WhitespaceBeforeArgumentParentheses);    
            SetArguments();
        }
    | LEFT_ARG_PARENTHESIS closed_args
        {
            _tokenizer.LexicalState = LexicalState.EXPR_ENDARG;
        }
      RIGHT_PARENTHESIS
        {
            _tokenizer.ReportWarning(Errors.WhitespaceBeforeArgumentParentheses);    
            $$ = $2;
        }
;

block_reference:
      BLOCK_REFERENCE arg
        {
            $$ = new BlockReference($2, @$);
        }
;

opt_block_reference:
      COMMA block_reference
        {
            $$ = $2;
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
    | args COMMA arg
        {
            PushArgument($1, $3);
        }
;

primary: 
      numeric_literal
    | symbol
        {
            $$ = new SymbolLiteral($1, @$);
        }
    | immutable_string
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
    | primary LEFT_INDEXING_BRACKET array_key RIGHT_BRACKET
        {
            $$ = new ArrayItemAccess($1, $3, @$);
        }
    | LEFT_BRACKET array_key RIGHT_BRACKET
        {
            $$ = new ArrayConstructor($2, @$);
        }
    | LEFT_BRACE RIGHT_BRACE
        {
            $$ = new HashConstructor(null, null, @$);
        }
    | LEFT_BRACE maplets trailer RIGHT_BRACE
        {
            $$ = new HashConstructor($2, null, @$);
        }
    | LEFT_BRACE args trailer RIGHT_BRACE
        {
            $$ = new HashConstructor(null, PopHashArguments($2, @3), @$);
        }                        
    | YIELD LEFT_PARENTHESIS open_args RIGHT_PARENTHESIS
        {
            $$ = new YieldCall(RequireNoBlockArg($3), @$);
        }
    | YIELD LEFT_PARENTHESIS RIGHT_PARENTHESIS
        {
            $$ = new YieldCall(Arguments.Empty, @$);
        }
    | YIELD
        {
            $$ = new YieldCall(null, @1);
        }
    | DEFINED opt_nl LEFT_PARENTHESIS expr RIGHT_PARENTHESIS
        {
            $$ = new IsDefinedExpression($4, @$);
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
    | FOR block_parameters IN
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
            $$ = new ForLoopExpression(CurrentScope, $2, $5, $8, @$);
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
      opt_nl RIGHT_PARENTHESIS
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
            EnterTopScope();
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
            EnterTopScope();
        }
      body END
        {
            _inInstanceMethodDefinition = $<Integer1>4;
            _inSingletonMethodDefinition = $<Integer1>6;
            $$ = new SingletonDefinition(LeaveScope(), $3, $7, @$);
        }
    | MODULE qualified_module_name
        {
            EnterTopScope();
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
            EnterTopScope();
        }
      parameters_definition body END
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
            _tokenizer.LexicalState = LexicalState.EXPR_END;
            EnterTopScope();
        }
      parameters_definition body END
        {
            _inSingletonMethodDefinition--;
            $$ = new MethodDefinition(CurrentScope, $2, $5, $7, $8, @$);
            LeaveScope();
        }
;

body: 
      compstmt rescue_clauses_opt else_opt ensure_opt
        {
            $$ = MakeBody($1, $2, $3, @3, $4, @$);
        }
;

case_expression:
      CASE expr opt_terms when_clauses else_opt END
        {
            $$ = new CaseExpression($2, $4, $5, @$);
        }
    | CASE opt_terms when_clauses else_opt END
        {
            $$ = new CaseExpression(null, $3, $4, @$);
        }
    | CASE opt_terms ELSE compstmt END
        {
            $$ = new CaseExpression(null, null, new ElseIfClause(null, $4, @$), @$);
        }   
;

then:
      term
    | COLON
    | THEN
    | term THEN
;

do: 
      term
    | COLON
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

block_parameters:
      lhs 
        { 
            $$ = new CompoundLeftValue(CollectionUtils.MakeList<LeftValue>($1), null, @1); 
        }
    | compound_lhs 
        { 
            $$ = $1; 
        }
;

block_parameters_opt:
      /* empty */
        {
            $$ = CompoundLeftValue.UnspecifiedBlockSignature;
        }
    | PIPE PIPE 
        {
            $$ = CompoundLeftValue.EmptyBlockSignature;
        }
    | LOGICAL_OR
        {
            $$ = CompoundLeftValue.EmptyBlockSignature;
        }
    | PIPE block_parameters PIPE
        {
            $$ = $2;
        }
;

do_block: 
      BLOCK_DO
        {
            EnterNestedScope();
        }
      block_parameters_opt compstmt END
        {
            $$ = new BlockDefinition(CurrentScope, $3, $4, @$);
            LeaveScope();
        }
;

block_call: 
      command do_block
        {                            
            SetBlock($$ = $1, $2);
        }
    | block_call DOT operation2 opt_paren_args
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
    | block_call SEPARATING_DOUBLE_COLON operation2 opt_paren_args
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
;

method_call: 
      operation paren_args
        {
            $$ = MakeMethodCall(null, $1, $2, @$);
        }
    | primary DOT operation2 opt_paren_args
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
    | primary SEPARATING_DOUBLE_COLON operation2 paren_args
        {
            $$ = MakeMethodCall($1, $3, $4, @$);
        }
    | primary SEPARATING_DOUBLE_COLON operation3
        {
            $$ = new MethodCall($1, $3, null, @3);
        }
    | SUPER paren_args
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
            $$ = new BlockDefinition(CurrentScope, $3, $4, @$);
            LeaveScope();
        }
    | DO
        {
            EnterNestedScope();    
        }
      block_parameters_opt compstmt END
        {
            $$ = new BlockDefinition(CurrentScope, $3, $4, @$);
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
      WHEN when_args then compstmt
         {
             $$ = MakeWhenClause($2, $4, @4);
         }
;
            
when_args: 
      args
        {
            SetWhenClauseArguments($1, null);
        }
    | args COMMA STAR arg
        {
            SetWhenClauseArguments($1, $4);
        }
    | STAR arg
        {
            SetWhenClauseArguments(0, $2);
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
            $$ = new RescueClause($2, $4, @$);        
        }
    | RESCUE arg exc_var then compstmt
        {
            $$ = new RescueClause($2, $3, $5, @$);        
        }
    | RESCUE compound_rhs exc_var then compstmt
        {
            $$ = new RescueClause($2, $3, $5, @$);        
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
      string
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

immutable_string:
      SYMBOL_BEGIN string_contents STRING_END
        {
            $$ = MakeSymbolConstructor($2, @$);
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
            $$ = new ArrayConstructor(new Arguments($2.ToArray(), null, null, @2), @$);
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
            $$ = MakeVerbatimWords($2, @2, @$);
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
            _tokenizer.LeaveParenthesisedExpression();
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

symbol:
    SYMBOL_BEGIN sym
      {
          _tokenizer.LexicalState = LexicalState.EXPR_END;
          $$ = $2;
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

parameters_definition:
      LEFT_PARENTHESIS parameters opt_nl RIGHT_PARENTHESIS
          {
              $$ = $2;
              _tokenizer.LexicalState = LexicalState.EXPR_BEG;
          }
    | parameters term
        {
            $$ = $1;
        }
;

parameters: 
      parameter_list COMMA default_parameter_list COMMA array_parameter block_parameter_opt
        {
            $$ = new Parameters($1, $3, $5, $6, @$);
        }
    | parameter_list COMMA default_parameter_list block_parameter_opt
        {
            $$ = new Parameters($1, $3, null, $4, @$);
        }
    | parameter_list COMMA array_parameter block_parameter_opt
        {
            $$ = new Parameters($1, null, $3, $4, @$);
        }
    | parameter_list block_parameter_opt
        {
            $$ = new Parameters($1, null, null, $2, @$);
        }
    | default_parameter_list COMMA array_parameter block_parameter_opt
        {
            $$ = new Parameters(null, $1, $3, $4, @$);
        }
    | default_parameter_list block_parameter_opt
        {
            $$ = new Parameters(null, $1, null, $2, @$);
        }
    | array_parameter block_parameter_opt
        {
            $$ = new Parameters(null, null, $1, $2, @$);
        }
    | block_parameter
        {
            $$ = new Parameters(null, null, null, $1, @$);
        }
    | /* empty */ 
        {
            $$ = new Parameters(null, null, null, null, @$);
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

parameter_list: 
      parameter
        {
            $$ = CollectionUtils.MakeList<LocalVariable>($1);
        }
    | parameter_list COMMA parameter
        {
            ($$ = $1).Add($3);
        }
;

default_parameter: 
      parameter ASSIGNMENT arg
        {        
            $$ = new SimpleAssignmentExpression($1, $3, null, @$);
        }
;

default_parameter_list: 
      default_parameter
        {
            $$ = CollectionUtils.MakeList<SimpleAssignmentExpression>($1);
        }
    | default_parameter_list COMMA default_parameter
        {
            ($$ = $1).Add($3);
        }
; 

array_parameter_mark: 
      ASTERISK
    | STAR
;

array_parameter: 
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
     expr opt_nl RIGHT_PARENTHESIS
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

opt_terms: 
      /* empty */
    | terms
;

opt_nl:
      /* empty */
    | NEW_LINE
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
