# -*- ruby -*-

$: << 'lib'

require 'rubygems'
require 'hoe'

Hoe.add_include_dirs("../../minitest/dev/lib")

Hoe.plugin :seattlerb

Hoe.spec "ZenTest" do
  developer 'Ryan Davis', 'ryand-ruby@zenspider.com'
  developer 'Eric Hodel', 'drbrain@segment7.net'
end

task :autotest do
  ruby "-Ilib -w ./bin/autotest"
end

task :update do
  system "p4 edit example_dot_autotest.rb"
  File.open "example_dot_autotest.rb", "w" do |f|
    f.puts "# -*- ruby -*-"
    f.puts
    Dir.chdir "lib" do
      Dir["autotest/*.rb"].sort.each do |s|
        next if s =~ /rails|discover/
        f.puts "# require '#{s[0..-4]}'"
      end
    end

    f.puts

    Dir["lib/autotest/*.rb"].sort.each do |file|
      file = File.read(file)
      m = file[/module.*/].split(/ /).last rescue nil
      next unless m

      file.grep(/def[^(]+=/).each do |setter|
        setter = setter.sub(/^ *def self\./, '').sub(/\s*=\s*/, ' = ')
        f.puts "# #{m}.#{setter}"
      end
    end
  end
  system "p4 diff -du example_dot_autotest.rb"
end

task :sort do
  begin
    sh 'for f in lib/*.rb; do echo $f; grep "^ *def " $f | grep -v sort=skip > x; sort x > y; echo $f; echo; diff x y; done'
    sh 'for f in test/test_*.rb; do echo $f; grep "^ *def.test_" $f > x; sort x > y; echo $f; echo; diff x y; done'
  ensure
    sh 'rm x y'
  end
end

# vim:syntax=ruby

