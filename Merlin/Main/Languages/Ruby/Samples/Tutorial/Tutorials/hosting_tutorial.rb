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

module HostingTutorial
  def self.ensure_script_method
    task(:run_unless => lambda { |bind| bind.script('2+2') == 4 },
         :body => %{
             This chapter uses the method +script+ that was defined in the first chapter. Enter the
             following commands, or go through the first chapter first.
         },
         :code => [
            '$engine = IronRuby.create_engine',
            '$scope = $engine.create_scope',
            'def script(script_code) $engine.execute(script_code, $scope) end']
         ) { |iar| iar.bind.script('2+2') == 4 }
  end
  
  class RedirectingOutputStream < System::IO::Stream
    def can_seek
     false
    end
    
    def can_read
     false
    end
  
    def can_write
     true
    end
    
    # TODO - This does not deal with any encoding issues
    def write(buffer, offset, count)
      char_array = System::Array[System::Char].new(buffer.length)
      buffer.each_index { |idx| char_array[idx] = buffer[idx] }
      # Do the actual write. Note that this will automatically honor $stdout redirection 
      # of the ScriptEngine of the tutorial application.
      print System::String.clr_new(char_array, offset, count)
    end
  end
end

load_assembly "Microsoft.Scripting"

module Microsoft
  module Scripting
    module Hosting
      class ScriptEngine
        def redirect_output
          stream = HostingTutorial::RedirectingOutputStream.new
          # TODO - This does not deal with any encoding issues
          self.runtime.i_o.set_output(stream, System::Text::UTF8Encoding.new)
        end
      end
    end
  end
end

# All strings use the RDoc syntax documented at 
# http://www.ruby-doc.org/stdlib/libdoc/rdoc/rdoc/index.html

