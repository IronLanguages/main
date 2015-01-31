#! /usr/bin/env rake
$LOAD_PATH.unshift('lib')

require 'rubygems'
require 'rake/gempackagetask'
require 'text/format'
require 'archive/tar/minitar'
require 'zlib'

DISTDIR = "text-format-#{Text::Format::VERSION}"
TARDIST = "../#{DISTDIR}.tar.gz"

DATE_RE = %r<(\d{4})[./-]?(\d{2})[./-]?(\d{2})(?:[\sT]?(\d{2})[:.]?(\d{2})[:.]?(\d{2})?)?>

if ENV['RELEASE_DATE']
  year, month, day, hour, minute, second = DATE_RE.match(ENV['RELEASE_DATE']).captures
  year ||= 0
  month ||= 0
  day ||= 0
  hour ||= 0
  minute ||= 0
  second ||= 0
  ReleaseDate = Time.mktime(year, month, day, hour, minute, second)
else
  ReleaseDate = nil
end

task :test do |t|
  require 'test/unit/testsuite'
  require 'test/unit/ui/console/testrunner'

  runner = Test::Unit::UI::Console::TestRunner

  $LOAD_PATH.unshift('tests')
  $stderr.puts "Checking for test cases:" if t.verbose
  Dir['tests/tc_*.rb'].each do |testcase|
    $stderr.puts "\t#{testcase}" if t.verbose
    load testcase
  end

  suite = Test::Unit::TestSuite.new("Text::Format")

  ObjectSpace.each_object(Class) do |testcase|
    suite << testcase.suite if testcase < Test::Unit::TestCase
  end

  runner.run(suite)
end

spec = eval(File.read("text-format.gemspec"))
spec.version = Text::Format::VERSION
desc "Build the RubyGem for Text::Format"
task :gem => [ :test ]
Rake::GemPackageTask.new(spec) do |g|
  g.need_tar    = false
  g.need_zip    = false
  g.package_dir = ".."
end

desc "Build a Text::Format .tar.gz distribution."
task :tar => [ TARDIST ]
file TARDIST => [ :test ] do |t|
  current = File.basename(Dir.pwd)
  Dir.chdir("..") do
    begin
      files = Dir["#{current}/**/*"].select { |dd| dd !~ %r{(?:/CVS/?|~$)} }
      files.map! do |dd|
        ddnew = dd.gsub(/^#{current}/, DISTDIR)
        mtime = ReleaseDate || File.stat(dd).mtime
        if File.directory?(dd)
          { :name => ddnew, :mode => 0755, :dir => true, :mtime => mtime }
        else
          if dd =~ %r{bin/}
            mode = 0755
          else
            mode = 0644
          end
          data = File.read(dd)
          { :name => ddnew, :mode => mode, :data => data, :size => data.size,
            :mtime => mtime }
        end
      end

      ff = File.open(t.name.gsub(%r{^\.\./}o, ''), "wb")
      gz = Zlib::GzipWriter.new(ff)
      tw = Archive::Tar::Minitar::Writer.new(gz)

      files.each do |entry|
        if entry[:dir]
          tw.mkdir(entry[:name], entry)
        else
          tw.add_file_simple(entry[:name], entry) { |os| os.write(entry[:data]) }
        end
      end
    ensure
      tw.close if tw
      gz.close if gz
    end
  end
end
task TARDIST => [ :test ]

desc "Build the RDoc documentation for Text::Format"
task :docs do
  require 'rdoc/rdoc'
  rdoc_options = %w(--title Text::Format --main README --line-numbers)
  files = FileList[*%w(README ChangeLog Install bin/**/*.rb lib/**/*.rb)]
  rdoc_options += files.to_a
  RDoc::RDoc.new.document(rdoc_options)
end

desc "Build everything."
task :default => [ :tar, :gem ]
