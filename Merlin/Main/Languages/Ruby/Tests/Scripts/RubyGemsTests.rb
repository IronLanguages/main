require "rubygems"

test_dir = File.expand_path("../External.LCA_RESTRICTED/Languages/IronRuby/RubyGems-1_3_5-test", ENV["MERLIN_ROOT"])
$LOAD_PATH << test_dir

# Note that the copy of minitest\unit.rb has a workaround at line 15 for http://redmine.ruby-lang.org/issues/show/1266
ENV["GEM_PATH"] = File.expand_path("../External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8", ENV["MERLIN_ROOT"])

all_test_files = Dir.glob("#{test_dir}/test_*.rb")

# Do some sanity checks
abort("Did not find enough RubyGems tests files...") unless all_test_files.size > 50
abort("Did not find some expected files...") unless all_test_files.select { |f| f =~ /test_gem_config/ }.size > 0
abort("Loaded the wrong version #{Gem::RubyGemsVersion} of RubyGems instead of the expected 1.3.5 ...") unless Gem::RubyGemsVersion == '1.3.5'
warn("Some tests are expected to fail with 'ir.exe -D'. Do not use -D...") if $DEBUG

# Note that the tests are registered using Kernel#at_exit, and will run during shutdown
# The "require" statement just registers the tests for being run later...
all_test_files.each { |f| require f }


#Merlin\External.LCA_RESTRICTED\Languages\IronRuby\RubyGems-1_3_1-test\gemutilities.rb has a workaround
#for http://rubyforge.org/tracker/?func=detail&group_id=126&aid=24169&atid=575. However, the following
#test fails inspite of the workaround. So we check if %TMP% is something like
#C:\DOCUME~1\JANEDO~1\LOCALS~1\Temp
if ENV['TMP'].include?('~')
  class TestGemDependencyInstaller
    def test_find_gems_with_sources_local() end
  end
end
