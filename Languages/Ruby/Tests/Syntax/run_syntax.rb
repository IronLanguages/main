require "../common"

if TestPath.get_environment_variable("DLR_ROOT").nil?
    print "Skipping syntax test for now ... \n"
    exit(0)
end 

class IntializationError
end 

class StateMachine
    def initialize
        @handlers = []
        @startState = nil
        @endStates = []
    end 
    
    def add_state(handler, end_state=false)
        @handlers << handler
        @endStates << handler if end_state 
    end 
    
    def set_start(handler)
        @startState = handler
    end 
    
    def run(target, line=nil)
        raise IntializationError if @startState.nil?
        raise IntializationError if @endStates.size == 0
        
        handler = target.method(@startState)
        while true
            (newState, line) = handler.call(line)
            if @endStates.include?(newState)
                target.method(newState).call(line)
                break
            elsif not @handlers.include?(newState)
                raise "Invalid target"
            else
                handler = target.method(newState)
            end
        end 
    end
end 

class LineParser
    def initialize(file_name)
        @stateMachine = StateMachine.new()
        @file_name = file_name
        @fp = File.open(file_name) # not close it explicitly yet
    end 

    def safe_readline
        begin
            line = @fp.readline()
        rescue EOFError
            return false
        else 
            return line
        end 
    end 

    def eof(line)
        # nothing 
    end 

    def process
        prepare
        @stateMachine.run(self, "no effect")
    end 
end 

class SimpleParser < LineParser
    attr_accessor :code_cb, :comment_cb
    REGEX_COMMENT = /^\#/
    
    def initialize(file_name)
        super
    end 

    def prepare
        @stateMachine.add_state(:eof, true)
        @stateMachine.add_state(:code)
        @stateMachine.add_state(:comment)
        @stateMachine.add_state(:first_state)
        @stateMachine.set_start(:first_state)
    end 
    
    def first_state(line)
        line = safe_readline
        return [:eof, line] unless line
        if line =~ REGEX_COMMENT
            return [:comment, line]
        else
            return [:code, line]
        end
    end 

    def code(line)
        while true
            @code_cb.call(line) if @code_cb
            line = safe_readline
            return [:eof, line] unless line
            return [:comment, line] if line =~ REGEX_COMMENT
        end 
    end

    def comment(line)
        while true
            @comment_cb.call(line) if @comment_cb
            line = safe_readline
            return [:eof, line] unless line
            return [:code, line] if line !~ REGEX_COMMENT
        end
    end 
end 

def test_SimpleParser
    p = SimpleParser.new(ARGV[0])
    p.code_cb    = proc { |line| printf "CODE    |%s", line }
    p.comment_cb = proc { |line| printf "COMMENT |%s", line }
    p.process
end 

class CodeSplitter < LineParser
    attr_accessor :patched_code_handler, :unpatched_code_handler
    
    def initialize(file_name)
        @count = 0
        @code_snippet = []
        super(file_name)
    end 
    
    def prepare
        @stateMachine.add_state(:eof, true)
        @stateMachine.add_state(:code)
        @stateMachine.add_state(:patched)
        @stateMachine.add_state(:unpatched)
        @stateMachine.add_state(:first_state)
        @stateMachine.set_start(:first_state)
    end 
    
    def first_state(line)
        line = safe_readline
        return [:eof, line] unless line
        if line =~ /^\# Scenario/
            return [:patched, line]
        elsif line =~ /^\#(\+|-)/
            return [:unpatched, line]
        else
            return [:code, line]
        end
    end 

    def code(line)
        while true
            @code_snippet << line
            line = safe_readline
            return [:eof, line] unless line
            return [:patched, line] if line =~ /^\# Scenario/
            return [:unpatched, line] if line =~ /^\#(\+|-)/
        end 
    end 
    
    def patched(line)
        _code(line, true)
    end 
    
    def unpatched(line)
        _code(line, false)
    end 
    
    def _code(line, patched = true)
        while true
            @code_snippet << line
            line = safe_readline
            
            if line and line =~ /^\#/
                next
            else 
                if patched
                    patched_code_handler.call(@code_snippet) if patched_code_handler
                else 
                    unpatched_code_handler.call(@code_snippet) if unpatched_code_handler
                end
                @code_snippet = []
                return [:eof, line] unless line
                return [:code, line]
            end 
        end 
    end 
end 

