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

module IronRubyTutorial
  def self.files_path
    File.dirname(__FILE__) + '/ironruby_files'
  end
  
  def self.primes_path
    files_path + '/primes.rb'
  end
end

$LOAD_PATH << IronRubyTutorial.files_path

# All strings use the RDoc syntax documented at 
# http://www.ruby-doc.org/stdlib/libdoc/rdoc/rdoc/index.html

tutorial "IronRuby tutorial" do

    legal %{
        Information in this document is subject to change without notice. The example companies,
        organizations, products, people, and events depicted herein are fictitious. No association with any
        real company, organization, product, person or event is intended or should be inferred. Complying with
        all applicable copyright laws is the responsibility of the user. Without limiting the rights under
        copyright, no part of this document may be reproduced, stored in or introduced into a retrieval
        system, or transmitted in any form or by any means (electronic, mechanical, photocopying, recording,
        or otherwise), or for any purpose, without the express written permission of Microsoft Corporation.

        Microsoft may have patents, patent applications, trademarked, copyrights, or other intellectual
        property rights covering subject matter in this document. Except as expressly provided in any written
        license agreement from Microsoft, the furnishing of this document does not give you any license to
        these patents, trademarks, copyrights, or other intellectual property.

        (c) Microsoft Corporation. All rights reserved.

        Microsoft, MS-DOS, MS, Windows, Windows NT, MSDN, Active Directory, BizTalk, SQL Server, SharePoint,
        Outlook, PowerPoint, FrontPage, Visual Basic, Visual C++, Visual J++, Visual InterDev, Visual
        SourceSafe, Visual C#, Visual J#,  and Visual Studio are either registered trademarks or trademarks of
        Microsoft Corporation in the U.S.A. and/or other countries.

        Other product and company names herein may be the trademarks of their respective owners
    }
    
    introduction %{
        IronRuby is the .NET[http://www.ecma-international.org/publications/standards/Ecma-335.htm]
        implementation of the {Ruby programming language}[http://www.ruby-lang.org/]. It's a dynamically 
        typed language with support for many programming paradigms such as object-oriented programming, 
        and also allows you to seamlessly use CLI code. 

        The goal of this tutorial is to quickly familiarize you with using IronRuby interactively, and to 
        show you how to make use of the extensive .NET libraries available.  This tutorial also shows you 
        how to get started in more specialized areas such as interoperating with COM.
        
        You can find more resources about IronRuby at http://ironruby.net.
    }

    section "Basic IronRuby - Introduction to the IronRuby interactive window" do
    
        introduction %{
            The objective of this tutorial is to launch the IronRuby interpreter, explore the environment
            of the interactive console and use IronRuby to interact with .NET libraries.
        }

        chapter "The interactive REPL window" do
            introduction %{
                This chapter explains the basic usage of a REPL window. REPL is an acronym for 
                <b>R</b>ead, <b>E</b>val, <b>P</b>rint, <b>L</b>oop. One of the big advantages of dynamic languages
                is the ability to do interactive exporation of new APIs and libraries from a REPL 
                window. You can enter expressions using the API you are exploring, and the results
                are immediately displayed. Depending on the result, you can chose to try different
                expressions. You can thus build programs in this fashion while avoiding a
                compile step after every operation.
            }

            task :body => %{
                    Let's start with a simple expression to add two numbers. Enter the expression below,
                    followed by the +Enter+ key. The expression and its result will be shown in the output 
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
                Entering multiple lines in an interactive console is a bit tricky as it can be ambigous when
                you are done entering a statement. When you press the +Enter+ key, you may either be expecting
                to execute the code you have typed already, or you may want to enter more code. Also,
                sometimes you might want to go back and edit a line above.
                
                The tutorial currently only handles single line input. Use <tt>;</tt> to separate statements
              },
              :code => "if 2 < 3 then puts 'this'; puts 'that' end"
              ) { |interaction| interaction.output =~ /this\nthat/ }
        end
        
        chapter "Built-in modules and interactive exploration" do
        
            task :body => %{
                     You can ask any object for the list of methods it supports. To see all the methods
                     available on a string, try this.
                 },
                 :code => "'Hello'.methods.sort"
            
            task :body => %{
                     To reduce the noise of methods that all objects respond to, lets filter out the methods
                     defined on the +Object+ class.
                 },
                 :code => "('Hello'.methods - Object.instance_methods).sort"
            
            task :body => %{
                     All loaded classes are exposed as constants in the +Object+ class. Let's take a look
                     at all the classes currently loaded.
                 },
                 :code => 'Object.constants.sort'
            
            task(:body => %{
                     IronRuby comes with several built-in modules. Some are loaded when IronRuby starts up
                     as you saw above. Some need to be explicitly loaded. This is done with the the +require+
                     function. Let's load the +thread+ module.
                 },
                 :code => "require 'thread'") { $LOADED_FEATURES.include? 'thread.rb' }
                 
            task :body => %{
                     Now let's see which new classes were loaded. Can you spot the new classes using
                     <tt>Object.constants.sort</tt> again? +Mutex+ is one of them. There are three others.
                 },
                 :code => 'Object.constants.sort'

            task :body => %{
                     You can see the methods of a class using methods like +public_methods+.
                 },
                 :code => 'Object.public_methods.sort'
        end

        chapter "User-defined modules" do
        
            task(:body => %{
                     This chapter uses the file <tt>primes.rb</tt>. Let's load it using the +require+
                     function. The +require+ function accepts relative as well as absolute paths. A file
                     extension can be specified, or it can be left out. All of the following statements are
                     equivalent.
                     
                       require 'primes'
                       require 'primes.rb'
                       require './primes.rb'                     
                 },
                 :source_files => IronRubyTutorial.primes_path,
                 :code => "require 'primes.rb'") { $LOADED_FEATURES.include? 'primes.rb' }

            task :body => %{
                    We know that the file defines a module called +Primes+. Let's explore the methods defined
                    in the module using the +method+ function. By default, this method shows all the methods
                    available on the class, including those defined by +Object+. Since we are not interested
                    in the methods defined by +Object+, we pass an argument of +false+ to exclude
                    methods defined by superclasses.
                },
                :code => 'Primes.methods(false)'

            task :body => %{
                    Now let's call the +is_prime+ method.
                },
                :code => 'Primes.is_prime(10)'
        end
            
    end

    section "Basic IronRuby - Using the standard .NET libraries" do
    
        introduction %{
            The power of IronRuby lies within the ability to seamlessly access the wealth of .NET libraries.
            This exercise will demonstrate how the .NET libraries can be used from IronRuby .
        }
        
        chapter "Basic .NET library use" do
            task :body => %{
                    IronRuby automatically loads mscorlib.dll, the core .NET library where many of the
                    basic types are defined. .NET namespaces behave like Ruby modules. Let's look at all the
                    types and sub-namespaces defined in the +System+ namespace.
                },
                :code => 'System.constants'

            task :body => %{
                    Explore the <tt>System.Environment</tt> class.                    
                },
                :code => 'System::Environment.methods(false).sort'

            task :body => %{
                    Let's call the +OSVersion+ property.
                },
                :code => 'System::Environment.OSVersion'

            task :body => %{
                    You can assign the class names to local constants for easier access.
                },
                :code => 'E = System::Environment'

            task :body => %{
                    Now try just <tt>E.OSVersion</tt> instead of having to say 
                    <tt>System::Environment.OSVersion</tt>
                },
                :code => 'E.OSVersion'

            task :body => %{
                    You can also use the +include+ method to import contents of a class or namespace. This
                    will allow direct access to all the classes.
                },
                :code => 'include System'

            task :body => %{
                    Now you have direct access to all the classes and sub-namespaces under +System+. For
                    example, <tt>System::Environment</tt> is now directly accessible.
                },
                :code => 'Environment.OSVersion'
        end

        chapter "Working with .NET classes" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end

        chapter "Generics" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end
    end

    section "Basic IronRuby - Loading .NET libraries" do
        introduction %{
            Template
        }
        
        chapter "Using System.Xml - load_assembly" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end

        chapter "Using System.Xml - require" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end

        chapter "Loading .NET libraries from a given path" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end
    end

    section "Advanced IronRuby - Events and delegates" do
        introduction %{
            Template
        }
        
        chapter "File System watcher" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end

        chapter "Improving the event handler" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end

        chapter "Defining events in IronRuby" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end
    end

    section "Advanced IronRuby - Windows Forms" do
        introduction %{
            Template
        }
        
        chapter "Simple Windows Forms application" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end
    end

    section "Advanced IronRuby - Windows Presentation Foundation" do
        introduction %{
            Template
        }
        
        chapter "Simple WPF application" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end

        chapter "WPF calculator" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end
    end

    section "COM Interoperability - Using Microsoft Word" do
        introduction %{
            Template
        }
        
        chapter "Checking spelling" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end

        chapter "Use Windows Form Dialog to Correct Spelling" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end

    end

    section "COM Interoperability - Using Microsoft Excel" do
        introduction %{
            Template
        }
        
        chapter "Template" do
            introduction %{
                Template
            }

            task :body => %{
                    Template
                },
                :code => '2+2'
        end
    end

   
    summary %{
        Congratulations! You have completed the IronRuby tutorial. 
           
        For more information about IronRuby, please visit http://ironruby.net.
    }
end

