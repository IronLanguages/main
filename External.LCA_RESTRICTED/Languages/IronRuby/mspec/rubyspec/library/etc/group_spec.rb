require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/windows'
require 'etc'

platform_is :windows do
  describe "Etc.group" do
    it_behaves_like(:etc_on_windows, :group)
  end
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
      gr = Etc.group
      gr.is_a?(Struct::Group).should == true
    end

    it "should return the first entry from /etc/group on the first call without a passed block" do
      expected = @etc_group.first
      gr = Etc.group
      gr.gid.should == expected.gid
    end

    it "should return the second entry from /etc/group on the second call without a passed block" do
      expected = @etc_group.at(1)
      Etc.group
      gr = Etc.group
      gr.gid.should == expected.gid
    end

    it "should return nil once all entries are retrieved without a passed block" do
      (1..@etc_group.length).each { Etc.group }
      gr = Etc.group
      gr.should be_nil
    end

    it "should loop through all the entries when a block is passed" do
      expected = @etc_group.length
      actual = 0
      Etc.group { |gr| actual += 1 }
      actual.should == expected
    end

    it "should reset the file for reading when a block is passed" do
      expected = @etc_group.first
      Etc.group
      actual = nil
      Etc.group { |gr| actual = gr if actual.nil? }
      actual.gid.should == expected.gid
    end

    it "should reset the file for reading again after a block is passed" do
      expected = @etc_group.at(1)
      last = nil
      Etc.group { |gr| last = gr unless gr.nil? }
      actual = Etc.group
      actual.gid.should == expected.gid
    end

  end
end
