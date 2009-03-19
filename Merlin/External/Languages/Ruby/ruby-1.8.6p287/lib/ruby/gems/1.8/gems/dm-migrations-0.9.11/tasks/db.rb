namespace :db do

  # pass the relative path to the migrations directory by MIGRATION_DIR
  task :setup_migration_dir do
    unless defined?(MIGRATION_DIR)
      migration_dir = ENV["MIGRATION_DIR"] || File.join("db", "migrations")
      MIGRATION_DIR = File.expand_path(File.join(File.dirname(__FILE__), migration_dir))
    end
    FileUtils.mkdir_p MIGRATION_DIR
  end

  # set DIRECTION to migrate down
  desc "Run your system's migrations"
  task :migrate => [:setup_migration_dir] do
    require File.expand_path(File.join(File.dirname(__FILE__), "lib", "migration_runner.rb"))
    require File.expand_path(File.join(MIGRATION_DIR, "config.rb"))

    Dir[File.join(MIGRATION_DIR, "*.rb")].each { |file| require file }

    ENV["DIRECTION"] != "down" ? migrate_up! : migrate_down!
  end
end
