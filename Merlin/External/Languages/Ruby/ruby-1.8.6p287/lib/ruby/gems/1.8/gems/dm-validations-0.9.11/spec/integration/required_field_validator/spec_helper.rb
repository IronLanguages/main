if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES

  #
  # SCMs
  #
  # This example may look stupid (I am sure it is),
  # but it is way better than foobars and easier to read/add cases
  # compared to gardening examples because every software engineer has idea
  # about SCMs and not every software engineer does gardening often.
  #

  class ScmOperation
    include DataMapper::Resource

    #
    # Property
    #

    property :id,                 Integer, :serial => true

    # operation name
    property :name,               String,  :auto_validation => false

    property :committer_name,     String,  :auto_validation => false, :default => "Just another Ruby hacker"
    property :author_name,        String,  :auto_validation => false, :default => "Just another Ruby hacker"
    property :network_connection, Boolean, :auto_validation => false
    property :message,            Text,    :auto_validation => false
    property :clean_working_copy, Boolean, :auto_validation => false

    #
    # Validations
    #

    validates_present :name
  end

  class SubversionOperation < ScmOperation
    #
    # Validations
    #

    validates_present :network_connection, :when => [:committing, :log_viewing]
  end

  class GitOperation < ScmOperation
    #
    # Validations
    #

    validates_present :author_name,        :when => :committing
    validates_present :committer_name,     :when => :committing

    validates_present :message,            :when => :committing
    validates_present :network_connection, :when => [:pushing, :pulling], :message => {
      :pushing => "though git is advanced, it cannot push without network connectivity",
      :pulling => "you must have network connectivity to pull from others"
    }
    validates_present :clean_working_copy, :when => :pulling
  end


  [ScmOperation, SubversionOperation, GitOperation].each do |dm_resource|
    dm_resource.auto_migrate!
  end

  __dir__ = File.dirname(__FILE__)
  require File.join(__dir__, "shared_examples")
end
