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

require "test/unit"
require "stringio"
require "console_tutorial"
require "html_tutorial"

class ConsoleTutorialTest < Test::Unit::TestCase
    def setup
        @in = StringIO.new
        @out = StringIO.new
        @app = ConsoleTutorial.new nil, @in, @out
    end
    
    def test_early_out
        @in.string = ["0"].join("\n")
        @app.run
        assert_match /Bye!/, @out.string
    end

    def test_chose_section
        @in.string = ["1", "0", "0"].join("\n")
        @app.run
        assert_match /Bye!/, @out.string
    end
end

class TutorialTests < Test::Unit::TestCase
    def self.clean_name n
        n.gsub(/[^[:alnum:]]+/, "").gsub(/\s/, "_")
    end
    
    app = ConsoleTutorial.new
    app.tutorial.sections.each_index do |s|
        section = app.tutorial.sections[s]
        section.chapters.each_index do |c|
            chapter = section.chapters[c]
            test_name = "test_#{clean_name(section.name)}_#{clean_name(chapter.name)}"
            define_method test_name.to_sym do
                codes = chapter.tasks.collect { |task| task.code }
                @in.string = ([(s + 1).to_s, (c + 1).to_s] + codes).join("\n")
            end
        end
    end
    
    def setup
        @in = StringIO.new
        @out = StringIO.new
        @app = ConsoleTutorial.new nil, @in, @out
    end
    
    def teardown
        @app.run
        assert_match /Chapter completed successfully!/, @out.string
    end    
end

class HtmlGeneratorTests < Test::Unit::TestCase
  def test_sanity
    html_tutorial = HtmlTutorial.new
    html = html_tutorial.generate_html
    assert_match %r{<h2>Table of Contents</h2>}, html
  end
end