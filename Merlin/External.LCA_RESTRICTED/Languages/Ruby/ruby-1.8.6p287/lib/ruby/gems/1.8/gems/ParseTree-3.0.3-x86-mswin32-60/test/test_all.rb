#!/usr/local/bin/ruby -w

pat = "test_*.rb"
if File.basename(Dir.pwd) != "test" then
  $: << "test"
  pat = File.join("test", pat)
end

Dir.glob(pat).each do |f|
  require f
end

require 'test/unit'
