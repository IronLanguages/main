require File.dirname(__FILE__) + '/../spec_helper'

describe "ir.exe without ir.exe.config" do
  before(:all) do
    bin = ENV['DLR_BIN'] || File.join(ENV['DLR_ROOT'], "bin", "debug")
    temp_bin = tmp("bin")
    Dir.foreach(bin) do |file|
      FileUtils.cp(File.join(bin, file), temp_bin) if file =~ /^(IronRuby|ir|Microsoft)/
    end
    FileUtils.rm_f(File.join(temp_bin, "ir.exe.config"))
    @old_ruby_exe, ENV['RUBY_EXE'] = ENV['RUBY_EXE'], File.join(temp_bin, "ir.exe")
  end

  after(:all) do
    ENV['RUBY_EXE'] = @old_ruby_exe
  end

  it "still runs" do
    ruby_exe("puts 'Hello'").chomp.should == "Hello"
  end

  it "can still host IR.exe" do
    ruby_exe(fixture(__FILE__, "hosting.rb")).chomp.should == "2"
  end
end
