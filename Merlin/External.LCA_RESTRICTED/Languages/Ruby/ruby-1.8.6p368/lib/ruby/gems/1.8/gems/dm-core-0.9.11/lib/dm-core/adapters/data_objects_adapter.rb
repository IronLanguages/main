gem 'data_objects', '~>0.9.11'
require 'data_objects'

module DataMapper
  module Adapters
    # You must inherit from the DoAdapter, and implement the
    # required methods to adapt a database library for use with the DataMapper.
    #
    # NOTE: By inheriting from DataObjectsAdapter, you get a copy of all the
    # standard sub-modules (Quoting, Coersion and Queries) in your own Adapter.
    # You can extend and overwrite these copies without affecting the originals.
    class DataObjectsAdapter < AbstractAdapter
      def create(resources)
        created = 0
        resources.each do |resource|
          repository = resource.repository
          model      = resource.model
          attributes = resource.dirty_attributes

          # TODO: make a model.identity_field method
          identity_field = model.key(repository.name).detect { |p| p.serial? }

          statement = create_statement(repository, model, attributes.keys, identity_field)
          bind_values = attributes.values

          result = execute(statement, *bind_values)

          if result.to_i == 1
            if identity_field
              identity_field.set!(resource, result.insert_id)
            end
            created += 1
          end
        end
        created
      end

      def read_many(query)
        Collection.new(query) do |collection|
          with_connection do |connection|
            command = connection.create_command(read_statement(query))
            command.set_types(query.fields.map { |p| p.primitive })

            begin
              bind_values = query.bind_values.map do |v|
                v == [] ? [nil] : v
              end
              reader = command.execute_reader(*bind_values)

              while(reader.next!)
                collection.load(reader.values)
              end
            ensure
              reader.close if reader
            end
          end
        end
      end

      def read_one(query)
        with_connection do |connection|
          command = connection.create_command(read_statement(query))
          command.set_types(query.fields.map { |p| p.primitive })

          begin
            reader = command.execute_reader(*query.bind_values)

            if reader.next!
              query.model.load(reader.values, query)
            end
          ensure
            reader.close if reader
          end
        end
      end

      def update(attributes, query)
        statement = update_statement(attributes.keys, query)
        bind_values = attributes.values + query.bind_values
        execute(statement, *bind_values).to_i
      end

      def delete(query)
        statement = delete_statement(query)
        execute(statement, *query.bind_values).to_i
      end

      # Database-specific method
      def execute(statement, *bind_values)
        with_connection do |connection|
          command = connection.create_command(statement)
          command.execute_non_query(*bind_values)
        end
      end

      def query(statement, *bind_values)
        with_reader(statement, bind_values) do |reader|
          results = []

          if (fields = reader.fields).size > 1
            fields = fields.map { |field| Extlib::Inflection.underscore(field).to_sym }
            struct = Struct.new(*fields)

            while(reader.next!) do
              results << struct.new(*reader.values)
            end
          else
            while(reader.next!) do
              results << reader.values.at(0)
            end
          end

          results
        end
      end

      protected

      def normalize_uri(uri_or_options)
        if uri_or_options.kind_of?(String) || uri_or_options.kind_of?(Addressable::URI)
          uri_or_options = DataObjects::URI.parse(uri_or_options)
        end

        if uri_or_options.kind_of?(DataObjects::URI)
          return uri_or_options
        end

        query = uri_or_options.except(:adapter, :username, :password, :host, :port, :database).map { |pair| pair.join('=') }.join('&')
        query = nil if query.blank?

        return DataObjects::URI.parse(Addressable::URI.new(
          :scheme   => uri_or_options[:adapter].to_s,
          :user     => uri_or_options[:username],
          :password => uri_or_options[:password],
          :host     => uri_or_options[:host],
          :port     => uri_or_options[:port],
          :path     => uri_or_options[:database],
          :query    => query
        ))
      end

      # TODO: clean up once transaction related methods move to dm-more/dm-transactions
      def create_connection
        if within_transaction?
          current_transaction.primitive_for(self).connection
        else
          # DataObjects::Connection.new(uri) will give you back the right
          # driver based on the Uri#scheme.
          DataObjects::Connection.new(@uri)
        end
      end

      # TODO: clean up once transaction related methods move to dm-more/dm-transactions
      def close_connection(connection)
        connection.close unless within_transaction? && current_transaction.primitive_for(self).connection == connection
      end

      private

      def initialize(name, uri_or_options)
        super

        # Default the driver-specifc logger to DataMapper's logger
        if driver_module = DataObjects.const_get(@uri.scheme.capitalize) rescue nil
          driver_module.logger = DataMapper.logger if driver_module.respond_to?(:logger=)
        end
      end

      def with_connection
        connection = nil
        begin
          connection = create_connection
          return yield(connection)
        rescue => e
          DataMapper.logger.error(e.to_s)
          raise e
        ensure
          close_connection(connection) if connection
        end
      end

      def with_reader(statement, bind_values = [])
        with_connection do |connection|
          reader = nil
          begin
            reader = connection.create_command(statement).execute_reader(*bind_values)
            return yield(reader)
          ensure
            reader.close if reader
          end
        end
      end

      # This model is just for organization. The methods are included into the
      # Adapter below.
      module SQL
        private

        # Adapters requiring a RETURNING syntax for INSERT statements
        # should overwrite this to return true.
        def supports_returning?
          false
        end

        # Adapters that do not support the DEFAULT VALUES syntax for
        # INSERT statements should overwrite this to return false.
        def supports_default_values?
          true
        end

        def create_statement(repository, model, properties, identity_field)
          statement = "INSERT INTO #{quote_table_name(model.storage_name(repository.name))} "

          if supports_default_values? && properties.empty?
            statement << 'DEFAULT VALUES'
          else
            statement << <<-EOS.compress_lines
              (#{properties.map { |p| quote_column_name(p.field(repository.name)) } * ', '})
              VALUES
              (#{(['?'] * properties.size) * ', '})
            EOS
          end

          if supports_returning? && identity_field
            statement << " RETURNING #{quote_column_name(identity_field.field(repository.name))}"
          end

          statement
        end

        def read_statement(query)
          statement = "SELECT #{fields_statement(query)}"
          statement << " FROM #{quote_table_name(query.model.storage_name(query.repository.name))}"
          statement << links_statement(query)                        if query.links.any?
          statement << " WHERE #{conditions_statement(query)}"       if query.conditions.any?
          statement << " GROUP BY #{group_by_statement(query)}"      if query.unique? && query.fields.any? { |p| p.kind_of?(Property) }
          statement << " ORDER BY #{order_statement(query)}"         if query.order.any?
          statement << " LIMIT #{quote_column_value(query.limit)}"   if query.limit
          statement << " OFFSET #{quote_column_value(query.offset)}" if query.offset && query.offset > 0
          statement
        rescue => e
          DataMapper.logger.error("QUERY INVALID: #{query.inspect} (#{e})")
          raise e
        end

        def update_statement(properties, query)
          statement = "UPDATE #{quote_table_name(query.model.storage_name(query.repository.name))}"
          statement << " SET #{set_statement(query.repository, properties)}"
          statement << " WHERE #{conditions_statement(query)}" if query.conditions.any?
          statement
        end

        def set_statement(repository, properties)
          properties.map { |p| "#{quote_column_name(p.field(repository.name))} = ?" } * ', '
        end

        def delete_statement(query)
          statement = "DELETE FROM #{quote_table_name(query.model.storage_name(query.repository.name))}"
          statement << " WHERE #{conditions_statement(query)}" if query.conditions.any?
          statement
        end

        def fields_statement(query)
          qualify = query.links.any?
          query.fields.map { |p| property_to_column_name(query.repository, p, qualify) } * ', '
        end

        def links_statement(query)
          table_list = [query.model.storage_name(query.repository.name)]

          statement = ''
          query.links.each do |relationship|
            parent_table_name = relationship.parent_model.storage_name(query.repository.name)
            child_table_name  = relationship.child_model.storage_name(query.repository.name)

            join_table_name = if table_list.include?(parent_table_name)
              child_table_name
            elsif table_list.include?(child_table_name)
              parent_table_name
            else
              raise ArgumentError, 'you\'re trying to join a table with no connection to this query'
            end
            table_list << join_table_name

            # We only do INNER JOIN for now
            statement << " INNER JOIN #{quote_table_name(join_table_name)} ON "

            statement << relationship.parent_key.zip(relationship.child_key).map do |parent_property,child_property|
              condition_statement(query, :eql, parent_property, child_property)
            end * ' AND '
          end

          statement
        end

        def conditions_statement(query)
          query.conditions.map { |o,p,b| condition_statement(query, o, p, b) } * ' AND '
        end

        def group_by_statement(query)
          repository = query.repository
          qualify    = query.links.any?
          query.fields.select { |p| p.kind_of?(Property) }.map { |p| property_to_column_name(repository, p, qualify) } * ', '
        end

        def order_statement(query)
          repository = query.repository
          qualify    = query.links.any?
          query.order.map { |i| order_column(repository, i, qualify) } * ', '
        end

        def order_column(repository, item, qualify)
          property, descending = nil, false

          case item
            when Property
              property = item
            when Query::Direction
              property  = item.property
              descending = true if item.direction == :desc
          end

          order_column = property_to_column_name(repository, property, qualify)
          order_column << ' DESC' if descending
          order_column
        end

        def condition_statement(query, operator, left_condition, right_condition)
          return left_condition if operator == :raw

          qualify = query.links.any?

          conditions = [ left_condition, right_condition ].map do |condition|
            if condition.kind_of?(Property) || condition.kind_of?(Query::Path)
              property_to_column_name(query.repository, condition, qualify)
            elsif condition.kind_of?(Query)
              opposite = condition == left_condition ? right_condition : left_condition
              query.merge_subquery(operator, opposite, condition)
              "(#{read_statement(condition)})"

            # [].all? is always true
            elsif condition.kind_of?(Array) && condition.any? && condition.all? { |p| p.kind_of?(Property) }
              property_values = condition.map { |p| property_to_column_name(query.repository, p, qualify) }
              "(#{property_values * ', '})"
            else
              '?'
            end
          end

          comparison = case operator
            when :eql, :in then equality_operator(right_condition)
            when :not      then inequality_operator(right_condition)
            when :like     then 'LIKE'
            when :gt       then '>'
            when :gte      then '>='
            when :lt       then '<'
            when :lte      then '<='
            else raise "Invalid query operator: #{operator.inspect}"
          end

          "(" + (conditions * " #{comparison} ") + ")"
        end

        def equality_operator(operand)
          case operand
            when Array, Query then 'IN'
            when Range        then 'BETWEEN'
            when NilClass     then 'IS'
            else                   '='
          end
        end

        def inequality_operator(operand)
          case operand
            when Array, Query then 'NOT IN'
            when Range        then 'NOT BETWEEN'
            when NilClass     then 'IS NOT'
            else                   '<>'
          end
        end

        def property_to_column_name(repository, property, qualify)
          table_name = property.model.storage_name(repository.name) if property && property.respond_to?(:model)

          if table_name && qualify
            "#{quote_table_name(table_name)}.#{quote_column_name(property.field(repository.name))}"
          else
            quote_column_name(property.field(repository.name))
          end
        end

        # TODO: once the driver's quoting methods become public, have
        # this method delegate to them instead
        def quote_table_name(table_name)
          table_name.gsub('"', '""').split('.').map { |part| "\"#{part}\"" } * '.'
        end

        # TODO: once the driver's quoting methods become public, have
        # this method delegate to them instead
        def quote_column_name(column_name)
          "\"#{column_name.gsub('"', '""')}\""
        end

        # TODO: once the driver's quoting methods become public, have
        # this method delegate to them instead
        def quote_column_value(column_value)
          return 'NULL' if column_value.nil?

          case column_value
            when String
              if (integer = column_value.to_i).to_s == column_value
                quote_column_value(integer)
              elsif (float = column_value.to_f).to_s == column_value
                quote_column_value(integer)
              else
                "'#{column_value.gsub("'", "''")}'"
              end
            when DateTime
              quote_column_value(column_value.strftime('%Y-%m-%d %H:%M:%S'))
            when Date
              quote_column_value(column_value.strftime('%Y-%m-%d'))
            when Time
              quote_column_value(column_value.strftime('%Y-%m-%d %H:%M:%S') + ((column_value.usec > 0 ? ".#{column_value.usec.to_s.rjust(6, '0')}" : '')))
            when Integer, Float
              column_value.to_s
            when BigDecimal
              column_value.to_s('F')
            else
              column_value.to_s
          end
        end
      end #module SQL

      include SQL

      # TODO: move to dm-more/dm-migrations
      module Migration
        # TODO: move to dm-more/dm-migrations
        def upgrade_model_storage(repository, model)
          table_name = model.storage_name(repository.name)

          if success = create_model_storage(repository, model)
            return model.properties(repository.name)
          end

          properties = []

          model.properties(repository.name).each do |property|
            schema_hash = property_schema_hash(repository, property)
            next if field_exists?(table_name, schema_hash[:name])
            statement = alter_table_add_column_statement(table_name, schema_hash)
            execute(statement)
            properties << property
          end

          properties
        end

        # TODO: move to dm-more/dm-migrations
        def create_model_storage(repository, model)
          return false if storage_exists?(model.storage_name(repository.name))

          execute(create_table_statement(repository, model))

          (create_index_statements(repository, model) + create_unique_index_statements(repository, model)).each do |sql|
            execute(sql)
          end

          true
        end

        # TODO: move to dm-more/dm-migrations
        def destroy_model_storage(repository, model)
          execute(drop_table_statement(repository, model))
          true
        end

        # TODO: move to dm-more/dm-transactions
        def transaction_primitive
          DataObjects::Transaction.create_for_uri(@uri)
        end

        module SQL
          private

          # Adapters that support AUTO INCREMENT fields for CREATE TABLE
          # statements should overwrite this to return true
          #
          # TODO: move to dm-more/dm-migrations
          def supports_serial?
            false
          end

          # TODO: move to dm-more/dm-migrations
          def alter_table_add_column_statement(table_name, schema_hash)
            "ALTER TABLE #{quote_table_name(table_name)} ADD COLUMN #{property_schema_statement(schema_hash)}"
          end

          # TODO: move to dm-more/dm-migrations
          def create_table_statement(repository, model)
            repository_name = repository.name

            statement = <<-EOS.compress_lines
              CREATE TABLE #{quote_table_name(model.storage_name(repository_name))}
              (#{model.properties_with_subclasses(repository_name).map { |p| property_schema_statement(property_schema_hash(repository, p)) } * ', '}
            EOS

            if (key = model.key(repository_name)).any?
              statement << ", PRIMARY KEY(#{ key.map { |p| quote_column_name(p.field(repository_name)) } * ', '})"
            end

            statement << ')'
            statement
          end

          # TODO: move to dm-more/dm-migrations
          def drop_table_statement(repository, model)
            "DROP TABLE IF EXISTS #{quote_table_name(model.storage_name(repository.name))}"
          end

          # TODO: move to dm-more/dm-migrations
          def create_index_statements(repository, model)
            table_name = model.storage_name(repository.name)
            model.properties(repository.name).indexes.map do |index_name, fields|
              <<-EOS.compress_lines
                CREATE INDEX #{quote_column_name("index_#{table_name}_#{index_name}")} ON
                #{quote_table_name(table_name)} (#{fields.map { |f| quote_column_name(f) } * ', '})
              EOS
            end
          end

          # TODO: move to dm-more/dm-migrations
          def create_unique_index_statements(repository, model)
            table_name = model.storage_name(repository.name)
            model.properties(repository.name).unique_indexes.map do |index_name, fields|
              <<-EOS.compress_lines
                CREATE UNIQUE INDEX #{quote_column_name("unique_index_#{table_name}_#{index_name}")} ON
                #{quote_table_name(table_name)} (#{fields.map { |f| quote_column_name(f) } * ', '})
              EOS
            end
          end

          # TODO: move to dm-more/dm-migrations
          def property_schema_hash(repository, property)
            schema = self.class.type_map[property.type].merge(:name => property.field(repository.name))
            # TODO: figure out a way to specify the size not be included, even if
            # a default is defined in the typemap
            #  - use this to make it so all TEXT primitive fields do not have size
            if property.primitive == String && schema[:primitive] != 'TEXT'
              schema[:size] = property.length
            elsif property.primitive == BigDecimal || property.primitive == Float
              schema[:precision] = property.precision
              schema[:scale]     = property.scale
            end

            schema[:nullable?] = property.nullable?
            schema[:serial?]   = property.serial?

            if property.default.nil? || property.default.respond_to?(:call)
              # remove the default if the property is not nullable
              schema.delete(:default) unless property.nullable?
            else
              if property.type.respond_to?(:dump)
                schema[:default] = property.type.dump(property.default, property)
              else
                schema[:default] = property.default
              end
            end

            schema
          end

          # TODO: move to dm-more/dm-migrations
          def property_schema_statement(schema)
            statement = quote_column_name(schema[:name])
            statement << " #{schema[:primitive]}"

            if schema[:precision] && schema[:scale]
              statement << "(#{[ :precision, :scale ].map { |k| quote_column_value(schema[k]) } * ','})"
            elsif schema[:size]
              statement << "(#{quote_column_value(schema[:size])})"
            end

            statement << ' NOT NULL' unless schema[:nullable?]
            statement << " DEFAULT #{quote_column_value(schema[:default])}" if schema.has_key?(:default)
            statement
          end

          # TODO: move to dm-more/dm-migrations
          def relationship_schema_hash(relationship)
            identifier, relationship = relationship

            self.class.type_map[Integer].merge(:name => "#{identifier}_id") if identifier == relationship.name
          end

          # TODO: move to dm-more/dm-migrations
          def relationship_schema_statement(hash)
            property_schema_statement(hash) unless hash.nil?
          end
        end # module SQL

        include SQL

        module ClassMethods
          # Default TypeMap for all data object based adapters.
          #
          # @return <DataMapper::TypeMap> default TypeMap for data objects adapters.
          #
          # TODO: move to dm-more/dm-migrations
          def type_map
            @type_map ||= TypeMap.new(super) do |tm|
              tm.map(Integer).to('INT')
              tm.map(String).to('VARCHAR').with(:size => Property::DEFAULT_LENGTH)
              tm.map(Class).to('VARCHAR').with(:size => Property::DEFAULT_LENGTH)
              tm.map(DM::Discriminator).to('VARCHAR').with(:size => Property::DEFAULT_LENGTH)
              tm.map(BigDecimal).to('DECIMAL').with(:precision => Property::DEFAULT_PRECISION, :scale => Property::DEFAULT_SCALE_BIGDECIMAL)
              tm.map(Float).to('FLOAT').with(:precision => Property::DEFAULT_PRECISION)
              tm.map(DateTime).to('DATETIME')
              tm.map(Date).to('DATE')
              tm.map(Time).to('TIMESTAMP')
              tm.map(TrueClass).to('BOOLEAN')
              tm.map(DM::Object).to('TEXT')
              tm.map(DM::Text).to('TEXT')
            end
          end
        end # module ClassMethods
      end # module Migration

      include Migration
      extend Migration::ClassMethods
    end # class DataObjectsAdapter
  end # module Adapters

  # TODO: move to dm-ar-finders
  module Model
    #
    # Find instances by manually providing SQL
    #
    # @param sql<String>   an SQL query to execute
    # @param <Array>    an Array containing a String (being the SQL query to
    #   execute) and the parameters to the query.
    #   example: ["SELECT name FROM users WHERE id = ?", id]
    # @param query<DataMapper::Query>  a prepared Query to execute.
    # @param opts<Hash>     an options hash.
    #     :repository<Symbol> the name of the repository to execute the query
    #       in. Defaults to self.default_repository_name.
    #     :reload<Boolean>   whether to reload any instances found that already
    #      exist in the identity map. Defaults to false.
    #     :properties<Array>  the Properties of the instance that the query
    #       loads. Must contain DataMapper::Properties.
    #       Defaults to self.properties.
    #
    # @note
    #   A String, Array or Query is required.
    # @return <Collection> the instance matched by the query.
    #
    # @example
    #   MyClass.find_by_sql(["SELECT id FROM my_classes WHERE county = ?",
    #     selected_county], :properties => MyClass.property[:id],
    #     :repository => :county_repo)
    #
    # -
    # @api public
    def find_by_sql(*args)
      sql = nil
      query = nil
      bind_values = []
      properties = nil
      do_reload = false
      repository_name = default_repository_name
      args.each do |arg|
        if arg.is_a?(String)
          sql = arg
        elsif arg.is_a?(Array)
          sql = arg.first
          bind_values = arg[1..-1]
        elsif arg.is_a?(DataMapper::Query)
          query = arg
        elsif arg.is_a?(Hash)
          repository_name = arg.delete(:repository) if arg.include?(:repository)
          properties = Array(arg.delete(:properties)) if arg.include?(:properties)
          do_reload = arg.delete(:reload) if arg.include?(:reload)
          raise "unknown options to #find_by_sql: #{arg.inspect}" unless arg.empty?
        end
      end

      repository = repository(repository_name)
      raise "#find_by_sql only available for Repositories served by a DataObjectsAdapter" unless repository.adapter.is_a?(DataMapper::Adapters::DataObjectsAdapter)

      if query
        sql = repository.adapter.send(:read_statement, query)
        bind_values = query.bind_values
      end

      raise "#find_by_sql requires a query of some kind to work" unless sql

      properties ||= self.properties(repository.name)

      Collection.new(Query.new(repository, self)) do |collection|
        repository.adapter.send(:with_connection) do |connection|
          command = connection.create_command(sql)

          begin
            reader = command.execute_reader(*bind_values)

            while(reader.next!)
              collection.load(reader.values)
            end
          ensure
            reader.close if reader
          end
        end
      end
    end
  end # module Model
end # module DataMapper
