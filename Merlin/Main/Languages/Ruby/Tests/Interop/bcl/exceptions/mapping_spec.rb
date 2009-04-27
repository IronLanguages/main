require File.dirname(__FILE__) + '/../../spec_helper'

describe "Mapping Ruby exceptions to CLR exceptions" do
  it "Errno::EACCES to System::UnauthorizedAccessException" do
    Errno::EACCES.should == System::UnauthorizedAccessException
  end
  
  it "Errno::ENOENT to System::IO::FileNotFoundException" do
    Errno::ENOENT.should == System::IO::FileNotFoundException
  end
  
  it "Errno::ENOTDIR to System::IO::DirectoryNotFoundException" do
    Errno::ENOTDIR.should == System::IO::DirectoryNotFoundException
  end
end