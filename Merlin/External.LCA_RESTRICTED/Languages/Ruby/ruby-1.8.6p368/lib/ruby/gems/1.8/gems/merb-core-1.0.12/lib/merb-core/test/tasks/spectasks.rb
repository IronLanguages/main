desc "Run specs, run a specific spec with TASK=spec/path_to_spec.rb"
task :spec => [ "spec:default" ]

namespace :spec do
  OPTS_FILENAME = "./spec/spec.opts"
  if File.exist?(OPTS_FILENAME)
    SPEC_OPTS = ["--options", OPTS_FILENAME]
  else
    SPEC_OPTS = ["--color", "--format", "specdoc"]
  end
  
  Spec::Rake::SpecTask.new('default') do |t|
      t.spec_opts = SPEC_OPTS
    if(ENV['TASK'])
      t.spec_files = [ENV['TASK']]
    else
      t.spec_files = Dir['spec/**/*_spec.rb'].sort
    end
  end

  desc "Run all model specs, run a spec for a specific Model with MODEL=MyModel"
  Spec::Rake::SpecTask.new('model') do |t|
    t.spec_opts = SPEC_OPTS
    if(ENV['MODEL'])
      t.spec_files = Dir["spec/models/**/#{ENV['MODEL']}_spec.rb"].sort
    else
      t.spec_files = Dir['spec/models/**/*_spec.rb'].sort
    end
  end

  desc "Run all request specs, run a spec for a specific Request with REQUEST=MyRequest"
  Spec::Rake::SpecTask.new('request') do |t|
    t.spec_opts = SPEC_OPTS
    if(ENV['REQUEST'])
      t.spec_files = Dir["spec/requests/**/#{ENV['REQUEST']}_spec.rb"].sort
    else    
      t.spec_files = Dir['spec/requests/**/*_spec.rb'].sort
    end
  end
  
  desc "Run all controller specs, run a spec for a specific Controller with CONTROLLER=MyController"
  Spec::Rake::SpecTask.new('controller') do |t|
    t.spec_opts = SPEC_OPTS
    if(ENV['CONTROLLER'])
      t.spec_files = Dir["spec/controllers/**/#{ENV['CONTROLLER']}_spec.rb"].sort
    else    
      t.spec_files = Dir['spec/controllers/**/*_spec.rb'].sort
    end
  end
  
  desc "Run all view specs, run specs for a specific controller (and view) with CONTROLLER=MyController (VIEW=MyView)"
  Spec::Rake::SpecTask.new('view') do |t|
    t.spec_opts = SPEC_OPTS
    if(ENV['CONTROLLER'] and ENV['VIEW'])
      t.spec_files = Dir["spec/views/**/#{ENV['CONTROLLER']}/#{ENV['VIEW']}*_spec.rb"].sort
    elsif(ENV['CONTROLLER'])
      t.spec_files = Dir["spec/views/**/#{ENV['CONTROLLER']}/*_spec.rb"].sort
    else
      t.spec_files = Dir['spec/views/**/*_spec.rb'].sort
    end
  end

  desc "Run all specs and output the result in html"
  Spec::Rake::SpecTask.new('html') do |t|
    t.spec_opts = ["--format", "html"]
    t.libs = ['lib', 'server/lib' ]
    t.spec_files = Dir['spec/**/*_spec.rb'].sort
  end

  desc "Run specs and check coverage with rcov"
  Spec::Rake::SpecTask.new('coverage') do |t|
    t.spec_opts = SPEC_OPTS
    t.spec_files = Dir['spec/**/*_spec.rb'].sort
    t.libs = ['lib', 'server/lib' ]
    t.rcov = true
    t.rcov_opts = ["--exclude 'config,spec,#{Gem::path.join(',')}'"]    
  end
end
