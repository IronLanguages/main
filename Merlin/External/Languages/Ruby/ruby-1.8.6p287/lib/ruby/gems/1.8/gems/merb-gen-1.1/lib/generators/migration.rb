module Merb::Generators
  
  class MigrationGenerator < Generator

    def self.source_root
      File.join(super, 'component', 'migration')
    end
    
    desc <<-DESC
      Generates a new database migration.
    DESC
    
    option :orm, :desc => 'Object-Relation Mapper to use (one of: none, activerecord, datamapper, sequel)'
    option :model, :as => :boolean, :desc => 'Specify this option to generate a migration which creates a table for the provided model'
    
    first_argument :name, :required => true
    second_argument :attributes, :as => :hash, :default => {}
    
    def table_name
      self.name.snake_case.pluralize
    end
    
    def class_name
      "#{self.name.camel_case}Migration"
    end

    def migration_name
      self.name.snake_case
    end
    
    def file_name
      "#{version}_#{migration_name}_migration"
    end
    
    def version
      # TODO: handle ActiveRecord timestamped migrations
      n = options[:delete] ? current_migration_nr : current_migration_nr + 1
      format("%03d", n)
    end

    protected
    
    def destination_directory
      File.join(destination_root, 'schema', 'migrations')
    end
    
    def current_migration_nr
      current_migration_number = Dir["#{destination_directory}/*"].map do |f|
        File.basename(f).match(/^(\d+)/)[0].to_i
      end.max.to_i
    end
    
  end
  
  add :migration, MigrationGenerator
  
end