tutorial "IronRuby Hosting tutorial" do

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
        One of the top DLR features is common hosting support for all languages implemented on the DLR. The 
        primary goal is supporting .NET applications hosting the DLR languages for scripting support so that
        users can extend the basic functionality of the host application using any (DLR) language of their
        choice, irrespective of the programming language used to implement the host appplication.
    }

    section "Hosting" do
        introduction %{
            A quick survey of functionality includes:
            * Create ScriptRuntimes locally or in remote app domains.
            * Execute snippets of code.
            * Execute files of code in their own execution context (ScriptScope).
            * Explicitly choose language engines to use or just execute files to let the DLR find the right engine.
            * Create scopes privately or publicly for executing code in.
            * Create scopes, set variables in the scope to provide host object models, and publish the scopes for dynamic languages to import, require, etc.
            * Create scopes, set variables to provide object models, and execute code within the scopes.
            * Fetch dynamic objects and functions from scopes bound to names or execute expressions that return objects.
            * Call dynamic functions as host command implementations or event handlers.
            * Get reflection information for object members, parameter information, and documentation.
            * Control how files are resolved when dynamic languages import other files of code.
    
            Hosts always start by calling statically on the ScriptRuntime to create a ScriptRuntime. In the 
            simplest case, the host can set globals and execute files that access the globals.  In more advanced 
            scenarios, hosts can fully control language engines, get services from them, work with compiled code, 
            explicitly execute code in specific scopes, interact in rich ways with dynamic objects from the 
            ScriptRuntime, and so on.

            A detailed specification of the hosting APIs is available at 
            http://www.codeplex.com/dlr/Wiki/View.aspx?title=Docs%20and%20specs.
        }
        
        chapter "Getting started" do

            task(:body => %{
                    We first need to create a language "engine" using the <tt>IronRuby.create_engine</tt>
                    method. This name is available only when doing hosting from IronRuby _itself_. If you
                    are using a different language to implement the host, the method name would be
                    <tt>IronRuby.Ruby.CreateEngine</tt>. You will also have to make sure to add a 
                    reference to IronRuby.dll. This is done using the <tt>/r</tt> command-line compiler
                    option for C# and VB.Net, or using the <tt>clr.AddReference</tt> method from IronPython.
                },
                :setup => lambda do |bind|
                    load_assembly "Microsoft.Scripting"
                    eval "engine = '(undefined)'", bind
                    eval "scope = '(undefined)'", bind
                    eval "def script() 'undefined' end", bind
                end,
                :code => 'engine = IronRuby.create_engine'
              ) { |iar| iar .bind.engine.redirect_output; true }
                
            task(:body => %{
                    Next we have to create a scope.
                },
                :code => 'scope = engine.create_scope'
                ) { |iar| iar.bind.scope.kind_of? Microsoft::Scripting::Hosting::ScriptScope }
                
            task :body => %{
                    Now let's execute some code.
                },
                :code => "engine.execute('$x = 100', scope)"

            task :body => %{
                    What did that do? Let's read back the value of <tt>$x</tt> to make sure it is set
                    as expected.
                },
                :code => "engine.execute('$x', scope)"
                
            task(:body => %{
                    We can verify that the code ran in a separate context by checking if <tt>$x</tt> exists 
                    in the current context.
                },
                :code => 'puts $x'
                ) { |iar| iar.output.chomp == 'nil' }
                
            task(:body => %{
                    Since typing <tt>engine.execute...</tt> gets verbose, let's define a method called
                    +script+ to encapsulate it. The name also represents the fact that the String parameter
                    it expects is conceptually arbitrary script code that the user can type. 
                    
                    Since Ruby methods cannot access outer local variables, we will need to store +engine+ 
                    and +scope+ as global variables first.
                },
                :code => [
                    '$engine = engine',
                    '$scope = scope',
                    'def script(script_code) $engine.execute(script_code, $scope) end']
                ) { |iar| iar.bind.script('$x') == 100 }
                
            task(:body => %{
                    We will use +script+ throughout the rest of the tutorial. Let's try it now to print the 
                    value of +$x+.
                },
                :code => "script 'puts $x'"
                ) { |iar| iar.output.chomp == '100' }
        end

        chapter "Exchanging variables with global variables" do
            introduction %{
                Running user script code gets more interesting if the host application can set variables
                that the user code can use. The variables will typically be set to the object model of the
                host application. The tutorial application you are using stores the tutorials as
                <tt>Tutorial::Tutorial.all</tt>. We will use this object model in this chapter.
            }
                
            HostingTutorial.ensure_script_method

            task(:body => %{
                    Let's set a Ruby global variable called <tt>Tutorials</tt>. The name should be a valid
                    constant name (ie. it should begin with an upper case letter)
                },
                :code => "engine.runtime.globals.set_variable 'Tutorials', ::Tutorial.all"
                ) { |iar| iar.bind.engine.runtime.globals.get_variable('Tutorials') == Tutorial.all }
                
            task(:body => %{
                    Now the user's script code has access to it! Let's have the user check how many tutorials there are.
                },
                :code => "script 'Tutorials.size'"
                ) { |iar| iar.result == Tutorial.all.size }
                
            task(:body => %{
                    This works in the reverse direction too. The script code can set global variables that
                    the host application can read back out.
                },
                :code => [
                    "script 'ThisIsScriptCode = 200', 'Place-holder string'",
                    "engine.runtime.globals.get_variable('ThisIsScriptCode')"]
                ) { |iar| iar.result == 200 }
        end

        chapter "Per-scope variables" do
            introduction %{
                Setting global variables is fine when all user script code needs access to the same shared
                values. However, often, you will want to scope a value to a narrower context. For example,
                for a tutorial defined in <tt>name_tutorial.rb</tt>, you might want to load files in a folder
                called +name_scripts+, and set +tutorial+ to point to the tutorial created by 
                <tt>name_tutorial.rb</tt>.
            }
            
            HostingTutorial.ensure_script_method
                    
            task(:body => %{
                    Let's create a second scope
                },
                :code => 'scope2 = $engine.create_scope'
                ) { |iar| iar.bind.scope2 }
                
            task(:body => %{
                    Now we will set a variable named +tutorial+ in each of the scopes.
                },
                :code => [
                    "$scope.set_variable 'tutorial', ::Tutorial.all.values[0]",
                    "scope2.set_variable 'tutorial', ::Tutorial.all.values[1]"]
                ) { |iar| iar.bind.scope2.get_variable 'tutorial' }
                
            task :body => %{
                    Now we can execute the same script code in the two scopes.
                },
                :code => [
                    "script 'tutorial.name'",
                    "$engine.execute 'tutorial.name', scope2"]
        end
    end
    
    section "Hello IronPython!" do
    
        introduction %{
            So far, we have hosted IronRuby within IronRuby. Now we will host IronPython from IronRuby
            to show that it really is easy to host multiple languages, and the host application
            does not have to change much to accomodate multiple languages.
        }
        
        chapter "Hello IronPython!" do
        
            task :body => %{
                    Coming soon
                 },
                 :code => '2+2'
        end
    end
   
    summary %{
        Congratulations! You have completed the Hosting tutorial. 
           
        For more information about the DLR and the DLR Hosting APIs, please visit http://www.codeplex.com/dlr/.
    }
end

