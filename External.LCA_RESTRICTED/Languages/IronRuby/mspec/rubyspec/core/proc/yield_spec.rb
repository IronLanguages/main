require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/call'

not_supported_on :ironruby do  # TODO: IronRuby doesn't have all 1.9 grammar changes implemented yet
ruby_version_is "1.9" do
  language_version __FILE__, "call"

  describe "Proc#yield" do
    it_behaves_like :proc_call, :yield
    it_behaves_like :proc_call_block_args, :yield
  end

  describe "Proc#yield on a Proc created with Proc.new" do
    it_behaves_like :proc_call_on_proc_new, :yield
  end

  describe "Proc#yield on a Proc created with Kernel#lambda or Kernel#proc" do
    it_behaves_like :proc_call_on_proc_or_lambda, :yield
  end
end
end