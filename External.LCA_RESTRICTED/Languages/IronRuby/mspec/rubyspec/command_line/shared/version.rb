describe "version option", :shared => true do 
  it "exits after operation if it is the only option" do
    ruby_exe(nil, :options => @method).chomp.match(/ruby/i).should_not be_nil
  end

  it "returns the version string" do
    #This needs to be hardened for other implementations
    version = case RUBY_NAME
              when 'ruby.exe'
                RUBY_VERSION
              when 'ironruby'
                IRONRUBY_VERSION
              else
                flunk "need to implement this for #{RUBY_NAME}"
              end

    ruby_exe(nil, :options => @method).chomp.should include(version)
  end
end
