require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "ENV.[]" do
  before :each do
    @variable_name = 'USER'
    platform_is :windows do
      @variable_name = 'USERNAME'
    end
  end

  it "returns the specified environment variable" do
    ENV[@variable_name].should == username
  end

  it "returns nil if the variable isn't found" do
    ENV["this_var_is_never_set"].should == nil
  end

  platform_is :windows do  
    it "does not expand %HOME% if it is already set" do
      EnvSpecs.with_temp_ENV do
        ENV['HOME'] = 'x:\Dummy\..\User\Jane Doe'
        ruby_exe("puts ENV['HOME']").chomp.should == 'x:\Dummy\..\User\Jane Doe'
      end
    end
 
    it "sets HOME to %HOMEDRIVE%/%HOMEPATH% if %HOME% is not set" do
      EnvSpecs.with_temp_ENV do
        ENV['HOMEDRIVE'] = 'x:'
        ENV['HOMEPATH'] = '\User\Jane Doe'
        ENV['USERPROFILE'] = 'y:\User\John Doe'
        ruby_exe("puts ENV['HOME']").chomp.should == 'x:/User/Jane Doe'
      end  
    end
 
    it "sets HOME to %HOMEDRIVE% if %HOME% is not set" do
      EnvSpecs.with_temp_ENV do
        ENV['HOMEDRIVE'] = 'x:'
        ENV['USERPROFILE'] = 'y:\User\John Doe'
        ruby_exe("puts ENV['HOME']").chomp.should == "x:/"
      end
    end
 
    it "sets HOME to %HOMEPATH% if %HOME% is not set" do
      EnvSpecs.with_temp_ENV do
        ENV['HOMEPATH'] = '\User\Jane Doe'
        ENV['USERPROFILE'] = 'y:\User\John Doe'
        ruby_exe("puts ENV['HOME']").chomp.should == "/User/Jane Doe"
      end
    end
  
    it "sets HOME to %USERPROFILE% if %HOMEDRIVE% or %HOMEPATH% are not set" do
      EnvSpecs.with_temp_ENV do
        ENV['USERPROFILE'] = 'y:\User\John Doe'
        ruby_exe("puts ENV['HOME']").chomp.should == "y:/User/John Doe"
      end
    end

    ruby_version_is "" ... "1.9" do
      it "does not set HOME if none of %HOMEDRIVE%, %HOMEPATH% or %USERPROFILE% are set" do
        EnvSpecs.with_temp_ENV do
          ENV['HOME'] = nil
          ENV['HOMEDRIVE'] = nil
          ENV['HOMEPATH'] = nil
          ENV['USERPROFILE'] = nil
          ruby_exe("puts puts ENV['HOME'], ENV['HOME'].nil?").chomp.should =~ /^true$/
        end
      end
    end

    ruby_version_is "1.9" do
      it "does set HOME if none of %HOMEDRIVE%, %HOMEPATH% or %USERPROFILE% are set" do
        EnvSpecs.with_temp_ENV do
          ENV['HOME'] = nil
          ENV['HOMEDRIVE'] = nil
          ENV['HOMEPATH'] = nil
          ENV['USERPROFILE'] = nil
          ruby_exe("puts puts ENV['HOME'], ENV['HOME'].nil?").chomp.should =~ /^false$/
        end
      end
      
      it "uses the locale encoding" do
        ENV[@variable_name].encoding.should == Encoding.find('locale')
      end
    end
  end
end
