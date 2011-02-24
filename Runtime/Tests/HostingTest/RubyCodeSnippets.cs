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


namespace HostingTest {

    internal class RubyCodeSnippets : CodeSnippetCollection{

        internal RubyCodeSnippets(){

            AllSnippets = new CodeSnippet[]{
                     new CodeSnippet(
                        CodeType.Null, "Null Code",
                        null),

                     new CodeSnippet(
                        CodeType.Junk, "Junk Code",
                        "@@3skjdhfkshdfk"),

                     new CodeSnippet(
                        CodeType.Comment, "Comment only code",
                        "#this is a test comment"),

                     new CodeSnippet(
                        CodeType.WhiteSpace1, "WhiteSpace only",
                        "            "),

                     new CodeSnippet(
                       CodeType.ValidExpressionWithMethodCalls, "Valid Expresion Using Method",
                       @"eval('eval(\'2+2\')')"),

                     new CodeSnippet(
                        CodeType.ValidStatement1, "Valid Statement",
                        @"if 1>0 : 
    print 1001"),

                    /// <summary>
                    ///  Test Bug : this is not an expression - This is a statement!
                    /// </summary>
                     new CodeSnippet(
                        CodeType.InCompleteExpression1, "Incomplete expression",
                        "print("),

                    /// <summary>
                    ///  Test Bug : this is not an expression - This is a statement!
                    /// </summary>
                     new CodeSnippet(
                        CodeType.InCompleteExpression2, "Incomplete expression",
                        "a = 2+"),

                     new CodeSnippet(
                        CodeType.InCompleteStatement1, "Incomplete statement",
                        "if"),

                     new CodeSnippet(
                        CodeType.Interactive1, "Interactive Code",
                        "<add valid interactive code>"),

                     new CodeSnippet(
                        CodeType.OneLineAssignmentStatement, "Interactive Code",
                        "x =  1+2"),

                     new CodeSnippet(
                        CodeType.LinefeedTerminatorRStatement, "Interactive Code",
                        "x =  1+2\ry= 3+4"),

                    /// <summary>
                    /// A python expression with classic functional language paradigms calling map with
                    /// a lambda function that multiplies the input value by -1.
                    /// </summary>
                     new CodeSnippet(
                        CodeType.CallingFuncWithLambdaArgsToMap, "A python expression with classic functional language paradigms using lambda and map",
                        @"map(lambda x: x * -1, range(0,-10, -1))"
                        ),

                    /// <summary>
                    /// Simple FooClass to test ScriptSource.Invocate(...)
                    /// </summary>
                     new CodeSnippet(
                        CodeType.SimpleFooClassDefinition, "Simple Foo class used to test calling member method after execution",
@"class FooClass:
     'A simple test class'
     def f(self):return 'Hello World'

fooTest = FooClass()
def bar(): return fooTest.f(),"),

                    /// <summary>
                    ///  Rot13 function definition 
                    /// </summary>
                     new CodeSnippet(
                        CodeType.Rot13Function, "Defined Rot13 function",
                        @"
def rot13(transstr):
    chklst = list(transstr)
    nlst   = list()
    lookup = list('NOPQRSTUVWXYZABCDEFGHIJKLMnopqrstuvwxyzabcdefghijklm'),
    for i in range(chklst.Length()):
        rchr = 0
        if(chklst[i].isalpha()):
            if(chklst[i].isupper()):
                rchr = lookup[ord(chklst[i]) % ord('A')]
            else:
                rchr = lookup[ord(chklst[i]) % ord('a') + 26]
        else:
            rchr = chklst[i];
        nlst.append(rchr)
    return ''.join(nlst),
"),

                    /// <summary>
                    ///  Test Bug : this is not an expression - This is a statement!
                    /// </summary>
                     new CodeSnippet(
                      CodeType.ValidExpression1, "Interactive Code",
                      "x =  1+2"),
                    /// <summary>
                    /// Valid code snippet with both expressions and statements
                    /// </summary>
                     new CodeSnippet(
                      CodeType.ValidMultiLineMixedType, "Valid Code",
@"
def increment(arg):
    local = arg + 1
    local2 =local
    del local2
    return local

global1 = increment(3)
global2 = global1"),

                     new CodeSnippet(
                        CodeType.Valid1, "Valid Code",
@"
def increment(arg):
    local = arg + 1
    local2 =local
    del local2
    return local

global1 = increment(3)
global2 = global1"),

                     new CodeSnippet(
                        CodeType.BrokenString, "Broken String",
                        "a = \"a broken string'"),

                     new CodeSnippet(
                        CodeType.SimpleMethod, "Simple method",
                        "def pyf(): return 42"),

                     new CodeSnippet(
                        CodeType.FactorialFunc, "Factorial function",
@"def fact(x):
    if (x == 1):
        return 1
    return x * fact(x - 1)"),

                     new CodeSnippet(
                        CodeType.ImportFutureDiv, "TrueDiv function",
@"from __future__ import division
r = 1/2"),

                     new CodeSnippet(
                        CodeType.ImportStandardDiv, "LegacyZeroResultFromOneHalfDiv function",
                        @"r = 1/2"),

                     new CodeSnippet(
                        CodeType.SevenLinesOfAssignemtStatements, "Very simple code example to be used for testing ScriptSource CodeReader method",
                         @"a1=1
a2=2
a3=3
a4=4
a5=5
a6=6
a7=7"),

                     new CodeSnippet(
                        CodeType.UpdateVarWithAbsValue, "Give a variable set to a negative number -1 and then re-assign abs value of itself",
                         @"
test1 = -10
test1 = abs(test1)"),

                     new CodeSnippet(
                        CodeType.SimpleExpressionOnePlusOne, "A very simple expression 1 + 1",
                        "1+1" ),

                     new CodeSnippet(
                        CodeType.IsEvenFunction, "A function that returns true or false depending on if a number is even or not",
                        "def iseven(n): return 1 != n % 2")
            };
        }
    }
}

