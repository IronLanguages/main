require File.dirname(__FILE__) + '/../lib/migration_runner'
require 'fileutils'
FileUtils.touch(File.join(Dir.pwd, "migration_test.db"))
# DataMapper.setup(:default, "sqlite3://#{Dir.pwd}/migration_test.db")
DataMapper.setup(:default, "postgres://localhost/migration_test")
# DataMapper.setup(:default, "mysql://localhost/migration_test")

DataMapper::Logger.new(STDOUT, :debug)
DataMapper.logger.debug( "Starting Migration" )

migration 1, :create_people_table do
  up do
    create_table :people do
      column :id,     Integer, :serial => true
      column :name,   String, :size => 50
      column :age,    Integer
    end
  end
  down do
    drop_table :people
  end
end

migration 2, :add_dob_to_people do
  up do
    modify_table :people do
      add_column :dob, DateTime, :nullable? => true
    end
  end

  down do
    modify_table :people do
      drop_column :dob
    end
  end
end

# migrate_down!
# migrate_up!
#
# class Person
#   include DataMapper::Resource
#
#   property :id, Integer, :serial => true
#   property :name, String, :size => 50
#   property :age, Integer
#   property :dob, DateTime, :default => Time.now
#
# end
#
# Person.create(:name => "Mark Bates", :age => 31)
# puts Person.first.inspect
# puts Person.all.inspect

if $0 == __FILE__
  if $*.first == "down"
    migrate_down!
  else
    migrate_up!
  end
end
