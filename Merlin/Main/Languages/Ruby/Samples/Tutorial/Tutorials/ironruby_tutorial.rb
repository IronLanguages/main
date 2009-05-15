require "tutorial"

class IntroductionSection < Section
    def initialize
        super "Introduction", "Basic IronRuby usage"
    end
    
    def simple_commands_chapter
        tasks = []
        
        tasks << Task.new(
            "String literals", 
            "Execute simple statements listed below. After each statement, IronRuby prints the result, if any, and awaits more input.",
            "2+2") { |bind, output, result| result == 4 }

        tasks << Task.new(
            "String literals", 
            "Now let's do some printing. This is done with the puts function.",
            "puts 'Hello world'") { |bind, output, result| output.chomp == 'Hello world' }

        tasks << Task.new(
            "String literals", 
            "Let's use a local variable.",
            "x = 1") { |bind, output, result| eval("x == 1", bind) }

        tasks << Task.new(
            "String literals", 
            "And then print the local variable.",
            "puts x") { |bind, output, result| output.chomp == "1" }

        Chapter.new "Simple commands", "This chapter gets you started with basic usage of the interactive console", tasks
    end
    
    def multi_line_chapter
        tasks = []
        
        tasks << Task.new(
            "Multi-line 'if' statement", 
            "Entering multiple lines in an interactive console is a bit tricky as it can be ambigous when you are done 
            entering a statement. When you press the @Enter@ key, you may either be expecting to execute the code you have
            typed already, or you may want to enter more code. Also, sometimes you might want to go back and edit a line
            above.
            
            The tutorial currently only handles single line input. Use @;@ to separate statements",
            ["if 2 < 3 then puts 'this'; puts 'that' end"]) { |bind, output, result| output =~ /this\nthat/ }

        Chapter.new "Simple multi-line commands", "This chapter explains how multi-line statements can be used in the tutorial", tasks
    end
end

class RubySection < Section
    def initialize
        super "Ruby", "Basic language features"
    end
    
    def string_chapter
        tasks = []
        
        tasks << Task.new(
            "String literals", 
            "Strings can be declared using either single or double quotes.",
            "'hello'") { |bind, output, result| result == 'hello' }

        tasks << Task.new(
            "String manipulation", 
            "The @index@ method allows you to search for a substring.",
            "'hello there'.index('there')") { |bind, output, result| result == 6 }
            
        Chapter.new "String", "This chapter shows common ways of working with strings", tasks
    end
    
    def array_chapter
        tasks = []
        
        tasks << Task.new(
            "Array literals", 
            "Arrays can be declared using square brackets.",
            "[1, 2, 3]") { |bind, output, result| result == [1, 2, 3] }

        tasks << Task.new(
            "Heterogenous arrays",
            "Arrays can contain data of any type.",
            "[1, nil, 'hello', []]") { |bind, output, result| result == [1, nil, 'hello', []] }
            
        Chapter.new "Array", "This chapter shows common ways of working with arrays", tasks
    end
end

class ClrSection < Section
    def initialize
        super "CLR", "CLR interop features"
    end
    
    def mscorlib_chapter
        tasks = []
        
        tasks << Task.new(
            "Basic CLR library use", 
            "The core CLR library mscorlib.dll is always automatically loaded in IronRuby. You can inspect the @System@ namespace in it.",
            "System") { |bind, output, result| result == System }
                        
        Chapter.new "mscorlib", "This chapter shows how to use the CLR namespaces and types", tasks
    end
end

class AdvancedClrSection < Section
    def initialize
        super "Advanced CLR", "Advanced CLR interop features"
    end	
end

class ComSection < Section
    def initialize
        super "COM", "COM interop features"
    end	
end

class EmbeddingSection < Section
    def initialize
        super "Embedding IronRuby", "Embedding IronRuby in a host app to make it scriptable"
    end	
end

class IronRubyTutorial < Tutorial
    def initialize
        sections = [
            IntroductionSection.new, 
            RubySection.new, 
            ClrSection.new, 
            AdvancedClrSection.new, 
            ComSection.new, 
            EmbeddingSection.new
            ]

        super "IronRuby tutorial", sections
    end
end

