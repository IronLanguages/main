require File.dirname(__FILE__) + '/../spec_helper'

describe "ir.exe without ir.exe.config" do
  before(:each) do
    bin = ENV['ROWAN_BIN'] || File.join(ENV['MERLIN_ROOT'], "bin", "debug")
    @config_old = File.join(bin, "ir.exe.config")
    @config_temp = File.join(bin, "not_ir.exe.config")

    FileUtils.mv(@config_old, @config_temp)
  end

  after(:each) do
    FileUtils.mv(@config_temp, @config_old)
  end

  it "still runs" do
    ruby_exe("puts 'Hello'").chomp.should == "Hello"
  end

  it "can still host IR.exe" do
    require System::Reflection::Assembly.get_assembly(IronRuby.create_engine.class.to_clr_type).to_s
    engine = IronRuby.create_engine(System::Action.of(Microsoft::Scripting::Hosting::LanguageSetup).new {|a| })
    engine.execute("1+1").should == 2
  end
end
