require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

# global first, then local to length validators
require __dir__.parent.parent + "spec_helper"
require __dir__ + 'spec_helper'

describe DataMapper::Validate::LengthValidator do
  it "passes if a default fulfills the requirements" do
    doc = BoatDock.new
    doc.should be_valid
  end
end
