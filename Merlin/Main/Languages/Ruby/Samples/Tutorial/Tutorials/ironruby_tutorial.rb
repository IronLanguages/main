# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require "tutorial"

# All strings use the RDoc syntax documented at 
# http://www.ruby-doc.org/stdlib/libdoc/rdoc/rdoc/index.html

tutorial "IronRuby tutorial" do

    introduction %{
        IronRuby is the CLI[http://www.ecma-international.org/publications/standards/Ecma-335.htm]
        implementation of the {Ruby programming language}[http://www.ruby-lang.org/]. It's a dynamically 
        typed language with support for many programming paradigms such as object-oriented programming, 
        and also allows you to seamlessly use CLI code. 

        The goal of this tutorial is to quickly familiarize you with using IronRuby interactively, and to 
        show you how to make use of the extensive CLI libraries available.  This tutorial also shows you 
        how to get started in more specialized areas such as interoperating with COM, and embedding 
        IronRuby.
        
        You can find more resources about IronRuby at http://ironruby.net.}

    section "Introduction" do
    
        introduction %{
            The objective of this chapter is to explain the basic usage of the IronRuby interactive interpreter.

            Estimated time to complete this section : <b>5 minutes</b>}

        chapter "The REPL window" do
            introduction %{
                This chapter explains the basic usage of a REPL window. REPL is an acronym for 
                <b>R</b>ead, <b>E</b>val, <b>P</b>rint, <b>L</b>oop. One of the big advantages of dynamic languages
                is the ability to do interactive exporation of new APIs and libraries from a REPL 
                window. You can enter expressions using the API you are exploring, and the results
                are immediately displayed. Depending on the result, you can chose to try different
                expressions. You can thus build programs in this fashion while avoiding a
                compile step after every operation.}

            task :body => %{
                    Let's start with a simple expression to add two numbers. Enter the expression below,
                    followed by the _Enter_ key. The expression and its result will be shown in the output 
                    window below the text-box where you enter the expression.
                  },
                  :code => '2 + 2'

            task(:body => "Now let's do some printing. This is done with the puts function.",
                 :code => "puts 'Hello world'") { |i| i.output =~ /Hello world/i }

            task(:body => "Let's use a local variable.",
                 :code => "x = 1") { |i| eval "x == 1", i.bind }

            task(:body => "And then print the local variable.",
                 :code => "puts x") { |i| i.output.chomp == "1" }
        end
        
        chapter "Multi-line statements" do
            introduction "This chapter explains how multi-line statements can be used in the tutorial"
            task(
              :body => %{
                Entering multiple lines in an interactive console is a bit tricky as it can be ambigous when you are done 
                entering a statement. When you press the +Enter+ key, you may either be expecting to execute the code you have
                typed already, or you may want to enter more code. Also, sometimes you might want to go back and edit a line
                above.
                
                The tutorial currently only handles single line input. Use <tt>;</tt> to separate statements},
              :code => "if 2 < 3 then puts 'this'; puts 'that' end"
              ) { |interaction| interaction.output =~ /this\nthat/ }
        end
    end

    section "Ruby" do
        introduction "Basic language features"
        
        chapter "String" do
            introduction "This chapter shows common ways of working with strings" 
            
            task :body => "Strings can be declared using either single or double quotes.",
                 :code => "'hello'"

            task :body => "The +index+ method allows you to search for a substring.",
                 :code => "'hello there'.index('there')"
        end
        
        chapter "Array" do
            introduction "This chapter shows common ways of working with arrays" 
            
            task :body => "Arrays can be declared using square brackets.",
                 :code => "[1, 2, 3]"

            task :body => "Arrays can contain data of any type.",
                 :code => "[1, nil, 'hello', []]"
        end
    end

    section "CLR" do
        introduction "CLR interop features"
        
        chapter "mscorlib" do
            introduction "This chapter shows how to use the CLR namespaces and types"

            task :body => "The core CLR library mscorlib.dll is always automatically loaded in IronRuby. You can inspect the +System+ namespace in it.",
                 :code => "System"
        end
    end

    section "Advanced CLR" do
        introduction "Advanced CLR interop features"
    end

    section "COM" do
        introduction "COM interop features"
    end

    section "Embedding IronRuby" do
        introduction "Embedding IronRuby in a host app to make it scriptable"
        chapter "Placeholder" do
            introduction "Placeholder"
            task :body => "Placeholder", :code => "2 + 2"
        end
    end
   
    summary %{
           Congratulations! You have completed the IronRuby tutorial. 
           
           For more information about IronRuby, please visit http://ironruby.net.}
end
