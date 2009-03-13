WIN32 = (RUBY_PLATFORM =~ /win32|mingw|bccwin|cygwin/) rescue nil
SUDO = WIN32 ? '' : ('sudo' unless ENV['SUDOLESS'])

desc "Install #{GEM_NAME}"
if WIN32
  task :install => :gem do
    system %{gem install --no-rdoc --no-ri -l pkg/#{GEM_NAME}-#{GEM_VERSION}.gem}
  end
  namespace :dev do
    desc 'Install for development (for windows)'
    task :winstall => :gem do
      warn "You can now call 'rake install' instead of 'rake dev:winstall'."
      system %{gem install --no-rdoc --no-ri -l pkg/#{GEM_NAME}-#{GEM_VERSION}.gem}
    end
  end
else
  task :install => :package do
    sh %{#{SUDO} gem install --local pkg/#{GEM_NAME}-#{GEM_VERSION}.gem}
  end
end
