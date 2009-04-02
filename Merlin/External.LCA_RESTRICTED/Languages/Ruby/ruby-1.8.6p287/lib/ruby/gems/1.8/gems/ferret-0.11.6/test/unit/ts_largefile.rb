if ENV['FERRET_DEV']
  require File.join(File.dirname(__FILE__), "../test_helper.rb")
  load_test_dir('unit/largefile')
end
