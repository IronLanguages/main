module Merb::Generators
  
  class SessionMigrationGenerator < Generator

    def self.source_root
      File.join(super, 'component', 'session_migration')
    end
    
    desc <<-DESC
      Generates a new session migration.
    DESC
    
    option :orm, :desc => 'Object-Relation Mapper to use (one of: none, activerecord, datamapper, sequel)'
    
    def version
      # TODO: handle ActiveRecord timestamped migrations
      format("%03d", current_migration_nr + 1)
    end

    protected
    
    def destination_directory
      File.join(destination_root, 'schema', 'migrations')
    end
    
    def current_migration_nr
      current_migration_number = Dir["#{destination_directory}/*"].map{|f| File.basename(f).match(/^(\d+)/)[0].to_i  }.max.to_i
    end
    
  end
  
  add :session_migration, SessionMigrationGenerator
  
end