class Array
  # RouteSet#deprecated_routes_for_controller_and_action_and_keys in actionpack\lib\action_controller\routing\route_set.rb in v2.3.5
  # assumes that sorting is stable (ie. if #<=> returns 0, then the relative order of the
  # elements in the original array is maintained). However, Ruby does not guarantee a stable order
  # according to ISO standard draft "15.3.2.1.19 Enumerable#sort" description and
  # http://redmine.ruby-lang.org/issues/show/1089.  
  #
  # Its unknown whether the failure in unit tests implies that Rails will misbehave (or if it just 
  # causes unoptimized routing for example).
  #
  def sort_by
    elements_with_indices = []
    each_with_index {|elem, i| elements_with_indices << [elem, i] }
    elements_with_indices.sort! do |a, b|
      c = yield(a[0]) <=> yield(b[0])
      c == 0 ? a[1] <=> b[1] : c
    end
    elements_with_indices.map {|a| a[0]}
  end
end

# We need to ensure that ActiveRecordTestConnector.able_to_connect (actionpack/test/active_record_unit.rb)
# always returns false. Otherwise, an attempt will be made to open a database connection,
# and it pops up a dialog box. We add this logic here so that we do not have to modify
# the Rails tests which makes it easy to update them from version to version.
class ActiveRecordTestConnector
  def self.singleton_method_added(name)
    if name == :able_to_connect and not @unpatching
      @unpatching = true
      def self.able_to_connect()
        false
      end 
    end
  end
end

class UnitTestSetup
  def initialize
    @name = "ActionPack"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= 2.0.5"
    gem 'actionmailer', "= 2.3.5"
    gem 'activerecord', "= 2.3.5"
    gem 'activesupport', "= 2.3.5"
    gem 'actionpack', "= 2.3.5"
    require 'action_pack/version'
    require 'active_record'
    require 'active_record/fixtures'
  end

  def gather_files
    @root_dir = File.expand_path '..\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-2.3.5\actionpack', ENV['MERLIN_ROOT']
    $LOAD_PATH << @root_dir + '/test'
    @all_test_files = Dir.glob("#{@root_dir}/test/[cft]*/**/*_test.rb").sort
  end

  def sanity
    # Do some sanity checks
    sanity_size(80)
    abort("Did not find some expected files...") unless File.exist?(@root_dir + "/test/controller/action_pack_assertions_test.rb")
    sanity_version('2.3.5', ActionPack::VERSION::STRING)
  end

  def disable_mri_failures
    disable CleanBacktraceTest, 
      # <["d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/lib/action_controller/abc"]> expected but was
      # <["d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/lib/action_controller/abc",
      #  "d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/lib/action_controll
      :test_should_clean_assertion_lines_from_backtrace
    
    disable LayoutSetInResponseTest,
      # <"abs_path_layout.rhtml hello.rhtml"> expected but was
      # Exception caught
      # Template is missing
      # Missing layout layouts/d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/test/fixtures/layout_tests/abs_path_layout.rhtml in view path 
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/test/controller/../fixtures/layout_tests.
      :test_absolute_pathed_layout_without_layouts_in_path
      
    disable AssetTagHelperTest,
     # Errno::EACCES: Permission denied - d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/test/template/../fixtures/public/absolute/test.css
      :test_concat_stylesheet_link_tag_when_caching_off

    disable CompiledTemplatesTest,
      #Errno::EACCES: Access to the path 'Merlin\External.LCA_RESTRICTED\Languages\IronRuby\RailsTests-2.3.5\actionpack\test\fixtures\test\hello_world.erb' is denied.
      :test_parallel_reloadable_view_paths_are_working,
      :test_template_changes_are_not_reflected_with_cached_template_loading,
      :test_template_changes_are_reflected_without_cached_template_loading
  end
  
  def disable_tests
    disable ActionCacheTest,
      # <"<title></title>\n1264061287.72215"> expected but was
      # <nil>.
      :test_action_cache_with_layout
      
    disable AssetTagHelperTest, 
      # <"<script src=\"/javascripts/prototype.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/effects.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/dragdrop.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/controls.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/slider.js\" type=\"text/javascript\"></script>\n<script src=\"/j
      :test_register_javascript_include_default_mixed_defaults,
      # <"<script src=\"/javascripts/first.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/prototype.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/effects.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/dragdrop.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/controls.js\" type=\"text/javascript\"></script>\n<script src=\"/ja
      :test_custom_javascript_expansions_and_defaults_puts_application_js_at_the_end,
      # NameError: uninitialized constant Test::Unit::Diff::ReadableDiffer::Encoding
      :test_preset_empty_asset_id,
      # <"<script src=\"/javascripts/prototype.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/effects.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/dragdrop.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/controls.js\" type=\"text/javascript\"></script>\n<script src=\"/javascripts/slider.js\" type=\"text/javascript\"></script>\n<script src=\"/j
      :test_register_javascript_include_default

    disable CachedRenderTest, 
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x000bacc @_rendered={:template=>#<ActionView::Template:0x0004544 @template_path=\"test/_raise.html.erb\", @load_path=\"d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/test/fixtures\", @base_path=\"test\", @name=\"_raise\", @locale=nil, @format=\"html\", @extension=\"erb\", @_memoized_variab
      :test_render_partial_with_errors,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x000bba8 @_rendered={:template=>#<ActionView::Template:0x000450e @template_path=\"test/sub_template_raise.html.erb\", @load_path=\"d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/test/fixtures\", @base_path=\"test\", @name=\"sub_template_raise\", @locale=nil, @format=\"html\", @extension=\"
      :test_render_sub_template_with_errors

    disable IsolatedHelpersTest, 
      # <NameError> exception expected but was
      # Class: <NoMethodError>
      # Message: <"undefined method `shout' for #<ActionView::Base:0x0041ca8 @_rendered={:template=>nil, :partials=>{}}, @assigns={}, @assigns_added=true, @controller=#<IsolatedHelpersTest::A:0x0041ca6 @before_filter_chain_aborted=false, @performed_redirect=false, @performed_render=false, @_request=#<ActionController::TestRequest:0x0041c98 @env=
      :test_helper_in_a

    disable ReloadableRenderTest, 
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x004e15a @_rendered={:template=>#<ActionView::ReloadableTemplate:0x004e1e6 @template_path=\"test/_raise.html.erb\", @load_path=\"d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/test/fixtures\", @base_path=\"test\", @name=\"_raise\", @locale=nil, @format=\"html\", @extension=\"erb\", @_memoi
      :test_render_partial_with_errors,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x004e51e @_rendered={:template=>#<ActionView::ReloadableTemplate:0x004e574 @template_path=\"test/sub_template_raise.html.erb\", @load_path=\"d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/actionpack/test/fixtures\", @base_path=\"test\", @name=\"sub_template_raise\", @locale=nil, @format=\"html\", @ex
      :test_render_sub_template_with_errors

    disable RenderTest, 
      # The line offset is wrong, perhaps the wrong exception has been raised, exception was: #<RuntimeError: Exception of type 'IronRuby.Builtins.RuntimeError' was thrown.>.
      # <"1"> expected but was
      # <"2">.
      :test_line_offset

  end
end
