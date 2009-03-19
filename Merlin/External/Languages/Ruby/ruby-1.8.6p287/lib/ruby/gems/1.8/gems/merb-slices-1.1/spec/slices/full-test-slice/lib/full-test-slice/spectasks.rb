namespace :slices do
  namespace :full_test_slice do
      
    desc "Run slice specs within the host application context"
    task :spec => [ "spec:explain", "spec:default" ]
    
    namespace :spec do
      
      slice_root = File.expand_path(File.join(File.dirname(__FILE__), '..', '..'))
      
      task :explain do
        puts "\nNote: By running FullTestSlice specs inside the application context any\n" +
             "overrides could break existing specs. This isn't always a problem,\n" +
             "especially in the case of views. Use these spec tasks to check how\n" +
             "well your application conforms to the original slice implementation."
      end
      
      Spec::Rake::SpecTask.new('default') do |t|
        t.spec_opts = ["--format", "specdoc", "--colour"]
        t.spec_files = Dir["#{slice_root}/spec/**/*_spec.rb"].sort
      end

      desc "Run all model specs, run a spec for a specific Model with MODEL=MyModel"
      Spec::Rake::SpecTask.new('model') do |t|
        t.spec_opts = ["--format", "specdoc", "--colour"]
        if(ENV['MODEL'])
          t.spec_files = Dir["#{slice_root}/spec/models/**/#{ENV['MODEL']}_spec.rb"].sort
        else
          t.spec_files = Dir["#{slice_root}/spec/models/**/*_spec.rb"].sort
        end
      end

      desc "Run all controller specs, run a spec for a specific Controller with CONTROLLER=MyController"
      Spec::Rake::SpecTask.new('controller') do |t|
        t.spec_opts = ["--format", "specdoc", "--colour"]
        if(ENV['CONTROLLER'])
          t.spec_files = Dir["#{slice_root}/spec/controllers/**/#{ENV['CONTROLLER']}_spec.rb"].sort
        else    
          t.spec_files = Dir["#{slice_root}/spec/controllers/**/*_spec.rb"].sort
        end
      end

      desc "Run all view specs, run specs for a specific controller (and view) with CONTROLLER=MyController (VIEW=MyView)"
      Spec::Rake::SpecTask.new('view') do |t|
        t.spec_opts = ["--format", "specdoc", "--colour"]
        if(ENV['CONTROLLER'] and ENV['VIEW'])
          t.spec_files = Dir["#{slice_root}/spec/views/**/#{ENV['CONTROLLER']}/#{ENV['VIEW']}*_spec.rb"].sort
        elsif(ENV['CONTROLLER'])
          t.spec_files = Dir["#{slice_root}/spec/views/**/#{ENV['CONTROLLER']}/*_spec.rb"].sort
        else
          t.spec_files = Dir["#{slice_root}/spec/views/**/*_spec.rb"].sort
        end
      end

      desc "Run all specs and output the result in html"
      Spec::Rake::SpecTask.new('html') do |t|
        t.spec_opts = ["--format", "html"]
        t.libs = ['lib', 'server/lib' ]
        t.spec_files = Dir["#{slice_root}/spec/**/*_spec.rb"].sort
      end
      
    end
    
  end
end