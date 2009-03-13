require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

# global first, then local to length validators
require __dir__.parent.parent + "spec_helper"
require __dir__ + 'spec_helper'

describe DataMapper::Validate::LengthValidator do
  it "lets user specify a minimum length of a string field" do
    class ::MotorLaunch
      validates_length :name, :min => 3
    end

    launch = MotorLaunch.new
    launch.name = 'Ab'
    launch.should_not be_valid
    launch.errors.on(:name).should include('Name must be more than 3 characters long')
  end

  it "aliases :minimum for :min" do
    class ::MotorLaunch
      validators.clear!
      validates_length :name, :minimum => 3
    end

    launch = MotorLaunch.new
    launch.name = 'Ab'
    launch.should_not be_valid
    launch.errors.on(:name).should include('Name must be more than 3 characters long')
  end
end
