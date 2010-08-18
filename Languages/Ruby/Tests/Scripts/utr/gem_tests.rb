class UnitTestSetup
  Version = '1.3.7'
  
  def initialize
    @name = "Gems"
    super
  end
  
  def require_files
    #HACK: this is loading up our defaults file which causes tests to fail.
    #the global is to stop that loading
    $utr_runner=true
    require 'rubygems'
  end

  def gather_files
    test_dir = File.expand_path("External.LCA_RESTRICTED/Languages/IronRuby/tests/RubyGems-#{Version}", ENV["DLR_ROOT"])
    $LOAD_PATH << test_dir

    # Note that the copy of minitest\unit.rb has a workaround at line 15 for http://redmine.ruby-lang.org/issues/show/1266
    ENV["GEM_PATH"] = File.expand_path("External.LCA_RESTRICTED/Languages/Ruby/ruby19/lib/ruby/gems/1.9.1", ENV["DLR_ROOT"])

    @all_test_files = Dir.glob("#{test_dir}/test_*.rb")
  end

  def sanity
    # Do some sanity checks
    sanity_size(50)
    abort("Did not find some expected files...") unless @all_test_files.select { |f| f =~ /test_gem_config/ }.size > 0
    sanity_version(Version, Gem::RubyGemsVersion)
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
    #dlr\External.LCA_RESTRICTED\Languages\IronRuby\RubyGems-1_3_1-test\gemutilities.rb has a workaround
    #for http://rubyforge.org/tracker/?func=detail&group_id=126&aid=24169&atid=575. However, the following
    #test fails inspite of the workaround. So we check if %TMP% is something like
    #C:\DOCUME~1\JANEDO~1\LOCALS~1\Temp
    if ENV['TMP'].include?('~')
      disable TestGemDependencyInstaller, :test_find_gems_with_sources_local
    end
  end
  
  def disable_mri_only_failures
    disable_by_name %w{
      test_self_prefix(TestGem)
      test_self_user_home_user_drive_and_path(TestGem)
      test_self_default_exec_format_jruby(TestGem)
      test_self_default_sources(TestGem)
      test_self_default_exec_format_18(TestGem)
      test_self_set_paths(TestGem)
      test_self_dir(TestGem)
      test_execute(TestGemCommandsEnvironmentCommand)
      test_execute_local_missing(TestGemCommandsInstallCommand)
      test_execute_nonexistent(TestGemCommandsInstallCommand)
      test_execute_all(TestGemCommandsQueryCommand)
      test_execute_legacy(TestGemCommandsQueryCommand)
      test_execute_local_notty(TestGemCommandsQueryCommand)
      test_execute(TestGemCommandsQueryCommand)
      test_execute_notty(TestGemCommandsQueryCommand)
      test_execute_local_details(TestGemCommandsQueryCommand)
      test_execute_details(TestGemCommandsQueryCommand)
      test_handle_options(TestGemCommandsServerCommand)
      test_execute_not_installed(TestGemCommandsUninstallCommand)
      test_execute_gem_path_missing(TestGemCommandsUnpackCommand)
      test_execute(TestGemCommandsUpdateCommand)
      test_execute_dependencies(TestGemCommandsUpdateCommand)
      test_execute_named(TestGemCommandsUpdateCommand)
      test_initialize_empty(TestGemDependency)
      test_install_env_shebang(TestGemDependencyInstaller)
      test_self_build_has_makefile(TestGemExtConfigureBuilder)
      test_self_build_fail(TestGemExtConfigureBuilder)
      test_self_build(TestGemExtConfigureBuilder)
      test_class_build(TestGemExtExtConfBuilder)
      test_class_build_extconf_fail(TestGemExtExtConfBuilder)
      test_class_make(TestGemExtExtConfBuilder)
      test_generate_index_ui(TestGemIndexer)
      test_shebang_env_shebang(TestGemInstaller)
      test_expand_and_validate_gem_dir(TestGemInstaller)
      test_app_script_text(TestGemInstaller)
      test_install_wrong_ruby_version(TestGemInstaller)
      test_install_wrong_rubygems_version(TestGemInstaller)
      test_fetch_size_bad_uri(TestGemRemoteFetcher)
      test_normalization(TestGemRequirement)
      test_parse_illformed(TestGemRequirement)
      test_parse(TestGemRequirement)
      test_cache_file(TestGemSourceInfoCache)
      test_latest_cache_file(TestGemSourceInfoCache)
      test_validate_email(TestGemSpecification)
      test_validate_summary(TestGemSpecification)
      test_validate_has_rdoc(TestGemSpecification)
      test_to_ruby(TestGemSpecification)
      test_validate_rubyforge_project(TestGemSpecification)
      test_validate_platform_legacy(TestGemSpecification)
      test_to_ruby_fancy(TestGemSpecification)
      test_validate_autorequire(TestGemSpecification)
      test_validate_executables(TestGemSpecification)
      test_validate(TestGemSpecification)
      test_validate_homepage(TestGemSpecification)
      test_initialize(TestGemSpecification)
      test_self_attribute_names(TestGemSpecification)
      test_validate_authors(TestGemSpecification)
      test_path_ok_eh_legacy(TestGemUninstaller)
      test_path_ok_eh(TestGemUninstaller)
      test_ok(TestGemVersion)
      test_normalize(TestGemVersion)
      test_bad(TestGemVersion)
      test_gem_conflicting(TestKernel)
    }
  end
end
