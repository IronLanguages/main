require File.dirname(__FILE__) + '/../../spec_helper'

describe "File.split" do
  before :each do
    @path_unix             = "/foo/bar/baz.rb"
    @path_windows_backward = "C:\\foo\\bar\\baz.rb"
    @path_windows_forward  = "C:/foo/bar/baz.rb"
  end

  it "splits the given string into a directory and a file component and returns them in a 2 element array" do
    File.split("/rubinius/better/than/ruby").should == ["/rubinius/better/than", "ruby"]
  end

  it "splits the given string into a directory and a file component and returns them in a two-element array. (unix)" do
    File.split(@path_unix).should == ["/foo/bar","baz.rb"]
  end

  it "splits the given string into a directory and a file component and returns them in a two-element array. (edge cases)" do
    File.split("").should == [".", ""]
  end

  platform_is_not :windows do
    it "deals with multiple forward slashes" do
      File.split("//foo////").should == ["/", "foo"]
    end
  end

  platform_is :windows do
    it "deals with multiple forward slashes" do
      File.split("//foo////").should == ["//foo", "/"]
    end

    not_compliant_on :rubinius do
      it "splits the given string into a directory and a file component and returns them in a two-element array. (windows)" do
        File.split(@path_windows_backward).should ==  ["C:\\foo\\bar", "baz.rb"]
      end
    end

    deviates_on :rubinius do
      # Note: MRI on Cygwin exhibits third type of behavior,
      # different from *both* variants above...
      it "splits the given string into a directory and a file component and returns them in a two-element array. (windows)" do
        File.split(@path_windows_backward).should ==  [".", "C:\\foo\\bar\\baz.rb"]
      end
    end

    it "deals with Windows edge cases" do
      File.split("c:foo").should == ["c:.", "foo"]
      File.split("c:.").should == ["c:.", "."]
      File.split("c:/foo").should == ["c:/", "foo"]
      File.split("c:/.").should == ["c:/", "."]
    end
  end

  it "splits the given string into a directory and a file component and returns them in a two-element array. (forward slash)" do
    File.split(@path_windows_forward).should == ["C:/foo/bar", "baz.rb"]
  end

  it "raises an ArgumentError when not passed a single argument" do
    lambda { File.split }.should raise_error(ArgumentError)
    lambda { File.split('string', 'another string') }.should raise_error(ArgumentError)
  end

  it "raises a TypeError if the argument is not a String type" do
    lambda { File.split(1) }.should raise_error(TypeError)
  end

  it "coerces the argument with to_str if it is not a String type" do
    class C; def to_str; "/rubinius/better/than/ruby"; end; end
    File.split(C.new).should == ["/rubinius/better/than", "ruby"]
  end
end
