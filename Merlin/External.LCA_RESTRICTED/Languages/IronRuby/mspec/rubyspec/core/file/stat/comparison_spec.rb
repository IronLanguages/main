require File.dirname(__FILE__) + '/../../../spec_helper'

describe "File::Stat#<=>" do
  before :each do
    @name1 = tmp("i_exist")
    @name2 = tmp("i_exist_too")
    File.open(@name1, "w") {}
    File.open(@name2, "w") {}
    @stat1 = File.stat(@name1)
    @stat2 = File.stat(@name2)
  end

  after :each do
    File.delete @name1
    File.delete @name2
  end

  it "is able to compare files by the same modification times" do
    res = false
    10.times do
      File.open(@name1, "w") {}
      File.open(@name2, "w") {}
      res ||= ((File.stat(@name1) <=> File.stat(@name2)) == 0)
      break if res
    end
    res.should be_true
  end

  it "is able to compare files by different modification times" do
    File.utime(Time.now, Time.now + 100, @name2)
    (File.stat(@name1) <=> File.stat(@name2)).should == -1

    File.utime(Time.now, Time.now - 100, @name2)
    (File.stat(@name1) <=> File.stat(@name2)).should == 1
  end

  it "should also include Comparable and thus == shows mtime equality between two File::Stat objects" do
    (@stat1 == @stat2).should == true
    (@stat1 == @stat1).should == true
    (@stat2 == @stat2).should == true

    File.utime(Time.now, Time.now + 100, @name2)

    (File.stat(@name1) == File.stat(@name2)).should == false
    (File.stat(@name1) == File.stat(@name1)).should == true
    (File.stat(@name2) == File.stat(@name2)).should == true
  end
end
