class UnitTestSetup
  def initialize
    @name = "Gems"
    super
  end
  
  def require_files
    require 'rubygems'
  end

  def gather_files
    test_dir = File.expand_path("../External.LCA_RESTRICTED/Languages/IronRuby/tests/RubyGems-1_3_5-test", ENV["MERLIN_ROOT"])
    $LOAD_PATH << test_dir

    # Note that the copy of minitest\unit.rb has a workaround at line 15 for http://redmine.ruby-lang.org/issues/show/1266
    ENV["GEM_PATH"] = File.expand_path("../External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8", ENV["MERLIN_ROOT"])

    @all_test_files = Dir.glob("#{test_dir}/test_*.rb")
  end

  def sanity
    # Do some sanity checks
    sanity_size(50)
    abort("Did not find some expected files...") unless @all_test_files.select { |f| f =~ /test_gem_config/ }.size > 0
    sanity_version('1.3.5', Gem::RubyGemsVersion)
    warn("Some tests are expected to fail with 'ir.exe -D'. Do not use -D...") if $DEBUG
  end
  
  # test_gem_stream_ui.rb uses Timeout.timeout which can cause hangs as described in 
  # http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=4023
  # Increase the timeout to reduce the chance of this occuring when other processes are
  # also running (like with "irtests -p")
  require 'timeout'
  Timeout.instance_eval do
    alias :original_timeout :timeout
    def timeout(sec, klass=nil, &b) original_timeout(((sec == nil) ? sec : sec * 50), klass, &b) end
  end

  def disable_tests
    #Merlin\External.LCA_RESTRICTED\Languages\IronRuby\RubyGems-1_3_1-test\gemutilities.rb has a workaround
    #for http://rubyforge.org/tracker/?func=detail&group_id=126&aid=24169&atid=575. However, the following
    #test fails inspite of the workaround. So we check if %TMP% is something like
    #C:\DOCUME~1\JANEDO~1\LOCALS~1\Temp
    if ENV['TMP'].include?('~')
      disable TestGemDependencyInstaller, :test_find_gems_with_sources_local
    end
  end
end
