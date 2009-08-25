
$stdlib = {}
ObjectSpace.each_object(Module) do |m|
  $stdlib[m.name] = true if m.respond_to? :name
end

require 'zentest_mapping'

$:.unshift( *$I.split(/:/) ) if defined? $I and String === $I
$r = false unless defined? $r # reverse mapping for testclass names

if $r then
  # all this is needed because rails is retarded
  $-w = false
  $: << 'test'
  $: << 'lib'
  require 'config/environment'
  f = './app/controllers/application.rb'
  require f if test ?f, f
end

$TESTING = true

class Module
  def zentest
    at_exit { ZenTest.autotest(self) }
  end
end

##
# ZenTest scans your target and unit-test code and writes your missing
# code based on simple naming rules, enabling XP at a much quicker
# pace. ZenTest only works with Ruby and Test::Unit.
#
# == RULES
#
# ZenTest uses the following rules to figure out what code should be
# generated:
#
# * Definition:
#   * CUT = Class Under Test
#   * TC = Test Class (for CUT)
# * TC's name is the same as CUT w/ "Test" prepended at every scope level.
#   * Example: TestA::TestB vs A::B.
# * CUT method names are used in CT, with "test_" prependend and optional "_ext" extensions for differentiating test case edge boundaries.
#   * Example:
#     * A::B#blah
#     * TestA::TestB#test_blah_normal
#     * TestA::TestB#test_blah_missing_file
# * All naming conventions are bidirectional with the exception of test extensions.
#
# See ZenTestMapping for documentation on method naming.

