task 'ci:doc' => :doc

namespace :ci do

  task :prepare do
    rm_rf ROOT + "ci"
    mkdir_p ROOT + "ci"
    mkdir_p ROOT + "ci/doc"
    mkdir_p ROOT + "ci/cyclomatic"
    mkdir_p ROOT + "ci/token"
  end

  Spec::Rake::SpecTask.new(:spec => :prepare) do |t|
    t.spec_opts = ["--colour", "--format", "specdoc", "--format", "html:#{ROOT}/ci/rspec_report.html", "--diff"]
    t.spec_files = Pathname.glob((ROOT + 'spec/**/*_spec.rb').to_s)
    unless ENV['NO_RCOV']
      t.rcov = true
      t.rcov_opts << '--exclude' << "spec,gems"
      t.rcov_opts << '--text-summary'
      t.rcov_opts << '--sort' << 'coverage' << '--sort-reverse'
      t.rcov_opts << '--only-uncovered'
    end
  end

  task :saikuro => :prepare do
    system "saikuro -c -i lib -y 0 -w 10 -e 15 -o ci/cyclomatic"
    mv 'ci/cyclomatic/index_cyclo.html', 'ci/cyclomatic/index.html'

    system "saikuro -t -i lib -y 0 -w 20 -e 30 -o ci/token"
    mv 'ci/token/index_token.html', 'ci/token/index.html'
  end

end

#task :ci => %w[ ci:spec ci:doc ci:saikuro install ci:publish ]  # yard-related tasks do not work yet
task :ci => %w[ ci:spec ]
