require File.dirname(__FILE__) + '/../../spec_helper'
require 'open3'

describe "Open3#popen3" do
  it "gives access to stdout" do
    _, out, _ = Open3.popen3("dir")
    out.read().should =~ /bytes/
  end

  it "gives access to stderr" do
    _, _, err = Open3.popen3("dir non-existent")
    err.read().should =~ /File Not Found/i
  end
  
  it "returns an array" do
    res = Open3.popen3("dir")
    res.should.be_kind_of Array
  end

  it "returns IO objects" do
    res = Open3.popen3("dir")
    res[0..2].each { |r| r.should.be_kind_of IO }
  end

  ruby_version_is "1.9" do
    it "returns a thread" do
      _, _, _, t = Open3.popen3("dir")
      t.should.be_kind_of Thread
    end
  end
end
