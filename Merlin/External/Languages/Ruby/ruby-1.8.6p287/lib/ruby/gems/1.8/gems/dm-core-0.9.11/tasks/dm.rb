task :default => 'dm:spec'
task :spec    => 'dm:spec'
task :rcov    => 'dm:rcov'

namespace :spec do
  task :unit        => 'dm:spec:unit'
  task :integration => 'dm:spec:integration'
end

namespace :rcov do
  task :unit        => 'dm:rcov:unit'
  task :integration => 'dm:rcov:integration'
end

namespace :dm do
  def run_spec(name, files, rcov)
    Spec::Rake::SpecTask.new(name) do |t|
      t.spec_opts << '--colour' << '--loadby' << 'random'
      t.spec_files = Pathname.glob(ENV['FILES'] || files.to_s).map { |f| f.to_s }
      t.rcov = rcov
      t.rcov_opts << '--exclude' << 'spec,environment.rb'
      t.rcov_opts << '--text-summary'
      t.rcov_opts << '--sort' << 'coverage' << '--sort-reverse'
      t.rcov_opts << '--only-uncovered'
    end
  end

  unit_specs        = ROOT + 'spec/unit/**/*_spec.rb'
  integration_specs = ROOT + 'spec/integration/**/*_spec.rb'
  all_specs         = ROOT + 'spec/**/*_spec.rb'

  desc "Run all specifications"
  run_spec('spec', all_specs, false)

  desc "Run all specifications with rcov"
  run_spec('rcov', all_specs, true)

  namespace :spec do
    desc "Run unit specifications"
    run_spec('unit', unit_specs, false)

    desc "Run integration specifications"
    run_spec('integration', integration_specs, false)
  end

  namespace :rcov do
    desc "Run unit specifications with rcov"
    run_spec('unit', unit_specs, true)

    desc "Run integration specifications with rcov"
    run_spec('integration', integration_specs, true)
  end

  desc "Run all comparisons with ActiveRecord"
  task :perf do
    sh ROOT + 'script/performance.rb'
  end

  desc "Profile DataMapper"
  task :profile do
    sh ROOT + 'script/profile.rb'
  end
end
