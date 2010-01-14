class UnitTestSetup
  def initialize
    @name = "ActionPack"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'actionpack', "= 2.3.3"
    require 'action_pack/version'
  end

  def gather_files
    @root_dir = File.expand_path '..\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-2.3.3\actionpack', ENV['MERLIN_ROOT']
    $LOAD_PATH << @root_dir + '/test'
    @all_test_files = Dir.glob("#{@root_dir}/test/[cft]*/**/*_test.rb").sort
  end

  def sanity
    # Do some sanity checks
    sanity_size(80)
    abort("Did not find some expected files...") unless File.exist?(@root_dir + "/test/controller/action_pack_assertions_test.rb")
    sanity_version('2.3.3', ActionPack::VERSION::STRING)
  end

  def disable_tests
    disable LastModifiedRenderTest,
      # <"Mon, 07 Sep 2009 00:00:00 GMT"> expected but was
      # <"Sun, 06 Sep 2009 23:59:59 GMT">.
      :test_request_not_modified,
      # Expected response to be a <:not_modified>, but was <200>
      :test_request_with_bang_obeys_last_modified,
      # <"Sun, 06 Sep 2009 23:59:59 GMT"> expected but was
      # <"Mon, 07 Sep 2009 00:00:00 GMT">.
      :test_request_with_bang_gets_last_modified

    disable ActionCacheTest,
      # <false> is not true.
      :test_action_cache_with_custom_cache_path,
      # <false> is not true.
      :test_action_cache_with_custom_cache_path_in_block,
      # <false> is not true.
      :test_action_cache_with_layout,
      # <false> is not true.
      :test_action_cache_with_layout_and_layout_cache_false,
      # not all expectations were satisfied
      # unsatisfied expectations:
      # - expected exactly once, not yet invoked: #<ActionCachingTestController:0x411e>.write_fragment('hostname.com/action_caching_test', '12345.0', {:expires_in => 3600})
      # - expected exactly once, not yet invoked: #<ActionCachingTestController:0x411e>.read_fragment('hostname.com/action_caching_test', {:expires_in => 3600})
      # satisfied expectation
      :test_action_cache_with_store_options,
      # <"1252314416.2022"> expected but was
      # <"1252314416.3114">.
      :test_cache_expiration,
      # <"1252314416.96659"> expected but was
      # <"1252314417.29419">.
      :test_cache_is_scoped_by_subdomain,
      # <"application/xml"> expected but was
      # <"text/html">.
      :test_correct_content_type_is_returned_for_cache_hit,
      # <"application/xml"> expected but was
      # <"text/html">.
      :test_correct_content_type_is_returned_for_cache_hit_on_action_with_string_key,
      # <"application/xml"> expected but was
      # <"text/html">.
      :test_correct_content_type_is_returned_for_cache_hit_on_action_with_string_key_from_proc,
      # <false> is not true.
      :test_simple_action_cache,
      # <false> is not true.
      :test_xml_version_of_resource_is_treated_as_different_cache

    disable AssertSelectTest,
      # NameError: uninitialized constant Iconv::IllegalSequence
      :test_assert_select_email,
      # </ã?..ã??/u> expected but was
      # <"\343\203\201\343\202\261\343\203\203\343\203\210">.
      # <false> is not true.
      :test_assert_select_rjs_with_unicode

    disable AssetTagHelperTest,
      # <"<script src=\"/javascripts/prototype.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/effects.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/dragdrop.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/controls.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/application.js\" type=\"text/javascript\"></script>\n<script src
      :test_caching_javascript_include_tag_when_caching_off,
      # <"<script src=\"http://a0.example.com/javascripts/all.js\" type=\"text/javascript\"></script>"> expected to be == to
      # <"<script src=\"http://a0.example.com/javascripts/all.js?1245781105\" type=\"text/javascript\"></script>">.
      :test_caching_javascript_include_tag_when_caching_on,
      # <"<script src=\"http://a1.example.com/javascripts/cache/money.js\" type=\"text/javascript\"></script>"> expected to be == to
      # <"<script src=\"http://a2.example.com/javascripts/cache/money.js?1245781105\" type=\"text/javascript\"></script>">.
      :test_caching_javascript_include_tag_when_caching_on_and_using_subdirectory,
      # <"<script src=\"http://assets23.example.com/javascripts/vanilla.js\" type=\"text/javascript\"></script>"> expected to be == to
      # <"<script src=\"http://assets34.example.com/javascripts/vanilla.js?1245781105\" type=\"text/javascript\"></script>">.
      :test_caching_javascript_include_tag_when_caching_on_with_2_argument_object_asset_host,
      # <"<script src=\"http://assets23.example.com/javascripts/vanilla.js\" type=\"text/javascript\"></script>"> expected to be == to
      # <"<script src=\"http://assets34.example.com/javascripts/vanilla.js?1245781105\" type=\"text/javascript\"></script>">.
      :test_caching_javascript_include_tag_when_caching_on_with_2_argument_proc_asset_host,
      # <"<script src=\"http://a23.example.com/javascripts/scripts.js\" type=\"text/javascript\"></script>"> expected to be == to
      # <"<script src=\"http://a34.example.com/javascripts/scripts.js?1245781105\" type=\"text/javascript\"></script>">.
      :test_caching_javascript_include_tag_when_caching_on_with_proc_asset_host,
      # <"<script src=\"http://a0.example.com/javascripts/combined.js\" type=\"text/javascript\"></script>"> expected to be == to
      # <"<script src=\"http://a0.example.com/javascripts/combined.js?1245781105\" type=\"text/javascript\"></script>">.
      :test_caching_javascript_include_tag_with_all_and_recursive_puts_defaults_at_the_start_of_the_file,
      # <"<script src=\"http://a0.example.com/javascripts/combined.js\" type=\"text/javascript\"></script>"> expected to be == to
      # <"<script src=\"http://a0.example.com/javascripts/combined.js?1245781105\" type=\"text/javascript\"></script>">.
      :test_caching_javascript_include_tag_with_all_puts_defaults_at_the_start_of_the_file,
      # <"<script src=\"/collaboration/hieraki/javascripts/all.js\" type=\"text/javascript\"></script>"> expected to be == to
      # <"<script src=\"/collaboration/hieraki/javascripts/all.js?1245781105\" type=\"text/javascript\"></script>">.
      :test_caching_javascript_include_tag_with_relative_url_root,
      # <"<link href=\"/stylesheets/bank.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />\n<link href=\"/stylesheets/robber.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />\n<link href=\"/stylesheets/version.1.0.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />"> expected to be == to
      # <"<link href=\"/stylesheets/bank.css?1245781105\" media=\"screen\" rel=\"stylesheet
      :test_caching_stylesheet_include_tag_when_caching_off,
      # <"<link href=\"http://a0.example.com/stylesheets/all.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />"> expected to be == to
      # <"<link href=\"http://a0.example.com/stylesheets/all.css?1245781105\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />">.
      :test_caching_stylesheet_link_tag_when_caching_on,
      # <"<link href=\"http://a23.example.com/stylesheets/styles.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />"> expected to be == to
      # <"<link href=\"http://a34.example.com/stylesheets/styles.css?1245781105\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />">.
      :test_caching_stylesheet_link_tag_when_caching_on_with_proc_asset_host,
      # <"<link href=\"/collaboration/hieraki/stylesheets/all.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />"> expected to be == to
      # <"<link href=\"/collaboration/hieraki/stylesheets/all.css?1245781105\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />">.
      :test_caching_stylesheet_link_tag_with_relative_url_root,
      # <"<link href=\"/stylesheets/all.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />"> expected to be == to
      # <"<link href=\"/stylesheets/all.css?1245781105\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />">.
      :test_concat_stylesheet_link_tag_when_caching_off,
      # <"<script src=\"/javascripts/first.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/prototype.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/effects.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/dragdrop.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/controls.js\" type=\"text/javascript\"></script>\n<script src=\"/ja
      :test_custom_javascript_expansions_and_defaults_puts_application_js_at_the_end,
      # <"<script src=\"/javascripts/prototype.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/effects.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/dragdrop.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/controls.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/application.js\" type=\"text/javascript\"></script>"> expected t
      :test_javascript_include_tag_with_blank_asset_id,
      # <"<img alt=\"Rails\" src=\"/images/rails.png\" />"> expected but was
      # <"<img alt=\"Rails\" src=\"/images/rails.png?1245781105\" />">.
      :test_preset_empty_asset_id,
      # <"<script src=\"/javascripts/prototype.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/effects.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/dragdrop.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/controls.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/slider.js\" type=\"text/javascript\"></script>\n<script src=\"/j
      :test_register_javascript_include_default,
      # <"<script src=\"/javascripts/prototype.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/effects.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/dragdrop.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/controls.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/slider.js\" type=\"text/javascript\"></script>\n<script src=\"/j
      :test_register_javascript_include_default_mixed_defaults,
      # <"<link href=\"/stylesheets/bank.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />\n<link href=\"/stylesheets/robber.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />\n<link href=\"/stylesheets/version.1.0.css\" media=\"screen\" rel=\"stylesheet\" type=\"text/css\" />"> expected to be == to
      # <"<link href=\"/stylesheets/bank.css?1245781105\" media=\"screen\" rel=\"stylesheet
      :test_stylesheet_link_tag

    disable CachedRenderTest,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x000b208 @_rendered={:template=>#<ActionView::Template:0x0003f18 @template_path=\"test/_raise.html.erb\", @load_path=\"c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/fixtures\", @base_path=\"test\", @name=\"_raise\", @locale=nil, @format=\"html\", @extension=\"erb\", @_memoized_variable_
      :test_render_partial_with_errors,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x000b2bc @_rendered={:template=>#<ActionView::Template:0x0003ee2 @template_path=\"test/sub_template_raise.html.erb\", @load_path=\"c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/fixtures\", @base_path=\"test\", @name=\"sub_template_raise\", @locale=nil, @format=\"html\", @extension=\"erb
      :test_render_sub_template_with_errors

    disable CookieTest,
      # <["user_name=; path=/beaten; expires=Thu, 01-Jan-1970 00:00:00 GMT"]> expected but was
      # <[]>.
      :test_delete_cookie_with_path,
      # <["user_name=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT"]> expected but was
      # <[]>.
      :test_expiring_cookie,
      # <2> expected but was
      # <0>.
      :test_multiple_cookies,
      # <["user_name=david; path=/; expires=Mon, 10-Oct-2005 05:00:00 GMT"]> expected but was
      # <[]>.
      :test_setting_cookie_for_fourteen_days,
      # <["user_name=david; path=/; expires=Mon, 10-Oct-2005 05:00:00 GMT"]> expected but was
      # <[]>.
      :test_setting_cookie_for_fourteen_days_with_symbols

    disable IntegrationProcessTest,
      # <200> expected but was
      # <500>.
      :test_multipart_post_with_multiparameter_attribute_parameters

    disable IsolatedHelpersTest,
      # <NameError> exception expected but was
      # Class: <#<Class:0x0001ea2 @inheritable_attributes={}>>
      # Message: <"undefined method `shout' for #<ActionView::Base:0x00415fc @_rendered={:template=>nil, :partials=>{}}, @assigns={}, @assigns_added=true, @controller=#<IsolatedHelpersTest::A:0x00415fa @before_filter_chain_aborted=false, @performed_redirect=false, @performed_render=false, @_request=#<ActionControl
      :test_helper_in_a

    disable LastModifiedRenderTest,
      # <"Mon, 07 Sep 2009 00:00:00 GMT"> expected but was
      # <"Sun, 06 Sep 2009 23:59:59 GMT">.
      :test_request_modified,
      # <"Sun, 06 Sep 2009 23:59:59 GMT"> expected but was
      # <"Mon, 07 Sep 2009 00:00:00 GMT">.
      :test_responds_with_last_modified

    disable LegacyRouteSetTests,
      # <"/page/20"> expected but was
      # <"/pages/show/20">.
      :test_backwards,
      # <"/test"> expected but was
      # <"/post/show">.
      :test_both_requirement_and_optional,
      # <"/categories"> expected but was
      # <"/content/categories">.
      :test_named_route_method,
      # <"/categories"> expected but was
      # <"/content/categories">.
      :test_named_routes_array,
      # <"/"> expected but was
      # <"/content">.
      :test_named_url_with_no_action_specified,
      # <"/journal"> expected but was
      # <"/content/list_journal">.
      :test_nil_defaults,
      # <"/page"> expected but was
      # <"/content/show_page">.
      :test_route_with_fixnum_default,
      # <"/page/foo"> expected but was
      # <"/content/show_page/foo">.
      :test_route_with_text_default,
      # <"/pages/2005/6"> expected but was
      # <"/content/list_pages">.
      :test_set_to_nil_forgets,
      # <"/books/7/edit"> expected but was
      # <"/items/7/edit">.
      :test_subpath_generated,
      # <"/"> expected but was
      # <"/content">.
      :test_url_generated_when_forgetting_action,
      # <"/"> expected but was
      # <"/content">.
      :test_url_with_no_action_specified

    disable MultipartParamsParsingTest,
      # NoMethodError: undefined method `stat' for #<File:c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/controller/request/../../fixtures/multipart/none>
      :test_does_not_create_tempfile_if_no_file_has_been_selected,
      # NoMethodError: undefined method `stat' for #<File:c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/controller/request/../../fixtures/multipart/binary_file>
      :test_parses_binary_file,
      # NoMethodError: undefined method `stat' for #<File:c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/controller/request/../../fixtures/multipart/boundary_problem_file>
      :test_parses_boundary_problem_file,
      # NoMethodError: undefined method `stat' for #<File:c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/controller/request/../../fixtures/multipart/bracketed_param>
      :test_parses_bracketed_parameters,
      # NoMethodError: undefined method `stat' for #<File:c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/controller/request/../../fixtures/multipart/empty>
      :test_parses_empty_upload_file,
      # NoMethodError: undefined method `stat' for #<File:c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/controller/request/../../fixtures/multipart/large_text_file>
      :test_parses_large_text_file,
      # NoMethodError: undefined method `stat' for #<File:c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/controller/request/../../fixtures/multipart/mixed_files>
      :test_parses_mixed_files,
      # NoMethodError: undefined method `stat' for #<File:c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/controller/request/../../fixtures/multipart/single_parameter>
      :test_parses_single_parameter,
      # NoMethodError: undefined method `stat' for #<File:c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/controller/request/../../fixtures/multipart/text_file>
      :test_parses_text_file

    disable PageCachingTest,
      # <"/"> expected but was
      # <"/posts">.
      :test_page_caching_resources_saves_to_correct_path_with_extension_even_if_default_route,
      # get with ok status should have been cached.
      # <false> is not true.
      :test_should_cache_get_with_ok_status,
      # <false> is not true.
      :test_should_cache_ok_at_custom_path

    disable ReloadableRenderTest,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x004d78c @_rendered={:template=>#<ActionView::ReloadableTemplate:0x004d816 @template_path=\"test/_raise.html.erb\", @load_path=\"c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/fixtures\", @base_path=\"test\", @name=\"_raise\", @locale=nil, @format=\"html\", @extension=\"erb\", @_memoized
      :test_render_partial_with_errors,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x004db28 @_rendered={:template=>#<ActionView::ReloadableTemplate:0x004db7c @template_path=\"test/sub_template_raise.html.erb\", @load_path=\"c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/fixtures\", @base_path=\"test\", @name=\"sub_template_raise\", @locale=nil, @format=\"html\", @exten
      :test_render_sub_template_with_errors

    disable RenderTest,
      # NameError: uninitialized constant Mocha::TestCaseAdapter
      :test_line_offset

    disable RescueControllerTest,
      # NoMethodError: undefined method `message' for nil:NilClass
      :test_block_rescue_handler_with_argument,
      # NoMethodError: undefined method `message' for nil:NilClass
      :test_block_rescue_handler_with_argument_as_string,
      # NoMethodError: undefined method `message' for nil:NilClass
      :test_proc_rescue_handle_with_argument,
      # NoMethodError: undefined method `message' for nil:NilClass
      :test_proc_rescue_handle_with_argument_as_string

    disable RouteSetTest,
      # <"/about"> expected but was
      # <"/welcome/about">.
      :test_use_static_path_when_possible

    disable TestTest,
      # NoMethodError: undefined method `stat' for #<File:C:/Users/sborde/AppData/Local/Temp/mona_lisa.jpg.13276.10>
      :test_fixture_file_upload

    disable UrlWriterTests,
      # <"http://www.basecamphq.com/"> expected but was
      # <"http://www.basecamphq.com/posts">.
      :test_named_routes_with_nil_keys

    disable VerificationTest,
      # ActionView::MissingTemplate: Missing template verification_test/test/no_default_action.erb in view path c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/test/fixtures
      :test_default_failure_should_be_a_bad_request,
      # Expected response to be a <:redirect>, but was <200>
      :test_guarded_by_method_without_prereqs,
      # Expected response to be a <:redirect>, but was <200>
      :test_guarded_by_not_xhr_without_prereqs,
      # Expected response to be a <:redirect>, but was <200>
      :test_guarded_by_xhr_without_prereqs,
      # Expected response to be a <:redirect>, but was <200>
      :test_guarded_in_session_without_prereqs,
      # Expected response to be a <:redirect>, but was <200>
      :test_guarded_one_without_prereqs,
      # Expected response to be a <405>, but was <200>
      :test_guarded_post_and_calls_render_fails_and_sets_allow_header,
      # Expected response to be a <:redirect>, but was <200>
      :test_guarded_two_without_prereqs_both,
      # Expected response to be a <:redirect>, but was <200>
      :test_guarded_two_without_prereqs_one,
      # Expected response to be a <:redirect>, but was <200>
      :test_guarded_two_without_prereqs_two,
      # Expected response to be a <:redirect>, but was <200>
      :test_guarded_with_flash_without_prereqs,
      # Expected response to be a <:redirect>, but was <200>
      :test_multi_one_without_prereqs,
      # Expected response to be a <:redirect>, but was <200>
      :test_multi_two_without_prereqs,
      # Expected response to be a <:redirect>, but was <200>
      :test_no_deprecation_warning_for_named_route,
      # Expected response to be a redirect to <http://test.host/foo> but was a redirect to <http://test.host/verification_test/test/unguarded>.
      :test_using_symbol_back_redirects_to_referrer,
      # <#<Class:0x0001aba @inheritable_attributes={}>> exception expected but none was thrown.
      :test_using_symbol_back_with_no_referrer

    disable CleanBacktraceTest,
      # <["c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/lib/action_controller/abc"]> expected but was
      # <["c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/lib/action_controller/abc",
      #  "c:/github/ironruby/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/actionpack/lib/action_controller/assert
      :test_should_clean_assertion_lines_from_backtrace

    disable CompiledTemplatesTest,
      #Errno::EACCES: Access to the path 'Merlin\External.LCA_RESTRICTED\Languages\IronRuby\RailsTests-2.3.3\actionpack\test\fixtures\test\hello_world.erb' is denied.
      :test_parallel_reloadable_view_paths_are_working,
      :test_template_changes_are_not_reflected_with_cached_template_loading,
      :test_template_changes_are_reflected_without_cached_template_loading
  end
end
