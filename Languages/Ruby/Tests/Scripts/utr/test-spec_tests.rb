class UnitTestSetup
  def initialize
    @name = "Test::Spec"
    super
  end
  
  VERSION = '0.10.0'
  
  def require_files
    if defined?(RUBY_ENGINE) && RUBY_ENGINE == 'ironruby'
      require 'test/ispec'
    else
      require 'rubygems'
      gem 'test-spec', "=#{VERSION}"
    end
  end

  def gather_files
    @lib_tests_dir = File.expand_path("Languages/Ruby/Tests/Libraries/test-spec-#{VERSION}", ENV["DLR_ROOT"])
    @all_test_files = Dir.glob("#{@lib_tests_dir}/test/test*.rb") + Dir.glob("#{@lib_tests_dir}/test/spec*.rb")
  end

  def sanity
    # Some tests load data assuming the current folder
    Dir.chdir(@lib_tests_dir)
  end

  def exclude_critical_files
    @all_test_files = @all_test_files.delete_if{|i| i =~ /spec_mocha/}
  end
  
  def disable_mri_failures
  #  disable_spec "mocha", 
  #       "works with test/spec",
  #       "works with test/spec and Enterprise example"
  #  
  #  disable_spec "stubba",
  #       "works with test/spec and instance method stubbing",
  #       "works with test/spec and class method stubbing",
  #       "works with test/spec and global instance method stubbing"
  
    disable_spec 'should.output',
        'works with readline',
        'works for puts',
        'works for print'
        
    disable_spec 'flexmock', 'should handle failures during use'
  end
end
