require File.dirname(__FILE__) + '/migration'

module DataMapper
  module MigrationRunner
    # Creates a new migration, and adds it to the list of migrations to be run.
    # Migrations can be defined in any order, they will be sorted and run in the
    # correct order.
    #
    # The order that migrations are run in is set by the first argument. It is not
    # neccessary that this be unique; migrations with the same version number are
    # expected to be able to be run in any order.
    #
    # The second argument is the name of the migration. This name is used internally
    # to track if the migration has been run. It is required that this name be unique
    # across all migrations.
    #
    # Addtionally, it accepts a number of options:
    # * <tt>:database</tt> If you defined several DataMapper::database instances use this
    #   to choose which one to run the migration gagainst. Defaults to <tt>:default</tt>.
    #   Migrations are tracked individually per database.
    # * <tt>:verbose</tt> true/false, defaults to true. Determines if the migration should
    #   output its status messages when it runs.
    #
    # Example of a simple migration:
    #
    #   migration( 1, :create_people_table ) do
    #     up do
    #       create_table :people do
    #         column :id,     Integer, :serial => true
    #         column :name,   String, :size => 50
    #         column :age,    Integer
    #       end
    #     end
    #     down do
    #       drop_table :people
    #     end
    #   end
    #
    # Its recommended that you stick with raw SQL for migrations that manipulate data. If
    # you write a migration using a model, then later change the model, there's a
    # possibility the migration will no longer work. Using SQL will always work.
    def migration( number, name, opts = {}, &block )
      raise "Migration name conflict: '#{name}'" if migrations.map { |m| m.name }.include?(name.to_s)

      migrations << DataMapper::Migration.new( number, name.to_s, opts, &block )
    end

    # Run all migrations that need to be run. In most cases, this would be called by a
    # rake task as part of a larger project, but this provides the ability to run them
    # in a script or test.
    #
    # has an optional argument 'level' which if supplied, only performs the migrations
    # with a position less than or equal to the level.
    def migrate_up!(level = nil)
      migrations.sort.each do |migration|
        if level.nil?
          migration.perform_up()
        else
          migration.perform_up() if migration.position <= level.to_i
        end
      end
    end

    # Run all the down steps for the migrations that have already been run.
    #
    # has an optional argument 'level' which, if supplied, only performs the
    # down migrations with a postion greater than the level.
    def migrate_down!(level = nil)
      migrations.sort.reverse.each do |migration|
        if level.nil?
          migration.perform_down()
        else
          migration.perform_down() if migration.position > level.to_i
        end
      end
    end

    def migrations
      @migrations ||= []
    end

  end
end

include DataMapper::MigrationRunner
