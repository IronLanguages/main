require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

# global first, then local to length validators
require __dir__.parent.parent + "spec_helper"
require __dir__ + 'spec_helper'

describe DataMapper::Validate::LengthValidator do
  it "lets user specify custom error message" do
    class Jabberwock
      include DataMapper::Resource
      property :id, Integer, :key => true
      property :snickersnack, String
      validates_length :snickersnack, :within => 3..40, :message => "worble warble"
    end
    wock = Jabberwock.new
    wock.should_not be_valid
    wock.errors.on(:snickersnack).should include('worble warble')
    wock.snickersnack = "hello"
    wock.id = 1
    wock.should be_valid
  end
end
