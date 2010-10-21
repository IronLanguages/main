require 'rubygems'
require 'spec'
$: << File.dirname(__FILE__) + '/../lib'

require '$safeprojectname$'

describe 'Program' do
  it 'should work' do
    Program.new.hello_world.should == 42
  end
end