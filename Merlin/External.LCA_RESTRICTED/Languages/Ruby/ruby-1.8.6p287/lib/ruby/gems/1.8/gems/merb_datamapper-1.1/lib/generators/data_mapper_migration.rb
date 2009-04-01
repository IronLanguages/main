Merb::Generators::MigrationGenerator.template :migration_datamapper, :orm => :datamapper do |t|
  t.source = File.join(File.dirname(__FILE__), 'templates/migration.rb')
  t.destination = "#{destination_directory}/#{file_name}.rb"
end
