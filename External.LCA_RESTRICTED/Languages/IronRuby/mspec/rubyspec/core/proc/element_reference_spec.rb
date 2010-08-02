require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/call'

ruby_version_is "1.8"..."1.9" do
  language_version __FILE__, "call"
end

ruby_version_is "1.9" do
not_supported_on :ironruby do  # TODO: IronRuby doesn't have all 1.9 grammar changes implemented yet
  language_version __FILE__, "call"
end
end

describe "Proc#[]" do
  it_behaves_like :proc_call, :[]

  not_supported_on :ironruby do  # TODO: IronRuby doesn't have all 1.9 grammar changes implemented yet
  ruby_version_is "1.8.7" do
    it_behaves_like :proc_call_block_args, :[]
  end
  end
end

describe "Proc#call on a Proc created with Proc.new" do
  it_behaves_like :proc_call_on_proc_new, :call
end

describe "Proc#call on a Proc created with Kernel#lambda or Kernel#proc" do
  it_behaves_like :proc_call_on_proc_or_lambda, :call
end
