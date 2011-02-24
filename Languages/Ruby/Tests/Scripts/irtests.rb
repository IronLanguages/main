require 'optparse'
require 'singleton'

ClrProcess = System::Diagnostics::Process
ClrStartInfo = System::Diagnostics::ProcessStartInfo
ClrPriority = System::Diagnostics::ProcessPriorityClass
    
class IRTest
  attr_accessor :options  
  
  def initialize(options, tasks = [])
    if options[:wait_on_failure]
      at_exit do
        # IR bug:
        # unless $!.nil? || $!.class == SystemExit && $!.success?
        unless $exit_code == 0
          puts "Press any key to continue ..."
          STDIN.getc
        end
      end 
    end
    
    if ENV['DLR_VM'] and ENV['DLR_VM'].include?("mono.exe")
      options[:mono] = true
    elsif options[:mono]
      # assume mono.exe is on path
      ENV['DLR_VM'] ||= 'mono.exe'
    end
    
    @options = options
    if options[:clr2]
      if options[:release]
        @config = "v2Release"
        @sl_config = "Silverlight3Release"
      else
        @config = "v2Debug"
        @sl_config = "Silverlight3Debug"
      end
    else
      if options[:release]
        @config = "Release"
        @sl_config = "Silverlight4Release"
      else
        @config = "Debug"
        @sl_config = "Silverlight4Debug"
      end
    end
    
    @root = File.expand_path(ENV["DLR_ROOT"])
    
    explicit_config = !options[:clr2] or options[:release]
    requires_compile = !options[:nocompile]
    abort "Using DLR_BIN requires using --nocompile" if ENV["DLR_BIN"] and requires_compile and not explicit_config
    
    # Set DLR_BIN if the user asked for a specific configuration, or if it is not already set.
    # If it is already set, we will use it (unless the user overrides with an explict configuration request)
    # All the test command lines we execute will (or should) honor DLR_BIN
    if explicit_config or not ENV["DLR_BIN"]
      ENV["DLR_BIN"] = "#@root/bin/#@config"
    end
    
    ENV['HOME'] ||= ENV["USERPROFILE"]
    
    @ir = "#{vm_shim} #{dlr_path("bin/#@config/ir.exe")}" 
    
    if options[:selftest]
      @all_tasks = {
        :Failing          => ruby_runner(File.dirname(__FILE__) + "/irtests/failing_task.rb"),
        :Passing          => ruby_runner(File.dirname(__FILE__) + "/irtests/passing_task.rb"),
      }
      
      @parallel_tasks = [
        [:Passing, :Passing],
        [:Failing, :Passing],
      ]
    else
      @all_tasks = {
        :Smoke            => safe_ruby_runner(ruby_tests_path('Scripts/unit_tests.rb')),
        :RubySpec_A       => spec_runner(":lang :cli :netinterop :cominterop :thread :netcli"),
        :RubySpec_B       => spec_runner(":core1 :lib1"),
        :RubySpec_C       => spec_runner(":core2 :lib2"),
        :RubyGems         => utr_runner("gem"),
        :TZInfo           => utr_runner("tzinfo"),
        :Rake             => utr_runner("rake"),
        :Yaml             => ruby_runner(ruby_tests_path('Libraries/Yaml/yaml_test_suite.rb')),
        
        # TODO: fix these or merge them with mspec
        #:Legacy           => safe_ruby_runner(ruby_tests_path('Legacy/run.rb')),
        
        # TODO: fix these and get rid of .bat file
        #:Tutorial         => shell_runner("#{dlr_path('Languages/Ruby/Samples/Tutorial/tutorial.bat')} #{dlr_path('Languages/Ruby/Samples/Tutorial/test/test_console.rb')}"),
      }
      
      @all_tasks[:BuildSilverlight] = silverlight_build_runner unless options[:nocompile]
    
      if not options[:minimum]
        @all_tasks.merge!({
          :ActionMailer   => utr_runner("action_mailer"),
          #:ActionPack     => utr_runner("action_pack"),
          :ActiveSupport  => utr_runner("active_support"),
          :ActiveRecord   => utr_runner("active_record"),
          :ActiveResource => utr_runner("active_resource"),
        })
      end
    
      @parallel_tasks = [
         [:Smoke, :BuildSilverlight],
         [:RubySpec_A],
         [:RubySpec_B],
         [:RubySpec_C],
         [:TZInfo, :Yaml, :Tutorial, :RubyGems],
      ]
    
      if not options[:minimum]
         @parallel_tasks += [
           [:ActionMailer, :ActiveSupport, :ActionPack, :ActiveResource],
           [:ActiveRecord],
         ]
      end 
    end
    
    if options[:parallel]
      abort "Do not specify suite names in -p mode: #{tasks}" unless tasks.empty?
    elsif tasks.empty?
      # run all tasks
      @tasks = []
      @all_tasks.each { |k, v| @tasks << [k, v] }
    else
      # run selected tasks
      @tasks = []
      tasks.each do |task|
        runner = @all_tasks[task.to_sym]
        @tasks << [task.to_sym, runner] unless runner.nil?
      end
    end    
  end
  
  def dlr_path(path)
    q File.join(@root, path)
  end

  def ruby_tests_path(path)
    q File.join(@root, 'Languages/Ruby/Tests', path)
  end
  
  def q(str)
    str.include?(' ') ? '"' + str + '"' : str
  end
  
  def vm_shim
    ENV["DLR_VM"] ? q(ENV["DLR_VM"]) : nil
  end
  
  def shell_runner(cmd)
    # returns true on success:
    lambda do
      run_cmd cmd
    end
  end
  
  # Uses the latest shipped IronRuby.
  def safe_ruby_runner(cmd)
    shell_runner "#{vm_shim} #{dlr_path('Util/IronRuby/bin/ir.exe')} #{cmd}"
  end
  
  def ruby_runner(cmd)
    shell_runner "#@ir -v #{cmd}"
  end
  
  def utr_runner(suite, version = nil)
    shell_runner "#@ir #{version} #{ruby_tests_path('Scripts/utr.rb')} #{suite}"
  end
  
  def spec_runner(specs)
    safe_ruby_runner "#{ruby_tests_path('mspec/mspec/bin/mspec')} ci -fd #{specs}"
  end
  
  def build_cmd(solution, build_config = @config, options = "")
    build_engine = @options[:mono] ? "xbuild" : "msbuild"
    "#{build_engine} /verbosity:minimal #{dlr_path("Solutions/#{solution}.sln")} /p:Configuration=#{q build_config} #{options}"
  end
  
  def silverlight_build_runner
    lambda { run_cmd build_cmd("Ruby", @sl_config) }
  end
  
  def on_windows
	case System::Environment.OSVersion.Platform
	  when System::PlatformID.Win32S
	  when System::PlatformID.Win32Windows
	  when System::PlatformID.Win32NT
		true
	  else
		false
	end
  end
  
  def run
    prereqs
    
    # build prerequisites:
    unless options[:nocompile]
      colorize :cyan do
        puts "Building prerequisites ..."
      end
      run_cmd build_cmd("Ruby")
      puts
      run_cmd build_cmd("IronPython")
      puts
    end
    
    # run tests and report results:
    report run_tasks
  end
  
  # returns an array of failed tasks
  def run_tasks   
    if options[:parallel]
      run_parallel_tasks @parallel_tasks
    else
      run_serial_tasks @tasks  
    end
  end
  
  # Launches a process for each task group in tasks waits for these processes.
  # Returns an array of failed tasks.
  def run_parallel_tasks task_groups
    colorize :cyan do
      puts "Launching processes for task groups:"
    end
    
    common_exe = dlr_path('Util/IronRuby/bin/ir.exe')
    common_args = "#{q File.expand_path(__FILE__)} -w -n #{build_options.join}"
    if vm_shim
      common_args = "#{common_exe} #{common_args}"
      common_exe = vm_shim
    end
    puts "#{common_exe} #{common_args} <task list>"
    
    processes = task_groups.map do |serial_tasks| 
      task_str = serial_tasks.join(' ')
      
      process = ClrProcess.new
      process.start_info.file_name = common_exe
      process.start_info.arguments = "#{common_args} #{task_str}"
      
      print "  #{task_str}"
      process.start
      process.priority_class = ClrPriority.below_normal
      puts " => pid #{process.id}"
      
      [process, task_str]
    end
    
    puts
    colorize :cyan do
      puts "Waiting for tests to finish ..."
    end
    processes.each do |process, _| 
      process.wait_for_exit 
    end

    results = []
    processes.each do |process, task_str|
      results << task_str if process.exit_code != 0
    end    
    
    results
  end
    
  # Runs tasks in tasks one by one and collects failures.
  # Returns an array of failed tasks.
  def run_serial_tasks tasks
    results = []
    tasks.each do |name, runner|
      colorize :cyan do
        puts "Task #{name} ..."
      end
      
      start_time = Time.now
      success = runner.call
      results << name unless success
      duration = Time.now - start_time
      
      puts
      colorize(success ? :green : :red) do
        print success ? "PASSED" : "FAILED"
      end
      printf " in %d:%.1f.\n", duration / 60, duration % 60
      puts
    end
    results
  end
  
  def report failures
    puts "=" * 70
    
    if failures.empty?
      colorize :green do
        puts "PASSED"
      end
      $exit_code = 0
    else
      colorize :red do
        puts "FAILED:"
        failures.each { |failed_task| puts "  #{failed_task}" }
        $exit_code = 1
      end
    end

    exit $exit_code
  end
  
  def colorize color
    old_color = System::Console.foreground_color
    System::Console.foreground_color = System::ConsoleColor.method(color).call
    yield
  ensure
    System::Console.foreground_color = old_color
  end
  
  def run_cmd(cmd)
    puts "=" * 70
    puts cmd
    puts "=" * 70
    system cmd
  end
  
  def git?
    not File.exists? dlr_path("Internal/Dlr.sln")
  end
  
  def prereqs
    if git?
      autocrlf = `git config core.autocrlf`
      message = %{
        Please do 'git config core.autocrlf true'        
        Everyone should have autocrlf=true (the default value) so that the GIT blobs always use \\n
        as newline, while developers can edit the source files on platforms where newline is either
        \\n or \\r\\n. See http://www.kernel.org/pub/software/scm/git/docs/git-config.html for details
        
      }.gsub(/  +/, "")
      abort(message) if autocrlf.chomp != "true"
    end
  end 
  
  def build_options
    [
      @options[:mono] ? "-m" : nil,
      @options[:clr2] ? "-2" : nil,
      @options[:release] ? "-r" : nil,
      @options[:selftest] ? "-s" : nil,
    ]
  end
