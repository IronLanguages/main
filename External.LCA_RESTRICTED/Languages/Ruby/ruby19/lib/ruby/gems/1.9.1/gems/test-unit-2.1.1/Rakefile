# -*- ruby -*-

Encoding.default_internal = "UTF-8" if defined?(Encoding.default_internal)

require 'rubygems'
gem 'rdoc'
require 'hoe'
require './lib/test/unit/version.rb'

ENV["NODOT"] = "yes"

version = Test::Unit::VERSION
ENV["VERSION"] = version
project = Hoe.spec('test-unit') do
  Hoe::Test::SUPPORTED_TEST_FRAMEWORKS[:testunit2] = "test/run-test.rb"
  self.version = version
  developer('Kouhei Sutou', 'kou@cozmixng.org')
  developer('Ryan Davis', 'ryand-ruby@zenspider.com')

  # Ex-Parrot:
  # developer('Nathaniel Talbott', 'nathaniel@talbott.ws')
end

task :check_manifest => :clean_test_result
task :check_manifest => :clean_coverage

task :clean_test_result do
  test_results = Dir.glob("**/.test-result")
  sh("rm", "-rf", *test_results) unless test_results.empty?
end

task :clean_coverage do
  sh("rm", "-rf", "coverage")
end

desc "Publish HTML to Web site."
task :publish_html do
  config = YAML.load(File.read(File.expand_path("~/.rubyforge/user-config.yml")))
  host = "#{config["username"]}@rubyforge.org"

  rsync_args = "-av --exclude '*.erb' --exclude '*.svg' --exclude .svn"
  remote_dir = "/var/www/gforge-projects/#{project.rubyforge_name}/"
  sh "rsync #{rsync_args} html/ #{host}:#{remote_dir}"
end

desc "Tag the current revision."
task :tag do
  message = "Released Test::Unit #{version}!"
  base = "svn+ssh://#{ENV['USER']}@rubyforge.org/var/svn/test-unit/"
  sh 'svn', 'copy', '-m', message, "#{base}trunk", "#{base}tags/#{version}"
end

# vim: syntax=Ruby
