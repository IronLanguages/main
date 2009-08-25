require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

# global first, then local to length validators
require __dir__.parent.parent + "spec_helper"
require __dir__ + 'spec_helper'

describe DataMapper::Validate::LengthValidator do
  it "lets user specify a maximum length of a string field" do
    class ::MotorLaunch
      validators.clear!
      validates_length :name, :max => 5
    end

    launch = MotorLaunch.new
    launch.name = 'a' * 6
    launch.should_not be_valid
    launch.errors.on(:name).should include('Name must be less than 5 characters long')
  end

  it "aliases :maximum for :max" do
    class ::MotorLaunch
      validators.clear!
      validates_length :name, :maximum => 5
    end
    launch = MotorLaunch.new
    launch.name = 'a' * 6
    launch.should_not be_valid
    launch.errors.on(:name).should include('Name must be less than 5 characters long')
  end
end
