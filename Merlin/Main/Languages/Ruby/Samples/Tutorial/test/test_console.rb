require "test/unit"
require "stringio"
require "console_tutorial"
require File.expand_path("../Tutorials/ironruby_tutorial", File.dirname(__FILE__))

class SectionTest < Test::Unit::TestCase
    def setup
        @in = StringIO.new
        @out = StringIO.new
        @app = ConsoleApp.new nil, @in, @out
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

module ChapterTest
    def setup
        @in = StringIO.new
        @out = StringIO.new
        @app = ConsoleApp.new nil, @in, @out
    end
    
    def teardown
        @app.run
        assert_match /Chapter completed successfully!/, @out.string
    end    
end

class IntroductionSectionTest < Test::Unit::TestCase
    include ChapterTest
    
    def test_simple_commands_chapter
        hints = IntroductionSection.new.simple_commands_chapter.tasks.collect { |task| task.hint }
        @in.string = (["1", "1"] + hints).join("\n")
    end
    
    def test_multi_line_chapter
        hints = IntroductionSection.new.multi_line_chapter.tasks.collect { |task| task.hint }
        @in.string = (["1", "2"] + hints).join("\n")
    end
end

class RubySectionTest < Test::Unit::TestCase
    include ChapterTest
    
    def test_string_chapter
        hints = RubySection.new.string_chapter.tasks.collect { |task| task.hint }
        @in.string = (["2", "1"] + hints).join("\n")
    end
    
    def test_array_chapter
        hints = RubySection.new.array_chapter.tasks.collect { |task| task.hint }
        @in.string = (["2", "2"] + hints).join("\n")
    end
end

if defined? RUBY_ENGINE and RUBY_ENGINE == "ironruby"
class ClrSectionTest < Test::Unit::TestCase
    include ChapterTest
    
    def test_mscorlib_chapter
        hints = ClrSection.new.mscorlib_chapter.tasks.collect { |task| task.hint }
        @in.string = (["3", "1"] + hints).join("\n")
    end
end
end