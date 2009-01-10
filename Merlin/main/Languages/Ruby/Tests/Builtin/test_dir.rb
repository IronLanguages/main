# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require "../Util/simple_test.rb"

$current = Dir.pwd

describe "Dir#new" do
    it "throws if the new is bad" do
        should_raise(Errno::ENOENT) { Dir.new("") } 
        should_raise(Errno::ENOENT) { Dir.new("testdir/notexisting") } 
        #should_raise(Errno::ENOTDIR) { Dir.new("testdir/file1.txt") } 
        should_raise(Errno::ENOENT) { Dir.new("test?dir") } 
    end 
    
    it "does not work for subdirectory" do 
        should_raise(Errno::ENOENT) { Dir.new("testdir/folder") } 
    end 
    
    it "does allow / at the end" do 
        Dir.new("testdir/").close
    end     
end

describe "Dir#getwd" do
    it "returns the current directory" do 
        Dir.getwd.should == $current
    end 
end 

describe "Dir#chdir" do 
    skip "changes the current directory to HOME or LOGDIR if set" do 
      if ENV['HOME'] || ENV['LOGDIR']
        Dir.chdir
      else
        should_raise(ArgumentError) { Dir.chdir }
      end
    end 
    
    it "changes to the root directory" do 
        Dir.chdir("C:/").should == 0
        Dir.getwd.should == "C:/"
        
        Dir.chdir($current).should == 0
    end 
    
    it "throws if the directory does not exist, or invalid arg" do 
        saved = Dir.getwd
        #should_raise(Errno::ENOENT) { Dir.chdir("C:/notexisting/directory") }
        should_raise(Errno::EINVAL) { Dir.chdir("") }
        should_raise(Errno::EINVAL) { Dir.chdir("test?dir") }                
        Dir.getwd.should == saved
    end 
    
    it "" do 
        Dir.chdir("testdir/")   
        Dir.chdir($current)     
    end 
    
    skip "takes block" do 
        saved = Dir.getwd
        # TODO: entering try with non-empty stack bug
        #Dir.chdir("testdir") { |x| nil }.should == nil
        #Dir.chdir("testdir") { |x| x }.should == "testdir"
        #Dir.chdir(saved)
    end   
    
    Dir.chdir($current)
end 

describe "Dir#foreach" do 
    it "throws if dirname is wrong" do 
        should_raise(Errno::ENOENT) { Dir.foreach("C:/notexisting/directory") { |x| x }  }
        should_raise(Errno::ENOENT) { Dir.foreach("") { |x| x }  }
        should_raise(Errno::ENOENT) { Dir.foreach("test?dir") { |x| x }  }
    end 
    
    # TODO: fix this test so that it accounts for the hidden .svn directory in
    # the SVN layout tree
    skip "calls the block once for each entry" do
        l = []
        Dir.foreach("testdir") do |x|
            l << x
        end 
        l.should == ['.', '..', 'file1.txt', 'file2.txt', 'folder1']
    end 
end 

describe "Dir#glob" do 
    Dir.chdir("testdir")
    
    it "returns set of files with different patterns" do 
        # TODO: similar SVN layout bug
        #Dir.glob("*").sort.should == ['file1.txt', 'file2.txt', 'folder1']
        Dir.glob("*.txt").sort.should == ['file1.txt', 'file2.txt']
        Dir.glob("*.xyz").should == []
    end 
    
    skip "takes blocks too" do 
      # TODO: entering try with non-empty stack bug
      #  l = []
      #  Dir.glob("*1.*") { |x| l << x } .should == nil
      #  l.should == ['file1.txt']
    end 
    
    Dir.chdir($current)
end 

describe "Dir#each" do
    skip "calls the block once for each entry" do 
        # TODO: entering try with non-empty stack bug
        #x = Dir.new("testdir")
        
        #l = []
        #x.each do |y| 
        #    l << y
        #end.should == x
        
        #l.should == ['.', '..', 'file1.txt', 'file2.txt', 'folder1']
        
        #x.close
    end
end 

describe "Dir#path" do 
    it "returns the path parameter" do 
        x = Dir.open("testdir")
        x.path.should == "testdir"
        x.close
    end 
end 

describe "Dir#close" do 
    it "set some flag; after that, all operations will throw" do
        x = Dir.new("testdir")
        x.close.should == nil
        
        should_raise(IOError, "closed directory") { x.close }
        should_raise(IOError) { x.path }
        should_raise(IOError) { x.each {|x| x } }
        should_raise(IOError) { x.pos }
        should_raise(IOError) { x.pos = 1 }
        should_raise(IOError) { x.read }
        should_raise(IOError) { x.seek(1) }
        should_raise(IOError) { x.tell }
    end 
end 

describe "Dir#pos related" do 
    # TODO: fix test bug related to .svn hidden directory
    skip "does the following sequence" do 
        x = Dir.new("testdir")
        x.pos.should == 0
        x.tell.should == 0
        
        x.read.should  == "."
        x.pos.should == 1
        x.read.should  == ".."
        x.tell.should == 2
        x.tell.should == 2
        
        x.read.should == "file1.txt"
        x.pos = x.tell + 1
        x.read.should == "folder1"
        
        # at the end of stream
        x.read.should == nil
        x.read.should == nil
        x.pos.should == 5
        
        # set pos
        x.pos = 3
        x.read.should == "file2.txt"        
        
        x.rewind.should == x
        x.tell.should == 0
        x.seek(4).should == x
        x.read.should == "folder1"
        
        # negative or large number to seek 
        x.seek(-3).pos.should == 0
        x.seek(-2).pos.should == 0
        x.seek(5).pos.should == 5
        x.seek(6).pos.should == 5
        x.seek(106).pos.should == 5
    end 
end 

finished
