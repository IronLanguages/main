require "fileutils"
require File.dirname(__FILE__) + '/../../spec_helper'

describe "File.rename" do
  before :each do
    @old = "test.txt"
    @new = "test.new"
    @dir = "test.dir"
    @new_dir = "test.new_dir"
    
    File.delete(@old) if File.exist?(@old)
    File.delete(@new) if File.exist?(@new)
    FileUtils.remove_dir(@dir) if File.exist?(@dir)
    FileUtils.remove_dir(@new_dir) if File.exist?(@new_dir)

    File.open(@old,"w+") {|f| f.puts "hello" }
    FileUtils.mkdir(@dir)
  end

  after :each do
    File.delete(@old) if File.exist?(@old)
    File.delete(@new) if File.exist?(@new)
    FileUtils.remove_dir(@dir) if File.exist?(@dir)
    FileUtils.remove_dir(@new_dir) if File.exist?(@new_dir)
  end

  it "renames a file " do
    File.rename(@old, @new)
    File.exists?(@old).should == false
    File.exists?(@new).should == true
  end

  it "uses Dir.pwd for relative paths" do
    File.rename(@old, @new)
    File.exist?(File.join(Dir.pwd, @old)).should be_false
    File.exist?(File.join(Dir.pwd, @new)).should be_true
  end
  
  it "raises an Errno::ENOENT if the source does not exist" do
    File.delete(@old)
    lambda { File.rename(@old, @new) }.should raise_error(Errno::ENOENT)
    lambda { File.rename("non-existent", "non-existent") }.should raise_error(Errno::ENOENT)
  end

  it "overwrites destination if the destination already exists" do
    File.open(@new, "w+") {|f| f.puts "new contents" }
    File.rename(@old, @new)
    File.exists?(@old).should == false
    File.open(@new, "r") {|f| f.read }.should == "hello\n"
  end

  it "overwrites destination if the destination already exists and is read-only" do
    File.open(@new, "w+") {|f| f.puts "new contents" }
    File.chmod(0444, @new)
    File.rename(@old, @new)
    File.exists?(@old).should == false
    File.open(@new, "r") {|f| f.read }.should == "hello\n"
  end

  it "raises an Errno::EACCES if source is a file and if destination is an existing folder" do
    lambda { File.rename(@old, @dir) }.should raise_error(Errno::EACCES)
    File.open(@old, "r") {|f| f.read }.should == "hello\n"
  end

  it "raises an Errno::EACCES if source is a folder and destination is an existing folder" do
    FileUtils.mkdir(@new_dir)
    lambda { File.rename(@dir, @new_dir) }.should raise_error(Errno::EACCES)
  end

  it "does rename with a source path" do
    File.rename("#{Dir.pwd}/#{@old}", @new)
    File.exist?(@old).should be_false
    File.exist?(@new).should be_true
  end
  
  it "does rename with a destination path" do
    File.rename(@old, "#{@dir}/#{@new}")
    File.exist?(@old).should be_false
    File.exist?("#{@dir}/#{@new}").should be_true
  end
  
  it "renames a folder" do
    File.rename(@dir, @new_dir)
    File.exist?(@dir).should be_false
    File.exist?(@new_dir).should be_true
    File.directory?(@new_dir).should be_true
  end
  
  it "allows rename to the same name" do
    File.rename(@old, @old)
    File.exists?(@old).should == true

    File.rename("./#{@old}", @old)
    File.exists?(@old).should == true
  end
  
  it "raises an ArgumentError if not passed two arguments" do
    lambda { File.rename        }.should raise_error(ArgumentError)
    lambda { File.rename(@file) }.should raise_error(ArgumentError)
  end

  it "raises a TypeError if not passed String types" do
    lambda { File.rename(1, 2)  }.should raise_error(TypeError)
  end
end
