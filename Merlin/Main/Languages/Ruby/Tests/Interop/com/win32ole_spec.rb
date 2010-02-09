require File.dirname(__FILE__) + '/../spec_helper'
require "win32ole"

# Basic tests. MRI includes deeper tests

describe "win32ole" do
  before :each do
    @fs = WIN32OLE.new "Scripting.FileSystemObject"
  end
  
  it "supports enumeration" do    
    tmp = tmp("")
    Dir.mkdir(tmp + "/a_folder")
    begin
      tempdir = @fs.GetFolder tmp
      subfolders = []
      tempdir.SubFolders.each {|sub| subfolders << sub.name.to_str}
      subfolders.include?("a_folder").should be_true
    ensure
      Dir.rmdir(tmp + "/a_folder")
    end
  end
  
  it "supports const_load" do
    m = Module.new
    WIN32OLE.const_load(@fs, m)
    m.const_get("SystemFolder").should == 1
    m.const_get("CONSTANTS")["SystemFolder"].should == 1 #fails on MRI
  end

  it "raises WIN32OLERuntimeError for unknown servers" do
    lambda {WIN32OLE.new("IDONTEXIST.IAMNOTREAL")}.should raise_error(WIN32OLERuntimeError, /IDONTEXIST\.IAMNOTREAL/)
  end
end