class SnippetShow 
    def run(test)
        csp = CodeSplitter.new(test)
        csp.patched_code_handler = proc { |snippet| puts "+" * 50; snippet.each { |l| printf("P |%s", l) } }
        csp.unpatched_code_handler = proc { |snippet| puts "-" * 50; snippet.each { |l| printf("U |%s", l) } }
        csp.process
    end
end 

def test_SnippetShow
    rp = SnippetShow.new()
    rp.run(ARGV[0])
end 

class ResultPatcher
    attr_reader :total, :failure
    def initialize(name, command)
        @name = name
        @command = command
        @total = @failure = 0
    end 
    
    def get_output(snippet)
        temp_file = "temp.rb"
        File.open(temp_file, "w") do |f| 
            snippet.each { |l| f << l }
        end 

        x = IO.popen("#@command #{temp_file} 2>&1", 'r+') do |io|
            io.read
        end 
        x.chomp
    end
    
    def verify_patched_code(snippet)
        output = get_output(snippet)

        # parse
        h = { }
        snippet.each do |l|
            if l =~ /# (.+): (.+)/
                h[$1.downcase] = $2
            end 
        end 

        if h.has_key?(@name.downcase) == false
            append(snippet, output)
            return 
        end 

        expected = h[@name.downcase] 
        
        rewrite = false
        if $?.exitstatus == 0 	# success
            if expected != "pass"
                rewrite = true
            end 
        else # fail
            if output.include?(expected) == false	# but can not find the expected string
                rewrite = true
            end 
        end
        
        if rewrite
            rewrite(snippet, output)
        else 
            repeat(snippet)
        end 
    end 
    
    def repeat(snippet)
        snippet.each { |line| @new_fp << line }
    end 
    
    def append(snippet, output)
        snippet.each { |line| @new_fp << line }
        @new_fp << "\# #{@name}: #{capture_result(output)}\n"
    end 
    
    def rewrite(snippet, output)
        snippet.each do |line|
            if line =~ /^# Default/
                @new_fp << "\# #{@name}: #{capture_result(output)}\n"
            else
                @new_fp << line
            end 
        end 
    end 

    def verify_unpatched_code(snippet)
        output = get_output(snippet)
        
        comment = "unknown"
        snippet.each do |l|
            if l =~ /#\+(.+)/
                expected = true
                comment = $1
                break
            elsif l =~ /#-(.+)/
                expected = false
                comment = $1
                break
            end 
        end 
        
        snippet.each do |l|
            if l !~ /#(\+|-)/
                @new_fp << l
            else
                @new_fp << "\# Scenario: #{comment.strip}\n"
                @new_fp << "\# #{@name}: "
                @new_fp << ($?.exitstatus == 0 ? "pass" : capture_result(output))
                @new_fp << "\n"
            end 
        end 
    end 
    
    ERROR_MESSAGES = [ 
        'syntax error', 
        'ArgumentError', 
        'NameError', 
        'TypeError', 
        'unterminated',
        'class/module name must be CONSTANT',
        'duplicate argument name',
        'duplicate optional argument name',
        'duplicate block argument name',
        'odd number list for Hash',
        ]
        
    def capture_result(s, verbatim = false)
        if verbatim
            s.gsub("\n", "\\n")
        else
            last = s.index('\n')
            if last
                first_line = s[0...last]
                
            else 
                first_line = s
            end 
            x = ERROR_MESSAGES.find { |m| first_line.include?(m) }
            if x 
                x
            else
                s.gsub("\n", "\\n")
            end
        end 
    end 

    def run(test)
        csp = CodeSplitter.new(test)
        @new_fp = File.open(test + "2", "w")
        csp.patched_code_handler = proc { |snippet| verify_patched_code(snippet) }
        csp.unpatched_code_handler = proc { |snippet| verify_unpatched_code(snippet) }
        csp.process
    end 
end 

