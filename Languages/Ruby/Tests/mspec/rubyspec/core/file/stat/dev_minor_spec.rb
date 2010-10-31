require File.dirname(__FILE__) + '/../../../spec_helper'

describe "File::Stat#dev_minor" do
  platform_is_not :windows do
    it "returns the minor part of File::Stat#dev" do
      File.stat(FileStatSpecs.null_device).dev_minor.should be_kind_of(Integer)
    end
  end
  
  platform_is :windows do
    it "returns the number of the device on which the file exists" do
      File.stat(FileStatSpecs.null_device).dev_major.should be_nil
    end
  end
end