end


if $0 == __FILE__
  iroptions = {}

  ARGV.options do |opts|
    opts.program_name = "irtests.rb"

    opts.separator ""

    opts.on("-m", "--minimum", "Run the minimum set of tests required for a checkin") do |m|
      iroptions[:minimum] = m
    end
    opts.on("-p", "--[no-]parallel", "Run in parallel") do |p|
      iroptions[:parallel] = p
    end

    opts.on("-n", "--nocompile", "Don't compile before running") do |n|
      iroptions[:nocompile] = n
    end
    
    opts.on("-2", "--clr2", "Use CLR2 configuration") do |n|
      iroptions[:clr2] = n
    end
    
    opts.on("-m", "--mono", "Run tests on Mono") do |n|
      iroptions[:mono] = n
    end
    
    opts.on("-r", "--release", "Use Release configurations") do |n|
      iroptions[:release] = n
    end
    
    opts.on("-w", "--waitfail", "Wait on failure") do |n|
      iroptions[:wait_on_failure] = n
    end
    
    opts.on("-s", "--selftest", "Test the test runner") do |n|
      iroptions[:selftest] = n
    end
    
    opts.on_tail("-h", "--help", "Show this message") do |n|
      puts opts
      exit
    end
  end
  
  ARGV.order!

  IRTest.new(iroptions, ARGV).run
end
