module DataMapper
  module Constraints
    module DataObjectsAdapter
      module SQL

        ##
        # generates all foreign key create constraint statements for valid relationships
        #   given repository and a model
        #
        # This wraps calls to create_constraints_statement
        #
        # @see #create_constraints_statement
        #
        # @param repository_name [Symbol] Name of the repository to constrain
        #
        # @param model [DataMapper::Model] Model to constrain
        #
        # @return [Array[String]] List of statements to create constraints
        #
        #
        # @api public
        def create_constraints_statements(repository_name, model)
          model.many_to_one_relationships.map do |relationship|

            table_name      = model.storage_name(repository_name)
            constraint_name = constraint_name(table_name, relationship.name)
            next if constraint_exists?(table_name, constraint_name)

            keys          = relationship.child_key.map { |key| property_to_column_name(model.repository(repository_name), key, false) }
            parent        = relationship.parent_model
            foreign_table = parent.storage_name(repository_name)
            foreign_keys  = parent.key.map { |key| property_to_column_name(parent.repository(repository_name), key, false) }

            #Anonymous relationshps for :through => Resource
            one_to_many_relationship = parent.relationships.values.select { |rel|
              rel.options[:near_relationship_name] == Extlib::Inflection.tableize(model.name).to_sym
            }.first

            one_to_many_relationship ||= parent.relationships.values.select { |rel|
              rel.child_model == model
            }.first

            delete_constraint_type = case one_to_many_relationship.nil? ? :protect : one_to_many_relationship.delete_constraint
            when :protect, nil
              "NO ACTION"
            when :destroy, :destroy!
              "CASCADE"
            when :set_nil
              "SET NULL"
            when :skip
              nil
            end

            create_constraints_statement(table_name, constraint_name, keys, foreign_table, foreign_keys, delete_constraint_type) if delete_constraint_type
          end.compact
        end

        ##
        # generates all foreign key destroy constraint statements for valid relationships
        #   given repository and a model
        #
        # This wraps calls to destroy_constraints_statement
        #
        # @see #destroy_constraints_statement
        #
        # @param repository_name [Symbol] Name of the repository to constrain
        #
        # @param model [DataMapper::Model] Model to constrain
        #
        # @return [Array[String]] List of statements to destroy constraints
        #
        #
        # @api public
        def destroy_constraints_statements(repository_name, model)
          model.many_to_one_relationships.map do |relationship|
            table_name      = model.storage_name(repository_name)
            constraint_name = constraint_name(table_name, relationship.name)
            next unless constraint_exists?(table_name, constraint_name)

            destroy_constraints_statement(table_name, constraint_name)

          end.compact
        end

        private

        ##
        # Generates the SQL statement to create a constraint
        #
        # @param table_name [String] name of table to constrain
        #
        # @param constraint_name [String] name of foreign key constraint
        #
        # @param keys [Array[String]] keys that refer to another table
        #
        # @param foreign_table [String] table fk refers to
        #
        # @param foreign_keys [Array[String]] keys on foreign table that constraint refers to
        #
        # @param delete_constraint_type [String] the constraint to add to the table
        #
        # @return [String] SQL DDL Statement to create a constraint
        #
        # @api private
        def create_constraints_statement(table_name, constraint_name, keys, foreign_table, foreign_keys, delete_constraint_type)
          <<-EOS.compress_lines
            ALTER TABLE #{quote_table_name(table_name)}
            ADD CONSTRAINT #{quote_constraint_name(constraint_name)}
            FOREIGN KEY (#{keys * ', '})
            REFERENCES #{quote_table_name(foreign_table)} (#{foreign_keys * ', '})
            ON DELETE #{delete_constraint_type}
            ON UPDATE #{delete_constraint_type}
          EOS
        end

        ##
        # Generates the SQL statement to destroy a constraint
        #
        # @param table_name [String] name of table to constrain
        #
        # @param constraint_name [String] name of foreign key constraint
        #
        # @return [String] SQL DDL Statement to destroy a constraint
        #
        # @api private
        def destroy_constraints_statement(table_name, constraint_name)
          <<-EOS.compress_lines
            ALTER TABLE #{quote_table_name(table_name)}
            DROP CONSTRAINT #{quote_constraint_name(constraint_name)}
          EOS
        end

        ##
        # generates a unique constraint name given a table and a relationships
        #
        # @param table_name [String] name of table to constrain
        #
        # @param relationships_name [String] name of the relationship to constrain
        #
        # @return [String] name of the constraint
        #
        # @api private
        def constraint_name(table_name, relationship_name)
          "#{table_name}_#{relationship_name}_fk"
        end

        ##
        # SQL quotes a foreign key constraint name
        #
        # @see #quote_table_name
        #
        # @param foreign_key [String] SQL quotes a foreign key name
        #
        # @return [String] quoted constraint name
        #
        # @api private
        def quote_constraint_name(foreign_key)
          quote_table_name(foreign_key)
        end
      end

      module Migration
        def self.included(migrator)
          migrator.extend(ClassMethods)
          migrator.before_class_method :auto_migrate_down, :auto_migrate_constraints_down
          migrator.after_class_method :auto_migrate_up, :auto_migrate_constraints_up
        end

        module ClassMethods
          def auto_migrate_constraints_down(repository_name, *descendants)
            descendants = DataMapper::Resource.descendants.to_a if descendants.empty?
            descendants.each do |model|
              repository_name ||= model.repository(repository_name).name
              if model.storage_exists?(repository_name)
                adapter = model.repository(repository_name).adapter
                next unless adapter.respond_to?(:destroy_constraints_statements)
                statements = adapter.destroy_constraints_statements(repository_name, model)
                statements.each {|stmt| adapter.execute(stmt) }
              end
            end
          end

          def auto_migrate_constraints_up(retval, repository_name, *descendants)
            descendants = DataMapper::Resource.descendants.to_a if descendants.empty?
            descendants.each do |model|
              repository_name ||= model.repository(repository_name).name
              adapter = model.repository(repository_name).adapter
              next unless adapter.respond_to?(:create_constraints_statements)
              statements = adapter.create_constraints_statements(repository_name, model)
              statements.each {|stmt| adapter.execute(stmt) }
            end
          end
        end
      end
    end
  end
end