class TestRunner 
    attr_reader :total, :failure
    def initialize(name, command)
        @name = name
        @command = command
        @total = @failure = 0
    end 

    def get_directory_hint(file_name)
        pos = 0
        while pos = file_name.index(/_|\./, pos+1)  do 
            dir_name = file_name[0...pos].gsub(/_/, "\\")
            yield(dir_name)
        end 
        dir_name
    end 
    
    def get_generated_file_name(dir_name)
        @count += 1
        dir_name + "\\" + "%02d" % @count + ".g.rb"
    end 

    def get_output(snippet)
        @generated = get_generated_file_name(@dir_name)
        
        File.open(@generated, "w") do |f| 
            snippet.each { |l| f << l }
        end 

        @total += 1
        x = IO.popen("#@command #{@generated} 2>&1", 'r+') do |io|
            io.read
        end 
        x.chomp
    end
    
    def test_patched_code(snippet)
        # parse
        h = { }
        snippet.each do |l|
            if l =~ /# (.+): (.+)/
                h[$1.downcase] = $2
            end 
        end 
        
        if h.has_key?(@name.downcase)
            if h[@name.downcase].include? "merlin_bug"
                printf "S"
                return 
            end
        end 

        output = get_output(snippet)
        
        if not h.has_key?(@name.downcase) 
            if h.has_key?("default")
                expected = h["default"]
            else
                raise "Can not find the expected behavior: %s\n" % @generated
            end 
        else
            expected = h[@name.downcase]
        end 
        if $?.exitstatus == 0   # success
            if expected != "pass"
                @failure += 1
                printf "expected pass, but not. %s\n", @generated
            else
                printf "+"
            end 
        else # fail
            if output.include?(expected) == false     # but can not find the expected string
                @failure += 1
                printf "expected fail, but not. %s\n", @generated
            else
                printf "-"
            end 
            end
    end 
    
    def test_unpatched_code(snippet)
        raise "update tests"
    end 
    
    def run(test)
        @rb_file = test
        @count = 0
        @dir_name = get_directory_hint(@rb_file) do |d| 
            Dir.mkdir(d) unless File.exist? d
        end
        
        csp = CodeSplitter.new(test)
        csp.patched_code_handler = proc { |snippet| test_patched_code(snippet) }
        csp.unpatched_code_handler = proc { |snippet| test_unpatched_code(snippet) }
        csp.process
    end 
end 

# ruby.exe driver.rb -patch -driver:Default [file]
# ruby.exe driver.rb -patch -driver:ParseOnly [file]
# ruby.exe driver.rb -test -driver:Default [file]
# ruby.exe driver.rb -test -driver:ParseOnly [file]

processor = TestRunner
driver_name = "default"
test_files = []
not_run_list = ["run_syntax.rb", "parseonly.py"]
driver_list = { 
    "default"       => TestPath::CRUBY_EXE, 
}

ARGV.each do |arg|
    if arg =~ /-patch/i
        processor = ResultPatcher
    elsif arg =~ /-test/i
        processor = TestRunner
    elsif arg =~ /-driver:(.+)/i
        driver_name = $1
    elsif arg =~ /-snap/
    else
        test_files << arg
    end
end 

if test_files.empty?
    Dir.glob("*.rb") do |f|
        test_files << f unless not_run_list.include? f
    end 
end 

printf "Start @ %s\n\n", Time.now

if ARGV.include? "-snap"
    class SnippetGenerator
        def run(test)
            count = 0
            csp = CodeSplitter.new(test)
            
            csp.patched_code_handler = proc { |snippet| 
                file_name = File.dirname(File.expand_path(test)) + "/generated/" + File.basename(test, ".rb") + "_%02d" % count + ".g.rb"
                open(file_name, "w") do |f|
                    f << snippet
                    count += 1
                end 
            }
            csp.unpatched_code_handler = proc { |snippet| raise "unexpected" }
            csp.process           
        end
    end 

    gendir = File.join(TestPath::TEST_DIR, "syntax/generated")
    Dir.mkdir(gendir) unless File.exists? gendir
    Dir.glob(gendir + "/*.rb") { |f|  File.delete f } 
    
    cs = SnippetGenerator.new
    test_files.each do |f|
        puts f
        cs.run(f)
    end
    
    cmd = "\"#{TestPath::IPYTHON_EXE}\" parseonly.py \"#{TestPath::TEST_DIR}/syntax/generated\" > parsing.log"
    print "\n\nRunning #{cmd} ... \n"
    system(cmd)
    exit($?.exitstatus)
else 
    printf "Current mode   : %s\n", processor
    printf "Current driver : %s\n", driver_name

    rp = processor.new(driver_name, driver_list[driver_name.downcase])
    test_files.each do |f|
        printf "\n>>> testing %s\n", f
        rp.run(f)
    end 

    printf "\n\nFinish @ %s\n", Time.now
    printf "\nSummary [Total: %d, failure: %d]\n", rp.total, rp.failure
    exit(rp.failure)
end 
