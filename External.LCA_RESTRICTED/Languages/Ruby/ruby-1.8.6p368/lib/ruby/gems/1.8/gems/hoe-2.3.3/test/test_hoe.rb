require 'minitest/autorun'
require 'hoe'
require 'tempfile'

$rakefile = nil # shuts up a warning in rdoctask.rb

class TestHoe < MiniTest::Unit::TestCase
  def setup
    Rake.application.clear
  end

  def test_file_read_utf
    Tempfile.open 'BOM' do |io|
      io.write "\xEF\xBB\xBFBOM"
      io.rewind
      assert_equal 'BOM', File.read_utf(io.path)
    end
  end

  def test_possibly_better
    t = Gem::Specification::TODAY
    hoe = Hoe.spec("blah") do
      self.version = '1.2.3'
      developer 'author', 'email'
    end

    files = File.read("Manifest.txt").split(/\n/)

    spec = hoe.spec

    text_files = files.grep(/txt$/).reject { |f| f =~ /template/ }

    assert_equal 'blah', spec.name
    assert_equal '1.2.3', spec.version.to_s
    assert_equal '>= 0', spec.required_rubygems_version.to_s

    assert_equal ['author'], spec.authors
    assert_equal t, spec.date
    assert_equal 'sow', spec.default_executable
    assert_match(/Hoe.*Rakefiles/, spec.description)
    assert_equal ['email'], spec.email
    assert_equal ['sow'], spec.executables
    assert_equal text_files, spec.extra_rdoc_files
    assert_equal files, spec.files
    assert_equal true, spec.has_rdoc
    assert_equal "http://rubyforge.org/projects/seattlerb/", spec.homepage
    assert_equal ['--main', 'README.txt'], spec.rdoc_options
    assert_equal ['lib'], spec.require_paths
    assert_equal 'blah', spec.rubyforge_project
    assert_equal Gem::RubyGemsVersion, spec.rubygems_version
    assert_match(/^Hoe.*Rakefiles$/, spec.summary)
    assert_equal files.grep(/^test/), spec.test_files

    deps = spec.dependencies

    assert_equal 1, deps.size

    dep = deps.first

    assert_equal 'hoe', dep.name
    assert_equal :development, dep.type
    assert_equal ">= #{Hoe::VERSION}", dep.version_requirements.to_s
  end

  def test_plugins
    before = Hoe.plugins.dup
    Hoe.plugin :first, :second
    assert_equal before + [:first, :second], Hoe.plugins
    Hoe.plugin :first, :second
    assert_equal before + [:first, :second], Hoe.plugins
  ensure
    # FIX: maybe add Hoe.reset
    Hoe.plugins.replace before
  end

  def test_rename
    # project, file_name, klass = Hoe.normalize_names 'project_name'

    assert_equal %w(    word      word     Word),  Hoe.normalize_names('word')
    assert_equal %w(    word      word     Word),  Hoe.normalize_names('Word')
    assert_equal %w(two_words two_words TwoWords), Hoe.normalize_names('TwoWords')
    assert_equal %w(two_words two_words TwoWords), Hoe.normalize_names('twoWords')
    assert_equal %w(two_words two_words TwoWords), Hoe.normalize_names('two-words')
    assert_equal %w(two_words two_words TwoWords), Hoe.normalize_names('two_words')
  end
end
