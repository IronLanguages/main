Autotest.add_discovery do
  style = []
  style << "rails" if File.exist? 'config/environment.rb'
  style << "camping" if File.exist? 'test/camping_test_case.rb'
  style
end
