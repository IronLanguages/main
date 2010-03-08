class Array
  # RenderRjs::TestBasic#test_rendering_a_partial_in_an_RJS_template_should_pick_the_JS_template_over_the_HTML_one
  # relies on stable sorting (ie. if #<=> returns 0, then the relative order of the
  # elements in the original array is maintained) somewhere. However, Ruby does not guarantee a stable order
  # according to ISO standard draft "15.3.2.1.19 Enumerable#sort" description and
  # http://redmine.ruby-lang.org/issues/show/1089.  
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
    gem 'actionmailer', "= 3.0.pre"
    gem 'activerecord', "= 3.0.pre"
    gem 'activesupport', "= 3.0.pre"
    gem 'actionpack', "= 3.0.pre"
    require 'action_pack/version'
    require 'active_record'
    require 'active_record/fixtures'
  end

  def gather_files
    @root_dir = File.expand_path '..\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack', ENV['MERLIN_ROOT']
    $LOAD_PATH << @root_dir
    @all_test_files = Dir.glob("#{@root_dir}/[cft]*/**/*_test.rb").sort
  end

  def sanity
    # Do some sanity checks
    sanity_size(80)
    abort("Did not find some expected files...") unless File.exist?(@root_dir + "/controller/action_pack_assertions_test.rb")
    sanity_version('3.0.pre', ActionPack::VERSION::STRING)
  end

  def disable_unstable_tests
  
    disable LegacyRouteSetTests,
      # <"/page/%D1%82%D0%B5%D0"> expected but was
      # <"/page/%D1%82%D0%B5%D0%BA%D1%81%D1%82">.
      :test_route_with_text_default

    disable NumberHelperTest,
      # <"555-1234"> expected but was
      # <"-555-1234">.
      # diff:
      # - 555-1234
      # + -555-1234
      # ? +
      :test_number_to_phone

    disable TextHelperTest,
      # <"...\xEF\xAC\x83ciency could not be..."> expected but was
      # <"...\x83ciency could not be...">.
      # diff:
      # - ...n¼âciency could not be...
      # ?    --
      # + ...âciency could not be...
      :test_excerpt_with_utf8,
      # <"\xEC\x95\x84\xEB\xA6\xAC\xEB\x9E\x91 \xEC\x95\x84\xEB\xA6\xAC ..."> expected b
      # ut was
      # <"\xEC\x95\x84\xEB\xA6\xAC\xEB...">.
      :test_truncate_multibyte
  end
  
  def disable_tests

    disable ActionCacheTest, 
      # <false> is not true.
      :test_xml_version_of_resource_is_treated_as_different_cache

    disable AssetTagHelperTest, 
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\javascripts\all.js' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_javascript_include_tag_when_caching_on,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\javascripts\cache\money.js' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_javascript_include_tag_when_caching_on_and_using_subdirectory,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\javascripts\vanilla.js' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_javascript_include_tag_when_caching_on_with_2_argument_object_asset_host,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\javascripts\vanilla.js' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_javascript_include_tag_when_caching_on_with_2_argument_proc_asset_host,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\javascripts\scripts.js' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_javascript_include_tag_when_caching_on_with_proc_asset_host,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\javascripts\combined.js' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_javascript_include_tag_with_all_and_recursive_puts_defaults_at_the_start_of_the_file,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\javascripts\combined.js' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_javascript_include_tag_with_all_puts_defaults_at_the_start_of_the_file,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\javascripts\all.js' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_javascript_include_tag_with_relative_url_root,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\stylesheets\all.css' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_stylesheet_link_tag_when_caching_on,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\stylesheets\styles.css' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_stylesheet_link_tag_when_caching_on_with_proc_asset_host,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\stylesheets\all.css' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_caching_stylesheet_link_tag_with_relative_url_root,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\public\stylesheets\all.css' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_concat_stylesheet_link_tag_when_caching_off

    disable CachedViewRenderTest, 
      # ActionView::MissingTemplate: Missing template /d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/template/../fixtures/test/hello_world.erb - {:formats=>nil} - partial: false in view path d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/actionpack-3.0.pre/lib/action_view/paths.rb:48:in `find'
      # d:2:in `find'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:835:in `send'
      :test_render_file_with_full_path,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x0024de6 @_rendered={:template=>[d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/_raise.html.erb], :partials=>{d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/_raise.html.erb=>1}}, @formats=nil, @secret=\"in
      :test_render_partial_with_errors,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x002501a @_rendered={:template=>[d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/sub_template_raise.html.erb, d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/_raise.html.erb], :partials=>{d:/vs_langs01_s/Mer
      :test_render_sub_template_with_errors

    disable CompiledTemplatesTest, 
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\test\hello_world.erb' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_template_changes_are_not_reflected_with_cached_templates,
      # Errno::EACCES: Access to the path 'd:\vs_langs01_s\Merlin\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-3.0.pre\actionpack\fixtures\test\hello_world.erb' is denied.
      # mscorlib:0:in `WinIOError'
      # mscorlib:0:in `Init'
      # mscorlib:0:in `.ctor'
      :test_template_changes_are_reflected_with_uncached_templates

    disable FormTagHelperTest, 
      # <"<select id=\"people_\" multiple=\"multiple\" name=\"people[]\"><option>david</option></select>"> expected to be == to
      # <"<select id=\"people_\" multiple=\"multiple\" name=\"people[][]\"><option>david</option></select>">.
      :test_boolean_options

    disable LayoutSetInResponseTest, 
      # ActionView::MissingTemplate: Missing layout /d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/layout_tests/layouts/layout_test.rhtml - {} - partial: false in view path d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/layout_tests
      # paths.rb:39:in `find'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/actionpack-3.0.pre/lib/abstract_controller/layouts.rb:217:in `_find_layout'
      # rendering_controller.rb:136:in `find_template'
      :test_absolute_pathed_layout

    disable LazyViewRenderTest, 
      # ActionView::MissingTemplate: Missing template /d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/template/../fixtures/test/hello_world.erb - {:formats=>nil} - partial: false in view path d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures
      # paths.rb:39:in `find'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:835:in `send'
      # d:1:in `find'
      :test_render_file_with_full_path,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x00ae23c @_rendered={:template=>[d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/_raise.html.erb], :partials=>{d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/_raise.html.erb=>1}}, @formats=nil, @secret=\"in
      :test_render_partial_with_errors,
      # <"undefined method `doesnt_exist' for #<ActionView::Base:0x00ae480 @_rendered={:template=>[d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/sub_template_raise.html.erb, d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/_raise.html.erb], :partials=>{d:/vs_langs01_s/Mer
      :test_render_sub_template_with_errors

    disable RenderFile::TestBasic, 
      # <"The secret is in the sauce\n"> expected but was
      # <"<html xmlns=\"http://www.w3.org/1999/xhtml\">\n<head>\n  <title>Action Controller: Exception caught</title>\n  <style>\n    body { background-color: #fff; color: #333; }\n\n    body, p, ol, ul, td {\n      font-family: verdana, arial, helvetica, sans-serif;\n      font-size:   13px;\n      line-height: 18px;\n    }\n\n    pre {\n      background-c
      :test_rendering_a_Pathname,
      # <"The secret is in the sauce\n"> expected but was
      # <"<html xmlns=\"http://www.w3.org/1999/xhtml\">\n<head>\n  <title>Action Controller: Exception caught</title>\n  <style>\n    body { background-color: #fff; color: #333; }\n\n    body, p, ol, ul, td {\n      font-family: verdana, arial, helvetica, sans-serif;\n      font-size:   13px;\n      line-height: 18px;\n    }\n\n    pre {\n      background-c
      :test_rendering_file_with_locals,
      # <"Hello world!"> expected but was
      # <"<html xmlns=\"http://www.w3.org/1999/xhtml\">\n<head>\n  <title>Action Controller: Exception caught</title>\n  <style>\n    body { background-color: #fff; color: #333; }\n\n    body, p, ol, ul, td {\n      font-family: verdana, arial, helvetica, sans-serif;\n      font-size:   13px;\n      line-height: 18px;\n    }\n\n    pre {\n      background-color: #eee;\n   
      "test_rendering_path_without_specifying_the_:file_key",
      # <"The secret is in the sauce\n"> expected but was
      # <"<html xmlns=\"http://www.w3.org/1999/xhtml\">\n<head>\n  <title>Action Controller: Exception caught</title>\n  <style>\n    body { background-color: #fff; color: #333; }\n\n    body, p, ol, ul, td {\n      font-family: verdana, arial, helvetica, sans-serif;\n      font-size:   13px;\n      line-height: 18px;\n    }\n\n    pre {\n      background-c
      "test_rendering_path_without_specifying_the_:file_key_with_ivar",
      # <"The secret is in the sauce\n"> expected but was
      # <"<html xmlns=\"http://www.w3.org/1999/xhtml\">\n<head>\n  <title>Action Controller: Exception caught</title>\n  <style>\n    body { background-color: #fff; color: #333; }\n\n    body, p, ol, ul, td {\n      font-family: verdana, arial, helvetica, sans-serif;\n      font-size:   13px;\n      line-height: 18px;\n    }\n\n    pre {\n      background-c
      "test_rendering_path_without_specifying_the_:file_key_with_locals",
      # <"Hello world!"> expected but was
      # <"<html xmlns=\"http://www.w3.org/1999/xhtml\">\n<head>\n  <title>Action Controller: Exception caught</title>\n  <style>\n    body { background-color: #fff; color: #333; }\n\n    body, p, ol, ul, td {\n      font-family: verdana, arial, helvetica, sans-serif;\n      font-size:   13px;\n      line-height: 18px;\n    }\n\n    pre {\n      background-color: #eee;\n   
      :test_rendering_simple_template,
      # <"The secret is in the sauce\n"> expected but was
      # <"<html xmlns=\"http://www.w3.org/1999/xhtml\">\n<head>\n  <title>Action Controller: Exception caught</title>\n  <style>\n    body { background-color: #fff; color: #333; }\n\n    body, p, ol, ul, td {\n      font-family: verdana, arial, helvetica, sans-serif;\n      font-size:   13px;\n      line-height: 18px;\n    }\n\n    pre {\n      background-c
      :test_rendering_template_with_ivar

    disable RenderJSTest, 
      # <"$(\"body\").visualEffect(\"highlight\");"> expected but was
      # <"<p>This is grand!</p>\n">.
      :test_render_with_default_from_accept_header

    disable RenderTest, 
      # The line offset is wrong, perhaps the wrong exception has been raised, exception was: #<ActionView::Template::Error: ActionView::Template::Error>.
      # <"1"> expected but was
      # <"2">.
      :test_line_offset,
      # ActionView::MissingTemplate: Missing template /d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/render_file_with_ivar.erb - {:formats=>[:html]} - partial: false in view path d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures
      # paths.rb:39:in `find'
      # rendering_controller.rb:136:in `find_template'
      # rendering_controller.rb:131:in `_determine_template'
      :test_render_file_as_string_with_instance_variables,
      # ActionView::MissingTemplate: Missing template /d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/render_file_with_locals.erb - {:formats=>[:html]} - partial: false in view path d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures
      # paths.rb:39:in `find'
      # rendering_controller.rb:136:in `find_template'
      # rendering_controller.rb:131:in `_determine_template'
      :test_render_file_as_string_with_locals,
      # ActionView::Template::Error: Missing template /d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/render_file_with_ivar.erb - {:formats=>nil} - partial: false in view path d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures
      # On line #2 of d:/vs_langs01_s/merlin/external.lca_restricted/languages/ironruby/tests/railstests-3.0.pre/actionpack/fixtures/test/render_file_from_template.html.erb
      # 
      #     1: <%= render :file => @path %>
      # 
      #     paths.rb:39:in `find'
      #     D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:835:in `send'
      #     d:1:in `find'
      #     rendering.rb:13:in `render'
      #     d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/render_file_from_template.html.erb:2:in `_render_template_728241167_632135_0'
      #     D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:812:in `__send__'
      #     template.rb:37:in `render'
      #     instrumenter.rb:12:in `instrument'
      #     D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:835:in `send'
      #     d:1:in `instrument'
      #     D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:835:in `
      :test_render_file_from_template,
      # ActionView::MissingTemplate: Missing template /d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures/test/dot.directory/render_file_with_ivar.erb - {:formats=>[:html]} - partial: false in view path d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures
      # paths.rb:39:in `find'
      # rendering_controller.rb:136:in `find_template'
      # rendering_controller.rb:131:in `_determine_template'
      :test_render_file_using_pathname,
      # ActionView::MissingTemplate: Missing template /d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/controller/../fixtures/test/render_file_with_ivar.erb - {:formats=>[:html]} - partial: false in view path d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures
      # paths.rb:39:in `find'
      # rendering_controller.rb:136:in `find_template'
      # rendering_controller.rb:131:in `_determine_template'
      :test_render_file_with_instance_variables,
      # ActionView::MissingTemplate: Missing template /d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/controller/../fixtures/test/render_file_with_locals.erb - {:formats=>[:html]} - partial: false in view path d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/fixtures
      # paths.rb:39:in `find'
      # rendering_controller.rb:136:in `find_template'
      # rendering_controller.rb:131:in `_determine_template'
      :test_render_file_with_locals

    disable SendFileTest, 
      # Exception raised:
      # Class: <NoMethodError>
      # Message: <"undefined method `bytesize' for \"# encoding: utf-8\\r\\nrequire 'abstract_unit'\\r\\n\\r\\nmodule TestFileUtils\\r\\n  def file_name() File.basename(__FILE__) end\\r\\n  def file_path() File.expand_path(__FILE__) end\\r\\n  def file_data() @data ||= File.open(file_path, 'rb') { |f| f.read } end\\r\\nend\\r\\n\\r\\nclass SendFileController < Actio
      :test_data,
      # Exception raised:
      # Class: <NoMethodError>
      # Message: <"undefined method `bytesize' for \"# encoding: utf-8\\r\\nrequire 'abstract_unit'\\r\\n\\r\\nmodule TestFileUtils\\r\\n  def file_name() File.basename(__FILE__) end\\r\\n  def file_path() File.expand_path(__FILE__) end\\r\\n  def file_data() @data ||= File.open(file_path, 'rb') { |f| f.read } end\\r\\nend\\r\\n\\r\\nclass SendFileController < Actio
      :test_default_send_data_status,
      # NoMethodError: undefined method `bytesize' for "# encoding: utf-8\r\nrequire 'abstract_unit'\r\n\r\nmodule TestFileUtils\r\n  def file_name() File.basename(__FILE__) end\r\n  def file_path() File.expand_path(__FILE__) end\r\n  def file_data() @data ||= File.open(file_path, 'rb') { |f| f.read } end\r\nend\r\n\r\nclass SendFileController < ActionController::Base\r\n  include TestFileUtils\r\n  layout
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/actionpack-3.0.pre/lib/action_controller/metal/streaming.rb:145:in `send_data'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/controller/send_file_test.rb:24:in `data'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:789:in `send'
      :test_headers_after_send_shouldnt_include_charset,
      # NoMethodError: undefined method `bytesize' for "\320\232\320\270\321\200\320\270\320\273\320\270\321\206\320\260\n\347\245\235\346\202\250\345\245\275\351\201\213":String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/actionpack-3.0.pre/lib/action_controller/metal/streaming.rb:145:in `send_data'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack/controller/send_file_test.rb:28:in `multibyte_text_data'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:789:in `send'
      :test_send_data_content_length_header,
      # Exception raised:
      # Class: <NoMethodError>
      # Message: <"undefined method `bytesize' for \"# encoding: utf-8\\r\\nrequire 'abstract_unit'\\r\\n\\r\\nmodule TestFileUtils\\r\\n  def file_name() File.basename(__FILE__) end\\r\\n  def file_path() File.expand_path(__FILE__) end\\r\\n  def file_data() @data ||= File.open(file_path, 'rb') { |f| f.read } end\\r\\nend\\r\\n\\r\\nclass SendFileController < Actio
      :test_send_data_status


  end
end
