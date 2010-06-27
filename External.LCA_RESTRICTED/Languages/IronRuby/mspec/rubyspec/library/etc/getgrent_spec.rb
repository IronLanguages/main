require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/windows'
require 'etc'

describe "Etc.getgrent" do
  it_behaves_like(:etc_on_windows, :getgrent)
end

platform_is_not :windows do
  describe "Etc.group" do

    before(:all) do
      @etc_group = `cat /etc/group`.chomp.split("\n").
        map { |s| s.split(':') }.
        map { |e| Struct::Group.new(e[0],e[1],e[2].to_i,e[3].to_a) }
    end

    before(:each) do
      Etc.setgrent
    end

    it "should return an instance of Struct::Group" do
      Etc.getgrent.should be_an_instance_of(Struct::Group)
    end

    it "should return the first entry from /etc/group on the first call" do
      expected = @etc_group.first
      actual = Etc.getgrent
      actual.should == expected
    end

    it "should return the second entry from /etc/group on the second call" do
      expected = @etc_group.at(1)
      Etc.getgrent
      actual = Etc.getgrent
      actual.should == expected
    end

    it "should return nil once all entries are retrieved" do
      (1..@etc_group.length).each { Etc.getgrent }
      actual = Etc.getgrent
      actual.should be_nil
    end

  end
end
