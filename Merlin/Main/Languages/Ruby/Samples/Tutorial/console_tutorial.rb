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
require "stringio"

class ConsoleApp
	attr :tutorial
	
    def initialize(tutorial = nil, inp = $stdin, out = $stdout)
        @in = inp
        @out = out
        if tutorial
            @tutorial = tutorial
        else
            @tutorial = Tutorial.get_tutorial
        end
    end
    
    def run_chapter chapter
        @out.puts "---------------------"
        @out.puts "Starting #{chapter.name}"
        @out.print chapter.introduction
        scope = Object.new
        bind = scope.instance_eval { binding }
        chapter.tasks.each do |task|
            @out.puts task.description
            @out.puts "Enter the following code:"
            @out.puts task.code
            begin
                @out.print "> "
                if @in.eof? then raise "No more input..." end
                input = @in.gets
                begin
                    output = StringIO.new
                    old_stdout, $stdout = $stdout, output
                    result = nil
                    result = eval(input, bind)
                rescue => e
                    @out.puts output.string if not output.string.empty?
                    @out.puts e.to_s
                else
                    @out.puts output.string if not output.string.empty?
                    @out.puts "=> #{result.inspect}"
                ensure
                    $stdout = old_stdout
                end
            end until task.success?(Tutorial::InteractionResult.new(bind, output.string, result, e))
        end
        @out.puts "Chapter completed successfully!"
        @out.puts
    end
    
    def run_section section
        @out.puts "======================"
        @out.puts "Starting #{section.name}"
        @out.puts section.introduction
        loop do
            section.chapters.each_index { |i| @out.puts "#{i + 1}: #{section.chapters[i].name}" }
            @out.print "Select a chapter number and press enter (0 to return to main menu):"
            input = @in.gets
            break if input == nil or input.chomp == "0" or input.chomp == ""
            chapter = section.chapters[input.to_i - 1]
            run_chapter chapter
        end
    end
    
    def run    
        @out.puts "Welcome to #{@tutorial.name}"
        @out.puts @tutorial.introduction
        loop do
            @tutorial.sections.each_index { |i| @out.puts "#{i + 1}: #{@tutorial.sections[i].name}" }
            @out.print "Select a section number and press enter (0 to exit):"
            input = @in.gets
            break if input == nil or input.chomp == "0" or input.chomp == ""
            section = @tutorial.sections[input.to_i - 1]
            run_section section
        end
        @out.puts "Bye!"
     end
end

if $0 == __FILE__
    ConsoleApp.new.run
end