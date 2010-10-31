require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/windows'
require 'etc'

platform_is :windows do
  describe "Etc.setgrent" do
    it_behaves_like(:etc_on_windows, :setgrent)
  end
end

platform_is_not :windows do
  describe "Etc.setgrent" do

    before(:all) do
      @etc_group = `cat /etc/group`.chomp.split("\n").
        map { |s| s.split(':') }.
        map { |e| Struct::Group.new(e[0],e[1],e[2].to_i,e[3].to_a) }
    end

    it "should reset the file pointer on etc/group" do
      expected = @etc_group.first
      Etc.getgrent
      Etc.setgrent
      gr = Etc.getgrent
      gr.should == expected
    end

  end
end
