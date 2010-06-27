require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/windows'
require 'etc'

platform_is :windows do
  describe "Etc.getpwent" do
    it_behaves_like(:etc_on_windows, :getpwent)
  end
end

platform_is_not :windows do
  describe "Etc.getpwent" do

    before(:all) do
      @etc_passwd = `cat /etc/passwd`.chomp.split("\n").
        map { |s| s.split(':') }.
        map { |e| Struct::Passwd.new(e[0],e[1],e[2].to_i,e[3].to_i,e[4],e[5],e[6]) }
    end

    before(:each) do
      Etc.setpwent
    end

    it "should return an instance of Struct::Passwd" do
      Etc.getpwent.should be_an_instance_of(Struct::Passwd)
    end

    it "should return the first entry from /etc/passwd on the first call" do
      expected = @etc_passwd.first
      actual = Etc.getpwent
      actual.should == expected
    end

    it "should return the second entry from /etc/passwd on the second call" do
      expected = @etc_passwd.at(1)
      Etc.getpwent
      actual = Etc.getpwent
      actual.should == expected
    end

    it "should return nil once all entries are retrieved" do
      (1..@etc_passwd.length).each { Etc.getpwent }
      actual = Etc.getpwent
      actual.should be_nil
    end

  end
end
