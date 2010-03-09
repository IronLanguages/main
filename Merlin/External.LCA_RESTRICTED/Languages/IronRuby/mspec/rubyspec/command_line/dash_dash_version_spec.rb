require File.dirname(__FILE__) + '/../spec_helper'
require File.dirname(__FILE__) + '/shared/version'

describe "The --version command line option" do
  it_behaves_like "version option", "--version"
end
