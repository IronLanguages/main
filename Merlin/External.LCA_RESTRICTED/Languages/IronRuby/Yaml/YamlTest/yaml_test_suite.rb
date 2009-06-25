orig_dir = Dir.pwd
at_exit { Dir.chdir(orig_dir)}
Dir.chdir(File.dirname(__FILE__))
require 'test/unit'
require 'load_tests'
require 'load_file_tests'
require 'load_documents_tests'
require 'load_stream_tests'
require 'collections_tests'
require 'dump_tests'
require 'tags_tests'

class Load_tests < Test::Unit::TestCase
  def test_to_yaml
  end
end
require 'test_yaml'

class YAML_Unit_Tests < Test::Unit::TestCase
  def test_spec_builtin_time
  end

  def test_spec_private_types
  end

  def test_spec_url_escaping
  end

  def test_symbol_cycle
  end

  def test_time_now_cycle
  end

  def test_ypath_parsing
  end
end


