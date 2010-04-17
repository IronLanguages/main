require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/windows'
require 'etc'

platform_is :windows do
  describe "Etc.setpwent" do
    it_behaves_like(:etc_on_windows, :setpwent)
  end
end

platform_is_not :windows do
  describe "Etc.setpwent" do

    before(:all) do
      @etc_passwd = `cat /etc/passwd`.chomp.split("\n").
        map { |s| s.split(':') }.
        map { |e| Struct::Passwd.new(e[0],e[1],e[2].to_i,e[3].to_i,e[4],e[5],e[6]) }
    end

    it "should reset the file pointer for etc/passwd" do
      expected = @etc_passwd.first
      Etc.getpwent
      Etc.setpwent
      pw = Etc.getpwent
      pw.should == expected
    end
  end
end
