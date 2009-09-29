require File.dirname(__FILE__) + "/../spec_helper"

describe "Command line option: -profile" do
  it "enables profiling" do
    s = ruby_exe("puts IronRuby::Clr.profile{ require 'rubygems' }.size", :options => "-profile").chomp.to_i
    s.should > 80
  end
end
