require File.dirname(__FILE__) + '/../../../spec_helper'

require 'shell'

describe "Shell::CommandProcessor#echo" do
  before :each do
    sh = Shell.new
    @cmd = sh.instance_variable_get("@command_processor")
  end
  
  it "returns a Shell::Echo" do
    @cmd.echo("Hello").should be_kind_of(Shell::Echo)
  end
end
