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
    test_dir = File.expand_path("Languages/Ruby/Tests/Libraries/RubyGems-#{Version}", ENV["DLR_ROOT"])
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
#  require 'timeout'
#  Timeout.instance_eval do
#    alias :original_timeout :timeout
#    def timeout(sec, klass=nil, &b) original_timeout(((sec == nil) ? sec : sec * 50), klass, &b) end
#  end

  def disable_tests
    #
    # TODO: is this still applicable?
    # 
    #RubyGems-1_3_1-test\gemutilities.rb has a workaround
    #for http://rubyforge.org/tracker/?func=detail&group_id=126&aid=24169&atid=575. However, the following
    #test fails inspite of the workaround. So we check if %TMP% is something like
    #C:\DOCUME~1\JANEDO~1\LOCALS~1\Temp
    #if ENV['TMP'].include?('~')
    #  disable TestGemDependencyInstaller, :test_find_gems_with_sources_local
    #end

    disable_by_name %w{
      test_self_prefix(TestGem)
      test_invoke_with_help(TestGemCommand):
      test_defaults(TestGemCommand):
      test_invoke_with_options(TestGemCommand):
      test_process_args_install(TestGemCommandManager)
      test_process_args_update(TestGemCommandManager)
      test_execute_build(TestGemCommandsCertCommand):
      test_execute_certificate(TestGemCommandsCertCommand)
      test_execute_private_key(TestGemCommandsCertCommand)
      test_execute_sign(TestGemCommandsCertCommand)
      test_execute_add(TestGemCommandsCertCommand)
      test_execute(TestGemCommandsEnvironmentCommand)
      test_no_user_install(TestGemCommandsInstallCommand)
      test_install_security_policy(TestGemDependencyInstaller)
      test_uninstall_doc_unwritable(TestGemDocManager)
      test_self_build_fail(TestGemExtConfigureBuilder)
      test_self_build(TestGemExtConfigureBuilder)
      test_class_build(TestGemExtExtConfBuilder)
      test_class_build_extconf_fail(TestGemExtExtConfBuilder)
      test_sign_in_skips_with_existing_credentials(TestGemGemcutterUtilities)
      test_sign_in_with_bad_credentials(TestGemGemcutterUtilities)
      test_sign_in(TestGemGemcutterUtilities)
      test_sign_in_with_host(TestGemGemcutterUtilities)
      test_sign_in_with_other_credentials_doesnt_overwrite_other_keys(TestGemGemcutterUtilities)
      test_generate_index(TestGemIndexer)
      test_update_index(TestGemIndexer)
      test_user_install_disabled_read_only(TestGemInstallUpdateOptions)
      test_generate_bin_script_no_perms(TestGemInstaller)
      test_generate_bin_symlink_no_perms(TestGemInstaller)
      test_build_extensions_extconf_bad(TestGemInstaller)
      test_self_open_signed(TestGemPackageTarOutput)
      test_self_open(TestGemPackageTarOutput)
      test_getc(TestGemPackageTarReaderEntry)
      test_explicit_proxy(TestGemRemoteFetcher)
      test_no_proxy(TestGemRemoteFetcher)
      test_self_load_specification_utf_8(TestGemSourceIndex)
      test_validate_empty_require_paths(TestGemSpecification)
      test_validate_files(TestGemSpecification)
      test_ask_for_password(TestGemStreamUI)
    }
  end
  
  def disable_mri_only_failures
    disable_by_name %w{
      test_self_prefix(TestGem)
      test_process_args_update(TestGemCommandManager)
      test_process_args_install(TestGemCommandManager)
      test_execute(TestGemCommandsEnvironmentCommand)
      test_no_user_install(TestGemCommandsInstallCommand)
      test_uninstall_doc_unwritable(TestGemDocManager)
      test_self_build_has_makefile(TestGemExtConfigureBuilder)
      test_self_build_fail(TestGemExtConfigureBuilder)
      test_self_build(TestGemExtConfigureBuilder)
      test_class_build(TestGemExtExtConfBuilder)
      test_class_make(TestGemExtExtConfBuilder)
      test_sign_in_with_host(TestGemGemcutterUtilities)
      test_sign_in_skips_with_existing_credentials(TestGemGemcutterUtilities)
      test_sign_in_with_other_credentials_doesnt_overwrite_other_keys(TestGemGemcutterUtilities)
      test_sign_in_with_bad_credentials(TestGemGemcutterUtilities)
      test_sign_in(TestGemGemcutterUtilities)
      test_generate_index_modern(TestGemIndexer)
      test_user_install_disabled_read_only(TestGemInstallUpdateOptions)
      test_generate_bin_symlink_no_perms(TestGemInstaller) 
      test_generate_bin_script_no_perms(TestGemInstaller)
      test_validate_files(TestGemSpecification)
      test_validate_empty_require_paths(TestGemSpecification)
      test_ask_for_password(TestGemStreamUI)
      test_update_index(TestGemIndexer)
      test_self_prefix(TestGem)
    }
  end
end
