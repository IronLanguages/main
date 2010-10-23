# -*- encoding: utf-8 -*-
require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "IO#readline" do

  it "returns the next line on the stream" do
    testfile = File.dirname(__FILE__) + '/fixtures/gets.txt'
    f = File.open(testfile, 'r') do |f|
      f.readline.should == "Voici la ligne une.\n"
      f.readline.should == "Qui è la linea due.\n"
    end
  end

  it "goes back to first position after a rewind" do
    testfile = File.dirname(__FILE__) + '/fixtures/gets.txt'
    f = File.open(testfile, 'r') do |f|
      f.readline.should == "Voici la ligne une.\n"
      f.rewind
      f.readline.should == "Voici la ligne une.\n"
    end
  end

  it "is modified by the cursor position" do
    testfile = File.dirname(__FILE__) + '/fixtures/gets.txt'
    f = File.open(testfile, 'r') do |f|
      f.seek(1)
      f.readline.should == "oici la ligne une.\n"
    end
  end

  it "raises EOFError on end of stream" do
    testfile = File.dirname(__FILE__) + '/fixtures/gets.txt'
    File.open(testfile, 'r') do |f|
      lambda { while true; f.readline; end }.should raise_error(EOFError)
    end

  end

  it "raises IOError on closed stream" do
    lambda { IOSpecs.closed_file.readline }.should raise_error(IOError)
  end

  it "assigns the returned line to $_" do
    File.open(IOSpecs.gets_fixtures, 'r') do |f|
      IOSpecs.lines.each do |line|
        f.readline
        $_.should == line
      end
    end
  end

  it "accepts a separator" do
    path = tmp("readline_specs")
    begin
      File.open(path, "w") do |f| 
        f.print("A1\nA2\n\nB\nC;D\n")
      end

      File.open(path, "r") do |f|
        f.readline("\n\n").should == "A1\nA2\n\n"
        f.readline.should == "B\n"
        f.readline(";").should == "C;"
        f.readline.should == "D\n"
        lambda { f.readline }.should raise_error(EOFError)
      end

    ensure
      File.unlink(path)
    end
  end
  
end
