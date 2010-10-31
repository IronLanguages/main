=begin
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
=end

class UnitTestSetup
  def initialize
    @name = "ActionPack"
    super
  end
  
  def gather_files
    gather_rails_files
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= #{TestUnitVersion}"
    gem 'actionmailer', "= #{RailsVersion}"
    gem 'activerecord', "= #{RailsVersion}"
    gem 'activerecord-sqlserver-adapter', "= #{SqlServerAdapterVersion}"
    gem 'activesupport', "= #{RailsVersion}"
    gem 'actionpack', "= #{RailsVersion}"
    require 'action_pack/version'
    require 'active_record'
    require 'active_record/fixtures'
  end
  
  def sanity
    # Do some sanity checks
    sanity_size(80)
    sanity_version(RailsVersion, ActionPack::VERSION::STRING)
  end
  
=begin
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
      :test_caching_javascript_include_tag_when_caching_on,
      :test_caching_javascript_include_tag_when_caching_on_and_using_subdirectory,
      :test_caching_javascript_include_tag_when_caching_on_with_2_argument_object_asset_host,
      :test_caching_javascript_include_tag_when_caching_on_with_2_argument_proc_asset_host,
      :test_caching_javascript_include_tag_when_caching_on_with_proc_asset_host,
      :test_caching_javascript_include_tag_with_all_and_recursive_puts_defaults_at_the_start_of_the_file,
      :test_caching_javascript_include_tag_with_all_puts_defaults_at_the_start_of_the_file,
      :test_caching_javascript_include_tag_with_relative_url_root,
      :test_caching_stylesheet_link_tag_when_caching_on,
      :test_caching_stylesheet_link_tag_when_caching_on_with_proc_asset_host,
      :test_caching_stylesheet_link_tag_with_relative_url_root,
      :test_concat_stylesheet_link_tag_when_caching_off

    disable CachedViewRenderTest, 
      :test_render_file_with_full_path,
      :test_render_partial_with_errors,
      :test_render_sub_template_with_errors

    disable CompiledTemplatesTest, 
      :test_template_changes_are_not_reflected_with_cached_templates,
      :test_template_changes_are_reflected_with_uncached_templates

    disable FormTagHelperTest, 
      :test_boolean_options

    disable LayoutSetInResponseTest, 
      :test_absolute_pathed_layout

    disable LazyViewRenderTest, 
      :test_render_file_with_full_path,
      :test_render_partial_with_errors,
      :test_render_sub_template_with_errors

    disable RenderFile::TestBasic, 
      :test_rendering_a_Pathname,
      :test_rendering_file_with_locals,
      "test_rendering_path_without_specifying_the_:file_key",
      "test_rendering_path_without_specifying_the_:file_key_with_ivar",
      "test_rendering_path_without_specifying_the_:file_key_with_locals",
      :test_rendering_simple_template,
      :test_rendering_template_with_ivar

    disable RenderJSTest, 
      :test_render_with_default_from_accept_header

    disable RenderTest, 
      :test_line_offset,
      :test_render_file_as_string_with_instance_variables,
      :test_render_file_as_string_with_locals,
      :test_render_file_from_template,
      :test_render_file_using_pathname,
      :test_render_file_with_instance_variables,
      :test_render_file_with_locals

    disable SendFileTest, 
      :test_data,
      :test_default_send_data_status,
      :test_headers_after_send_shouldnt_include_charset,
      :test_send_data_content_length_header,
      :test_send_data_status


  end
=end
end
