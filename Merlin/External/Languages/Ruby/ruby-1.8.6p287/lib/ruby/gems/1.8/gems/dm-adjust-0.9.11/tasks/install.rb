def sudo_gem(cmd)
  sh "#{SUDO} #{RUBY} -S gem #{cmd}", :verbose => false
end

desc "Install #{GEM_NAME} #{GEM_VERSION}"
task :install => [ :package ] do
  sudo_gem "install --local pkg/#{GEM_NAME}-#{GEM_VERSION} --no-update-sources"
end

desc "Uninstall #{GEM_NAME} #{GEM_VERSION}"
task :uninstall => [ :clobber ] do
  sudo_gem "uninstall #{GEM_NAME} -v#{GEM_VERSION} -Ix"
end
