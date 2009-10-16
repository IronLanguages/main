require 'optparse'
require 'singleton'

class IRTest
  attr_accessor :options  
  
  def initialize(options = {})
    @options = options
    
    @config = (options[:clr4] ? "V4 " : "") + (options[:release] ? "Release" : "Debug")
    @sl_config = "Silverlight " + (options[:release] ? "Release" : "Debug")
    
    @results = ["irtests FAILURES:"]
    @root = ENV["MERLIN_ROOT"] 
    @bin = "#{@root}\\bin\\#{@config}"
    ENV["ROWAN_BIN"] = @bin
       
    mspec_base = "#{@root}\\..\\External.LCA_RESTRICTED\\Languages\\IronRuby\\mspec\\mspec\\bin\\mspec.bat ci -fd"
    ir = "\"#{@bin}\\ir.exe\" -v"
    if options[:parallel]
      ir = "cmd /K #{ir}"
    end
    @suites = {
      :Smoke => "#{@root}\\Languages\\Ruby\\Tests\\Scripts\\irtest.bat",
      :Legacy => "#{@root}\\Languages\\Ruby\\Tests\\run.bat",
      :RubySpec_A => "#{mspec_base} :lang :cli :netinterop :cominterop :thread, :netcli",
      :RubySpec_B => "#{mspec_base} :core1 :lib1",
      :RubySpec_C => "#{mspec_base} :core2 :lib2",
      :RubyGems => "#{ir} #{@root}\\Languages\\Ruby\\Tests\\Scripts\\RubyGemsTests.rb",
      :Rake => "#{ir} #{@root}\\Languages\\Ruby\\Tests\\Scripts\\RakeTests.rb",
      :Yaml => "#{ir} #{@root}\\..\\External.LCA_RESTRICTED\\Languages\\IronRuby\\yaml\\YamlTest\\yaml_test_suite.rb",
      :Tutorial => "#{ir} -I#{@root}\\Languages\\Ruby\\Samples\\Tutorial #{@root}\\Languages\\Ruby\\Samples\\Tutorial\\test\\test_console.rb",
      :ActionPack => "#{ir} #{@root}\\Languages\\Ruby\\Tests\\Scripts\\ActionPackTests.rb",
      :ActiveSupport => "#{ir} #{@root}\\Languages\\Ruby\\Tests\\Scripts\\ActiveSupportTests.rb"
    }
    @start = Time.now
  end

  def run
    time("Starting")
    kill
    prereqs
    time("Compiling")
    build_all
    time("Running tests")
    test_all
    report
  end
  
  def time(str, start_time = 0)
    if start_time.kind_of? Time
      diff_secs = (Time.now - start_time).to_int
      mins = diff_secs / 60
      secs = diff_secs % 60
      puts "#{str} #{mins}:#{secs} minutes"
    else
      puts "#{str} #{Time.now}"
    end
  end

  def git?
    git = File.exists? @root + "\\..\\..\\.git" # exists only for github.com
    tfs = File.exists? @root + "\\Support\\Rowan.sln" # exists only in TFS
    abort("Could not determine if this is a GIT repo or not") if git == tfs
    git
  end
  
  def prereqs
    if git?
      autocrlf = `git.cmd config core.autocrlf`
      message = %{
        Please do 'git config core.autocrlf true'        
        Everyone should have autocrlf=true (the default value) so that the GIT blobs always use \\n
        as newline, while developers can edit the source files on platforms where newline is either
        \\n or \\r\\n. See http://www.kernel.org/pub/software/scm/git/docs/git-config.html for details
        
      }.gsub(/  +/, "")
      abort(message) if autocrlf.chomp != "true"
    end
  end 

  def kill
    %w{ir.exe ipy.exe}.each do |app|
      3.times do
        system "taskkill /f /im #{app} > nul: 2>&1"
      end
    end
  end

  def build_all
    if options[:nocompile]
      puts "Skipping compile step..."
      return
    end
    
    sln = @options[:clr4] ? "4.sln" : ".sln"
    
    msbuild "Languages\\Ruby\\Ruby" + sln
    msbuild "Languages\\IronPython\\IronPython" + sln

    if File.exists?(file = "#{@root}\\Scripts\\Python\\GenerateSystemCoreCsproj.py")
      cmd = "#{@root}\\Bin\\#{@config}\\ipy.exe #{file}"
      run_cmd(cmd) { @results << "Dev10 Build failed!!!" }
    end
    
    build_sl unless @options[:clr4]
  end
  
  def build_sl
    options = ""
    if git?
      program_files = ENV['ProgramFiles(x86)'] ? ENV['ProgramFiles(x86)'] : ENV['ProgramFiles']
      # Patches change the version number
      sl_path_candidates = ["3.0.40624.0", "3.0.40723.0"].map {|ver| "#{program_files}\\Microsoft Silverlight\\#{ver}" }
      sl_path = sl_path_candidates.select {|p| File.exist? p }.first
      if sl_path
        options = "/p:SilverlightPath=\"#{sl_path}\""
      else
        puts "Skipping Silverlight build since a Silverlight installation was not found at #{sl_path}..."
        return
      end
    end
        
    msbuild "Hosts\\Silverlight\\Silverlight.sln", @sl_config, options
  end

  def msbuild(project, build_config = @config, options = "")
    cmd = "msbuild.exe /verbosity:minimal #{@root}\\#{project} /p:Configuration=\"#{build_config}\" #{options}"
    run_cmd(cmd) { exit 1 }
  end
   
  def test_all
    @suites.each_key do |key|
      test(key)
    end
  end

  def test(suite)
    @report = true
    title = suite.to_s.gsub("_", " ") << " Tests"
    test = @suites[suite]
    cmd = nil
    if options[:parallel]
      cmd = "start /BELOWNORMAL \"#{title}\" #{test}"
    else
      puts title
      cmd = test
    end
    time(title)
    run_cmd(cmd) { @results << "#{title} failed!!!"}
  end

  def run_cmd(cmd, &blk)
    puts
    puts cmd
    puts
    blk.call unless system cmd
  end
  
  def exit_report
    at_exit { report if @report}
  end
  
  def report
    puts "=" * 70
    exit_code = if @results.size == 1
      puts "IRTESTS PASSED!!"
      0
    else
      puts @results.join("\n")
      1
    end

    puts    
    time("Finished")
    time("Total Elapsed time: ", @start)
    exit exit_code
  end
end


if $0 == __FILE__
  iroptions = {}

  OptionParser.new do |opts|
    opts.banner = "Usage: irtests.rb [options]"

    opts.separator ""

    opts.on("-p", "--[no-]parallel", "Run in parallel") do |p|
      iroptions[:parallel] = p
    end

    opts.on("-n", "--nocompile", "Don't compile before running") do |n|
      iroptions[:nocompile] = n
    end
    
    opts.on("-4", "--clr4", "Use CLR4 configuration") do |n|
      iroptions[:clr4] = n
    end
    
    opts.on("-r", "--release", "Use Release configurations") do |n|
      iroptions[:release] = n
    end
    
    opts.on_tail("-h", "--help", "Show this message") do |n|
      puts opts
      exit
    end
  end.parse!

  IRTest.new(iroptions).run
end
