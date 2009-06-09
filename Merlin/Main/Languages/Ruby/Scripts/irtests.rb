require 'optparse'
require 'singleton'

class IRTest
  include Singleton
  attr_accessor :options  
  def initialize
    @options = {}
    @results = ["Results:"]
    @root = ENV["MERLIN_ROOT"]
    mspec_base = "mspec ci -fd"
    ir = "#{@root}\\bin\\debug\\ir.exe"
    @suites = {
      :Smoke => "#{@root}\\Languages\\Ruby\\Tests\\Scripts\\irtest.bat",
      :Legacy => "#{@root}\\Languages\\Ruby\\Tests\\run.bat",
      :RubySpec_A => "#{mspec_base} :lang :cli :netinterop :cominterop :thread",
      :RubySpec_B => "#{mspec_base} :core1 :lib1",
      :RubySpec_C => "#{mspec_base} :core2 :lib2",
      :RubyGems => "#{ir} #{@root}\\Languages\\Ruby\\Scripts\\RubyGemsTests.rb",
      :Rake => "#{ir} #{@root}\\Languages\\Ruby\\Scripts\\RakeTests.rb",
    }
  end

  def self.method_missing(meth, *args, &blk)
    self.instance.send(meth, *args, &blk)
  end

  def run
    kill
    build_all
    test_all
    report
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
    msbuild "Ruby\\Ruby.sln"
    msbuild "IronPython\\IronPython.sln"

    if File.exists?(file = "#{@root}\\Scripts\\Python\\GenerateSystemCoreCsproj.py")
      cmd = "#{@root}\\Bin\\Debug\\ipy.exe #{file}"
      run_cmd cmd { @results << "Dev10 Build failed!!!" }
    end
  end

  def msbuild(project)
    cmd = "msbuild.exe /verbosity:minimal #{@root}\\Languages\\#{project} /p:Configuration=\"Debug\""
    run_cmd(cmd) { exit 1 }
  end

  def test_all
    @suites.each_key do |key|
      test(key)
    end
  end

  def test(suite)
    
    title = suite.to_s.gsub("_", " ") << " Tests"
    test = @suites[suite]
    cmd = nil
    if options[:parallel]
      cmd = "start \"#{title}\" #{test}"
    else
      puts title
      cmd = test
    end
    run_cmd(cmd) { @results << "#{title} failed!!!"}
  end

  def run_cmd(cmd, &blk)
    blk.call unless system cmd
  end
  
  def report
    if @results.size == 1
      puts "Success!!"
      exit 0
    else
      puts @results.join("\n")
      exit 1
    end
  end
end

if $0 == __FILE__
  OptionParser.new do |opts|
    opts.banner = "Usage: irtests.rb [options]"

    opts.separator ""

    opts.on("-p", "--[no-]parallel", "Run in parallel") do |p|
      IRTest.options[:parallel] = p
    end

    opts.on("-n", "--nocompile", "Don't compile before running") do |n|
      IRTest.options[:nocompile] = n
    end
    
    opts.on_tail("-h", "--help", "Show this message") do |n|
      puts opts
      exit
    end
  end.parse!

  IRTest.run
end
