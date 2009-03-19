require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

module DataMapper
  module Validate
    describe DataMapper::Validate::GenericValidator do
      describe "#==" do
        it "should return true if types and fields are equal" do
          RequiredFieldValidator.new(:name).should == RequiredFieldValidator.new(:name)
        end
        it "should return false of types differ" do
          RequiredFieldValidator.new(:name).should_not == UniquenessValidator.new(:name)
        end
      end
    end
  end
end
