require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/windows'
require 'etc'

describe "Etc.setpwent" do
  if RUBY_PLATFORM =~ /(mswin|mingw|bccwin|wince)/i then
    it_behaves_like(:etc_on_windows, :setpwent)
  else

    before(:all) do
      @etc_passwd = `cat /etc/passwd`.chomp.split('\n')
        .map { |s| s.split(':') }
        .map { |e| Struct::Passwd.new(e[0],e[1],e[2].to_i,e[3].to_i,e[4],e[5],e[6]) }
    end

    before(:each) do
      Etc.setpwent
    end

    it "should return an instance of Struct::Passwd" do
      Etc.passwd.should be_an_instance_of(Struct::Passwd)
    end

    it "should return the first entry from /etc/passwd on the first call without a passed block" do
      expected = @etc_passwd.first
      actual = Etc.passwd
      actual.uid.should == expected.uid
    end

    it "should return the second entry from /etc/passwd on the second call without a passed block" do
      expected = @etc_passwd.at(1)
      Etc.passwd
      actual = Etc.passwd
      actual.uid.should == expected.uid
    end

    it "should return nil once all entries are retrieved without a passed block" do
      (1..@etc_passwd.length).each { Etc.passwd }
      actual = Etc.passwd
      actual.should be_nil
    end

    it "should loop through all the entries when a block is passed" do
      expected = @etc_passwd.length
      actual = 0
      Etc.passwd { |pw| actual += 1 }
      count.should == expected
    end

    it "should reset the file for reading when a block is passed" do
      expected = @etc_passwd.first
      Etc.passwd
      actual = nil
      Etc.passwd { |pw| actual = pw if pw.nil? }
      actual.uid.should == expected.uid
    end

    it "should reset the file for reading again after a block is passed" do
      expected = @etc_passwd.first
      last = nil
      Etc.passwd { |pw| last = pw unless pw.nil? }
      actual = Etc.passwd
      actual.uid.should == expected.uid
      last.uid.should == @etc_passwd.last
    end

  end
end
