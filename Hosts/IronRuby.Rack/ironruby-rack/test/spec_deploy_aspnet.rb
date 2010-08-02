if defined?(RUBY_ENGINE) && RUBY_ENGINE == 'ironruby'

require File.expand_path('helpers', File.dirname(__FILE__))
DEPLOY_DIR = File.dirname(__FILE__) + '/../lib/rack/deploy'
require DEPLOY_DIR + '/aspnet'
TEMP_APP = File.join(System::IO::Path.get_temp_path, 'testapp')

context "Rack::Deploy::ASPNETApplication" do

  specify 'exists' do
    should.not.raise NameError do
      Rack::Deploy.const_get('ASPNETApplication')
    end
  end
  
  specify '.new' do
    app = Rack::Deploy::ASPNETApplication.new TEMP_APP
    app.instance_variable_get('@app_type').should == ''
    app.instance_variable_get('@app_dir').should == TEMP_APP
    File.expand_path(app.instance_variable_get('@template_dir')).should.equal(
      File.expand_path(File.join(DEPLOY_DIR, 'template')))
    File.expand_path(app.instance_variable_get('@bin_dir')).should.equal(
      File.expand_path(File.dirname(__FILE__) + '/../bin'))
    app.config.rack_version.should.nil
  end
  
  specify '.config' do
    app = Rack::Deploy::ASPNETApplication.new TEMP_APP
    app.instance_variable_get('@config').should.nil
    cfg = app.config
    app.instance_variable_get('@config').should.be cfg
    app.config.should == cfg
    app.config.should.instance_of Rack::Deploy::ASPNETConfig
  end
  
  specify '.generate' do
    app = Rack::Deploy::ASPNETApplication.new TEMP_APP
    app.generate
    File.directory?(File.join(TEMP_APP, 'log')).should == true
    ['config.ru', 'web.config', 'bin/IronRuby.dll', 'bin/IronRuby.Libraries.dll',
     'bin/IronRuby.Libraries.Yaml.dll', 'bin/IronRuby.Rack.dll', 'bin/Microsoft.Scripting.dll', 'bin/Microsoft.Scripting.Metadata.dll',
     'bin/Microsoft.Dynamic.dll', 'bin/Cassini.exe', 'bin/ir.exe', 'bin/ir.exe.config'
    ].each{|f| File.exist?(File.join(TEMP_APP, f)).should == true}
  end
  
  after(:all) do
    require 'fileutils'
    FileUtils.rm_r TEMP_APP
  end
end

else
  $stderr.puts "Skipping Rack::Deploy::ASPNET tests (IronRuby is required). http://ironruby.net/download."
end