class ZenTest

  VERSION = '4.1.4'

  include ZenTestMapping

  if $TESTING then
    attr_reader :missing_methods
    attr_accessor :test_klasses
    attr_accessor :klasses
    attr_accessor :inherited_methods
  else
    def missing_methods; raise "Something is wack"; end
  end

  def initialize
    @result = []
    @test_klasses = {}
    @klasses = {}
    @error_count = 0
    @inherited_methods = Hash.new { |h,k| h[k] = {} }
    # key = klassname, val = hash of methods => true
    @missing_methods = Hash.new { |h,k| h[k] = {} }
  end

  # load_file wraps require, skipping the loading of $0.
  def load_file(file)
    puts "# loading #{file} // #{$0}" if $DEBUG

    unless file == $0 then
      begin
        require file
      rescue LoadError => err
        puts "Could not load #{file}: #{err}"
      end
    else
      puts "# Skipping loading myself (#{file})" if $DEBUG
    end
  end

  # obtain the class klassname, either from Module or
  # using ObjectSpace to search for it.
  def get_class(klassname)
    begin
      klass = Module.const_get(klassname.intern)
      puts "# found class #{klass.name}" if $DEBUG
    rescue NameError
      ObjectSpace.each_object(Class) do |cls|
        if cls.name =~ /(^|::)#{klassname}$/ then
          klass = cls
          klassname = cls.name
          break
        end
      end
      puts "# searched and found #{klass.name}" if klass and $DEBUG
    end

    if klass.nil? and not $TESTING then
      puts "Could not figure out how to get #{klassname}..."
      puts "Report to support-zentest@zenspider.com w/ relevant source"
    end

    return klass
  end

  # Get the public instance, class and singleton methods for
  # class klass. If full is true, include the methods from
  # Kernel and other modules that get included.  The methods
  # suite, new, pretty_print, pretty_print_cycle will not
  # be included in the resuting array.
  def get_methods_for(klass, full=false)
    klass = self.get_class(klass) if klass.kind_of? String

    # WTF? public_instance_methods: default vs true vs false = 3 answers
    # to_s on all results if ruby >= 1.9
    public_methods = klass.public_instance_methods(false)
    public_methods -= Kernel.methods unless full
    public_methods.map! { |m| m.to_s }
    public_methods -= %w(pretty_print pretty_print_cycle)

    klass_methods = klass.singleton_methods(full)
    klass_methods -= Class.public_methods(true)
    klass_methods = klass_methods.map { |m| "self.#{m}" }
    klass_methods  -= %w(self.suite new)

    result = {}
    (public_methods + klass_methods).each do |meth|
      puts "# found method #{meth}" if $DEBUG
      result[meth] = true
    end

    return result
  end

  # Return the methods for class klass, as a hash with the
  # method nemas as keys, and true as the value for all keys.
  # Unless full is true, leave out the methods for Object which
  # all classes get.
  def get_inherited_methods_for(klass, full)
    klass = self.get_class(klass) if klass.kind_of? String

    klassmethods = {}
    if (klass.class.method_defined?(:superclass)) then
      superklass = klass.superclass
      if superklass then
        the_methods = superklass.instance_methods(true)

        # generally we don't test Object's methods...
        unless full then
          the_methods -= Object.instance_methods(true)
          the_methods -= Kernel.methods # FIX (true) - check 1.6 vs 1.8
        end

        the_methods.each do |meth|
          klassmethods[meth.to_s] = true
        end
      end
    end
    return klassmethods
  end

  # Check the class klass is a testing class
  # (by inspecting its name).
  def is_test_class(klass)
    klass = klass.to_s
    klasspath = klass.split(/::/)
    a_bad_classpath = klasspath.find do |s| s !~ ($r ? /Test$/ : /^Test/) end
    return a_bad_classpath.nil?
  end

  # Generate the name of a testclass from non-test class
  # so that  Foo::Blah => TestFoo::TestBlah, etc. It the
  # name is already a test class, convert it the other way.
  def convert_class_name(name)
    name = name.to_s

    if self.is_test_class(name) then
      if $r then
        name = name.gsub(/Test($|::)/, '\1') # FooTest::BlahTest => Foo::Blah
      else
        name = name.gsub(/(^|::)Test/, '\1') # TestFoo::TestBlah => Foo::Blah
      end
    else
      if $r then
        name = name.gsub(/($|::)/, 'Test\1') # Foo::Blah => FooTest::BlahTest
      else
        name = name.gsub(/(^|::)/, '\1Test') # Foo::Blah => TestFoo::TestBlah
      end
    end

    return name
  end

  # Does all the work of finding a class by name,
  # obtaining its methods and those of its superclass.
  # The full parameter determines if all the methods
  # including those of Object and mixed in modules
  # are obtained (true if they are, false by default).
  def process_class(klassname, full=false)
    klass = self.get_class(klassname)
    raise "Couldn't get class for #{klassname}" if klass.nil?
    klassname = klass.name # refetch to get full name

    is_test_class = self.is_test_class(klassname)
    target = is_test_class ? @test_klasses : @klasses

    # record public instance methods JUST in this class
    target[klassname] = self.get_methods_for(klass, full)

    # record ALL instance methods including superclasses (minus Object)
    # Only minus Object if full is true.
    @inherited_methods[klassname] = self.get_inherited_methods_for(klass, full)
    return klassname
  end

  # Work through files, collecting class names, method names
  # and assertions. Detects ZenTest (SKIP|FULL) comments
  # in the bodies of classes.
  # For each class a count of methods and test methods is
  # kept, and the ratio noted.
  def scan_files(*files)
    assert_count = Hash.new(0)
    method_count = Hash.new(0)
    klassname = nil

    files.each do |path|
      is_loaded = false

      # if reading stdin, slurp the whole thing at once
      file = (path == "-" ? $stdin.read : File.new(path))

      file.each_line do |line|

        if klassname then
          case line
          when /^\s*def/ then
            method_count[klassname] += 1
          when /assert|flunk/ then
            assert_count[klassname] += 1
          end
        end

        if line =~ /^\s*(?:class|module)\s+([\w:]+)/ then
          klassname = $1

          if line =~ /\#\s*ZenTest SKIP/ then
            klassname = nil
            next
          end

          full = false
          if line =~ /\#\s*ZenTest FULL/ then
            full = true
          end

          unless is_loaded then
            unless path == "-" then
              self.load_file(path)
            else
              eval file, TOPLEVEL_BINDING
            end
            is_loaded = true
          end

          begin
            klassname = self.process_class(klassname, full)
          rescue
            puts "# Couldn't find class for name #{klassname}"
            next
          end

          # Special Case: ZenTest is already loaded since we are running it
          if klassname == "TestZenTest" then
            klassname = "ZenTest"
            self.process_class(klassname, false)
          end

        end # if /class/
      end # IO.foreach
    end # files

    result = []
    method_count.each_key do |classname|

      entry = {}

      next if is_test_class(classname)
      testclassname = convert_class_name(classname)
      a_count = assert_count[testclassname]
      m_count = method_count[classname]
      ratio = a_count.to_f / m_count.to_f * 100.0

      entry['n'] = classname
      entry['r'] = ratio
      entry['a'] = a_count
      entry['m'] = m_count

      result.push entry
    end

    sorted_results = result.sort { |a,b| b['r'] <=> a['r'] }

    @result.push sprintf("# %25s: %4s / %4s = %6s%%", "classname", "asrt", "meth", "ratio")
    sorted_results.each do |e|
      @result.push sprintf("# %25s: %4d / %4d = %6.2f%%", e['n'], e['a'], e['m'], e['r'])
    end
  end

  # Adds a missing method to the collected results.
  def add_missing_method(klassname, methodname)
    @result.push "# ERROR method #{klassname}\##{methodname} does not exist (1)" if $DEBUG and not $TESTING
    @error_count += 1
    @missing_methods[klassname][methodname] = true
  end

  # looks up the methods and the corresponding test methods
  # in the collection already built.  To reduce duplication
  # and hide implementation details.
  def methods_and_tests(klassname, testklassname)
    return @klasses[klassname], @test_klasses[testklassname]
  end

  # Checks, for the given class klassname, that each method
  # has a corrsponding test method. If it doesn't this is
  # added to the information for that class
  def analyze_impl(klassname)
    testklassname = self.convert_class_name(klassname)
    if @test_klasses[testklassname] then
      methods, testmethods = methods_and_tests(klassname,testklassname)

      # check that each method has a test method
      @klasses[klassname].each_key do | methodname |
        testmethodname = normal_to_test(methodname)
        unless testmethods[testmethodname] then
          begin
            unless testmethods.keys.find { |m| m =~ /#{testmethodname}(_\w+)+$/ } then
              self.add_missing_method(testklassname, testmethodname)
            end
          rescue RegexpError => e
            puts "# ERROR trying to use '#{testmethodname}' as a regex. Look at #{klassname}.#{methodname}"
          end
        end # testmethods[testmethodname]
      end # @klasses[klassname].each_key
    else # ! @test_klasses[testklassname]
      puts "# ERROR test class #{testklassname} does not exist" if $DEBUG
      @error_count += 1

      @klasses[klassname].keys.each do | methodname |
        self.add_missing_method(testklassname, normal_to_test(methodname))
      end
    end # @test_klasses[testklassname]
  end

  # For the given test class testklassname, ensure that all
  # the test methods have corresponding (normal) methods.
  # If not, add them to the information about that class.
  def analyze_test(testklassname)
    klassname = self.convert_class_name(testklassname)

    # CUT might be against a core class, if so, slurp it and analyze it
    if $stdlib[klassname] then
      self.process_class(klassname, true)
      self.analyze_impl(klassname)
    end

    if @klasses[klassname] then
      methods, testmethods = methods_and_tests(klassname,testklassname)

      # check that each test method has a method
      testmethods.each_key do | testmethodname |
        if testmethodname =~ /^test_(?!integration_)/ then

          # try the current name
          methodname = test_to_normal(testmethodname, klassname)
          orig_name = methodname.dup

          found = false
          until methodname == "" or methods[methodname] or @inherited_methods[klassname][methodname] do
              # try the name minus an option (ie mut_opt1 -> mut)
            if methodname.sub!(/_[^_]+$/, '') then
              if methods[methodname] or @inherited_methods[klassname][methodname] then
                found = true
              end
            else
              break # no more substitutions will take place
            end
          end # methodname == "" or ...

          unless found or methods[methodname] or methodname == "initialize" then
            self.add_missing_method(klassname, orig_name)
          end

        else # not a test_.* method
          unless testmethodname =~ /^util_/ then
            puts "# WARNING Skipping #{testklassname}\##{testmethodname}" if $DEBUG
          end
        end # testmethodname =~ ...
      end # testmethods.each_key
    else # ! @klasses[klassname]
      puts "# ERROR class #{klassname} does not exist" if $DEBUG
      @error_count += 1

      @test_klasses[testklassname].keys.each do |testmethodname|
        @missing_methods[klassname][test_to_normal(testmethodname)] = true
      end
    end # @klasses[klassname]
  end

  def test_to_normal(_name, klassname=nil)
    super do |name|
      if defined? @inherited_methods then
        known_methods = (@inherited_methods[klassname] || {}).keys.sort.reverse
        known_methods_re = known_methods.map {|s| Regexp.escape(s) }.join("|")

        name = name.sub(/^(#{known_methods_re})(_.*)?$/) { $1 } unless
          known_methods_re.empty?

        name
      end
    end
  end

  # create a given method at a given
  # indentation. Returns an array containing
  # the lines of the method.
  def create_method(indentunit, indent, name)
    meth = []
    meth.push indentunit*indent + "def #{name}"
    meth.last << "(*args)" unless name =~ /^test/
    indent += 1
    meth.push indentunit*indent + "raise NotImplementedError, 'Need to write #{name}'"
    indent -= 1
    meth.push indentunit*indent + "end"
    return meth
  end

  # Walk each known class and test that each method has
  # a test method
  # Then do it in the other direction...
  def analyze
    # walk each known class and test that each method has a test method
    @klasses.each_key do |klassname|
      self.analyze_impl(klassname)
    end

    # now do it in the other direction...
    @test_klasses.each_key do |testklassname|
      self.analyze_test(testklassname)
    end
  end

  # Using the results gathered during analysis
  # generate skeletal code with methods raising
  # NotImplementedError, so that they can be filled
  # in later, and so the tests will fail to start with.
  def generate_code
    @result.unshift "# Code Generated by ZenTest v. #{VERSION}"

    if $DEBUG then
      @result.push "# found classes: #{@klasses.keys.join(', ')}"
      @result.push "# found test classes: #{@test_klasses.keys.join(', ')}"
    end

    if @missing_methods.size > 0 then
      @result.push ""
      @result.push "require 'test/unit/testcase'"
      @result.push "require 'test/unit' if $0 == __FILE__"
      @result.push ""
    end

    indentunit = "  "

    @missing_methods.keys.sort.each do |fullklasspath|

      methods = @missing_methods[fullklasspath]
      cls_methods = methods.keys.grep(/^(self\.|test_class_)/)
      methods.delete_if {|k,v| cls_methods.include? k }

      next if methods.empty? and cls_methods.empty?

      indent = 0
      is_test_class = self.is_test_class(fullklasspath)
      klasspath = fullklasspath.split(/::/)
      klassname = klasspath.pop

      klasspath.each do | modulename |
        m = self.get_class(modulename)
        type = m.nil? ? "module" : m.class.name.downcase
        @result.push indentunit*indent + "#{type} #{modulename}"
        indent += 1
      end
      @result.push indentunit*indent + "class #{klassname}" + (is_test_class ? " < Test::Unit::TestCase" : '')
      indent += 1

      meths = []

      cls_methods.sort.each do |method|
        meth = create_method(indentunit, indent, method)
        meths.push meth.join("\n")
      end

      methods.keys.sort.each do |method|
        next if method =~ /pretty_print/
        meth = create_method(indentunit, indent, method)
        meths.push meth.join("\n")
      end

      @result.push meths.join("\n\n")

      indent -= 1
      @result.push indentunit*indent + "end"
      klasspath.each do | modulename |
        indent -= 1
        @result.push indentunit*indent + "end"
      end
      @result.push ''
    end

    @result.push "# Number of errors detected: #{@error_count}"
    @result.push ''
  end

  # presents results in a readable manner.
  def result
    return @result.join("\n")
  end

  # Runs ZenTest over all the supplied files so that
  # they are analysed and the missing methods have
  # skeleton code written.
  def self.fix(*files)
    zentest = ZenTest.new
    zentest.scan_files(*files)
    zentest.analyze
    zentest.generate_code
    return zentest.result
  end

  # Process all the supplied classes for methods etc,
  # and analyse the results. Generate the skeletal code
  # and eval it to put the methods into the runtime
  # environment.
  def self.autotest(*klasses)
    zentest = ZenTest.new
    klasses.each do |klass|
      zentest.process_class(klass)
    end

    zentest.analyze

    zentest.missing_methods.each do |klass,methods|
      methods.each do |method,x|
        warn "autotest generating #{klass}##{method}"
      end
    end

    zentest.generate_code
    code = zentest.result
    puts code if $DEBUG

    Object.class_eval code
  end
end
