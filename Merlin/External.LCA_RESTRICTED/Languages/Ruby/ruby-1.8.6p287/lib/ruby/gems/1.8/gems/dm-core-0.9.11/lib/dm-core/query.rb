module DataMapper
  class Query
    include Assertions

    OPTIONS = [
      :reload, :offset, :limit, :order, :add_reversed, :fields, :links, :includes, :conditions, :unique
    ]

    attr_reader :repository, :model, *OPTIONS - [ :reload, :unique ]
    attr_writer :add_reversed
    alias add_reversed? add_reversed

    def reload?
      @reload
    end

    def unique?
      @unique
    end

    def reverse
      dup.reverse!
    end

    def reverse!
      # reverse the sort order
      update(:order => self.order.map { |o| o.reverse })

      self
    end

    def update(other)
      assert_kind_of 'other', other, self.class, Hash

      assert_valid_other(other)

      if other.kind_of?(Hash)
        return self if other.empty?
        other = self.class.new(@repository, model, other)
      end

      return self if self == other

      # TODO: update this so if "other" had a value explicitly set
      #       overwrite the attributes in self

      # only overwrite the attributes with non-default values
      @reload       = other.reload?       unless other.reload?       == false
      @unique       = other.unique?       unless other.unique?       == false
      @offset       = other.offset        if other.reload? || other.offset != 0
      @limit        = other.limit         unless other.limit         == nil
      @order        = other.order         unless other.order         == model.default_order
      @add_reversed = other.add_reversed? unless other.add_reversed? == false
      @fields       = other.fields        unless other.fields        == @properties.defaults
      @links        = other.links         unless other.links         == []
      @includes     = other.includes      unless other.includes      == []

      update_conditions(other)

      self
    end

    def merge(other)
      dup.update(other)
    end

    def ==(other)
      return true if super
      return false unless other.kind_of?(self.class)

      # TODO: add a #hash method, and then use it in the comparison, eg:
      #   return hash == other.hash
      @model        == other.model         &&
      @reload       == other.reload?       &&
      @unique       == other.unique?       &&
      @offset       == other.offset        &&
      @limit        == other.limit         &&
      @order        == other.order         &&  # order is significant, so do not sort this
      @add_reversed == other.add_reversed? &&
      @fields       == other.fields        &&  # TODO: sort this so even if the order is different, it is equal
      @links        == other.links         &&  # TODO: sort this so even if the order is different, it is equal
      @includes     == other.includes      &&  # TODO: sort this so even if the order is different, it is equal
      @conditions.sort_by { |c| c.at(0).hash + c.at(1).hash + c.at(2).hash } == other.conditions.sort_by { |c| c.at(0).hash + c.at(1).hash + c.at(2).hash }
    end

    alias eql? ==

    def bind_values
      bind_values = []
      conditions.each do |tuple|
        next if tuple.size == 2
        operator, property, bind_value = *tuple
        if :raw == operator
          bind_values.push(*bind_value)
        else
          bind_values << bind_value
        end
      end
      bind_values
    end

    def inheritance_property
      fields.detect { |property| property.type == DataMapper::Types::Discriminator }
    end

    def inheritance_property_index
      fields.index(inheritance_property)
    end

    # TODO: spec this
    def key_property_indexes(repository)
      if (key_property_indexes = model.key(repository.name).map { |property| fields.index(property) }).all?
        key_property_indexes
      end
    end

    # find the point in self.conditions where the sub select tuple is
    # located. Delete the tuple and add value.conditions. value must be a
    # <DM::Query>
    #
    def merge_subquery(operator, property, value)
      assert_kind_of 'value', value, self.class

      new_conditions = []
      conditions.each do |tuple|
        if tuple.at(0).to_s == operator.to_s && tuple.at(1) == property && tuple.at(2) == value
          value.conditions.each do |subquery_tuple|
            new_conditions << subquery_tuple
          end
        else
          new_conditions << tuple
        end
      end
      @conditions = new_conditions
    end

    def inspect
      attrs = [
        [ :repository, repository.name ],
        [ :model,      model           ],
        [ :fields,     fields          ],
        [ :links,      links           ],
        [ :conditions, conditions      ],
        [ :order,      order           ],
        [ :limit,      limit           ],
        [ :offset,     offset          ],
        [ :reload,     reload?         ],
        [ :unique,     unique?         ],
      ]

      "#<#{self.class.name} #{attrs.map { |(k,v)| "@#{k}=#{v.inspect}" } * ' '}>"
    end

    # TODO: add docs
    # @api public
    def to_hash
      hash = {
        :reload       => reload?,
        :unique       => unique?,
        :offset       => offset,
        :order        => order,
        :add_reversed => add_reversed?,
        :fields       => fields,
      }

      hash[:limit]    = limit    unless limit    == nil
      hash[:links]    = links    unless links    == []
      hash[:includes] = includes unless includes == []

      conditions  = {}
      raw_queries = []
      bind_values = []

      conditions.each do |condition|
        if condition[0] == :raw
          raw_queries << condition[1]
          bind_values << condition[2]
        else
          operator, property, bind_value = condition
          conditions[ Query::Operator.new(property, operator) ] = bind_value
        end
      end

      if raw_queries.any?
        hash[:conditions] = [ raw_queries.join(' ') ].concat(bind_values)
      end

      hash.update(conditions)
    end

    # TODO: add docs
    # @api private
    def _dump(*)
      Marshal.dump([ repository, model, to_hash ])
    end

    # TODO: add docs
    # @api private
    def self._load(marshalled)
      new(*Marshal.load(marshalled))
    end

    private

    def initialize(repository, model, options = {})
      assert_kind_of 'repository', repository, Repository
      assert_kind_of 'model',      model,      Model
      assert_kind_of 'options',    options,    Hash

      options.each_pair { |k,v| options[k] = v.call if v.is_a? Proc } if options.is_a? Hash

      assert_valid_options(options)

      @repository = repository
      @properties = model.properties(@repository.name)

      @model        = model                               # must be Class that includes DM::Resource
      @reload       = options.fetch :reload,       false  # must be true or false
      @unique       = options.fetch :unique,       false  # must be true or false
      @offset       = options.fetch :offset,       0      # must be an Integer greater than or equal to 0
      @limit        = options.fetch :limit,        nil    # must be an Integer greater than or equal to 1
      @order        = options.fetch :order,        model.default_order(@repository.name)   # must be an Array of Symbol, DM::Query::Direction or DM::Property
      @add_reversed = options.fetch :add_reversed, false  # must be true or false
      @fields       = options.fetch :fields,       @properties.defaults  # must be an Array of Symbol, String or DM::Property
      @links        = options.fetch :links,        []     # must be an Array of Tuples - Tuple [DM::Query,DM::Assoc::Relationship]
      @includes     = options.fetch :includes,     []     # must be an Array of DM::Query::Path
      @conditions   = []                                  # must be an Array of triplets (or pairs when passing in raw String queries)

      # normalize order and fields
      @order  = normalize_order(@order)
      @fields = normalize_fields(@fields)

      # XXX: should I validate that each property in @order corresponds
      # to something in @fields?  Many DB engines require they match,
      # and I can think of no valid queries where a field would be so
      # important that you sort on it, but not important enough to
      # return.

      # normalize links and includes.
      # NOTE: this must be done after order and fields
      @links    = normalize_links(@links)
      @includes = normalize_includes(@includes)

      # treat all non-options as conditions
      (options.keys - OPTIONS).each do |k|
        append_condition(k, options[k])
      end

      # parse raw options[:conditions] differently
      if conditions = options[:conditions]
        if conditions.kind_of?(Hash)
          conditions.each do |k,v|
            append_condition(k, v)
          end
        elsif conditions.kind_of?(Array)
          raw_query, *bind_values = conditions
          @conditions << if bind_values.empty?
            [ :raw, raw_query ]
          else
            [ :raw, raw_query, bind_values ]
          end
        end
      end
    end

    def initialize_copy(original)
      # deep-copy the condition tuples when copying the object
      @conditions = original.conditions.map { |tuple| tuple.dup }
    end

    # validate the options
    def assert_valid_options(options)
      # [DB] This might look more ugly now, but it's 2x as fast as the old code
      # [DB] This is one of the heavy spots for Query.new I found during profiling.
      options.each_pair do |attribute, value|

        # validate the reload option and unique option
        if [:reload, :unique].include? attribute
          if value != true && value != false
            raise ArgumentError, "+options[:#{attribute}]+ must be true or false, but was #{value.inspect}", caller(2)
          end

        # validate the offset and limit options
        elsif [:offset, :limit].include? attribute
          assert_kind_of "options[:#{attribute}]", value, Integer
          if attribute == :offset && value < 0
            raise ArgumentError, "+options[:offset]+ must be greater than or equal to 0, but was #{value.inspect}", caller(2)
          elsif attribute == :limit && value < 1
            raise ArgumentError, "+options[:limit]+ must be greater than or equal to 1, but was #{options[:limit].inspect}", caller(2)
          end

        # validate the :order, :fields, :links and :includes options
        elsif [ :order, :fields, :links, :includes ].include? attribute
          assert_kind_of "options[:#{attribute}]", value, Array

          if value.empty?
            if attribute == :fields
              if options[:unique] == false
                raise ArgumentError, '+options[:fields]+ cannot be empty if +options[:unique] is false', caller(2)
              end
            elsif attribute == :order
              if options[:fields] && options[:fields].any? { |p| !p.kind_of?(Operator) }
                raise ArgumentError, '+options[:order]+ cannot be empty if +options[:fields] contains a non-operator', caller(2)
              end
            else
              raise ArgumentError, "+options[:#{attribute}]+ cannot be empty", caller(2)
            end
          end

        # validates the :conditions option
        elsif :conditions == attribute
          assert_kind_of 'options[:conditions]', value, Hash, Array

          if value.empty?
            raise ArgumentError, '+options[:conditions]+ cannot be empty', caller(2)
          end
        end
      end
    end

    # validate other DM::Query or Hash object
    def assert_valid_other(other)
      return unless  other.kind_of?(self.class)

      unless other.repository == repository
        raise ArgumentError, "+other+ #{self.class} must be for the #{repository.name} repository, not #{other.repository.name}", caller(2)
      end

      unless other.model == model
        raise ArgumentError, "+other+ #{self.class} must be for the #{model.name} model, not #{other.model.name}", caller(2)
      end
    end

    # normalize order elements to DM::Query::Direction
    def normalize_order(order)
      order.map do |order_by|
        case order_by
          when Direction
            # NOTE: The property is available via order_by.property
            # TODO: if the Property's model doesn't match
            # self.model, append the property's model to @links
            # eg:
            #if property.model != self.model
            #  @links << discover_path_for_property(property)
            #end

            order_by
          when Property
            # TODO: if the Property's model doesn't match
            # self.model, append the property's model to @links
            # eg:
            #if property.model != self.model
            #  @links << discover_path_for_property(property)
            #end

            Direction.new(order_by)
          when Operator
            property = @properties[order_by.target]
            Direction.new(property, order_by.operator)
          when Symbol, String
            property = @properties[order_by]

            if property.nil?
              raise ArgumentError, "+options[:order]+ entry #{order_by} does not map to a DataMapper::Property", caller(2)
            end

            Direction.new(property)
          else
            raise ArgumentError, "+options[:order]+ entry #{order_by.inspect} not supported", caller(2)
        end
      end
    end

    # normalize fields to DM::Property
    def normalize_fields(fields)
      # TODO: return a PropertySet
      # TODO: raise an exception if the property is not available in the repository
      fields.map do |field|
        case field
          when Property, Operator
            # TODO: if the Property's model doesn't match
            # self.model, append the property's model to @links
            # eg:
            #if property.model != self.model
            #  @links << discover_path_for_property(property)
            #end
            field
          when Symbol, String
            property = @properties[field]

            if property.nil?
              raise ArgumentError, "+options[:fields]+ entry #{field} does not map to a DataMapper::Property", caller(2)
            end

            property
          else
            raise ArgumentError, "+options[:fields]+ entry #{field.inspect} not supported", caller(2)
        end
      end
    end

    # normalize links to DM::Query::Path
    def normalize_links(links)
      # XXX: this should normalize to DM::Query::Path, not DM::Association::Relationship
      # because a link may be more than one-hop-away from the source.  A DM::Query::Path
      # should include an Array of Relationship objects that trace the "path" between
      # the source and the target.
      links.map do |link|
        case link
          when Associations::Relationship
            link
          when Symbol, String
            link = link.to_sym if link.kind_of?(String)

            unless model.relationships(@repository.name).has_key?(link)
              raise ArgumentError, "+options[:links]+ entry #{link} does not map to a DataMapper::Associations::Relationship", caller(2)
            end

            model.relationships(@repository.name)[link]
          else
            raise ArgumentError, "+options[:links]+ entry #{link.inspect} not supported", caller(2)
        end
      end
    end

    # normalize includes to DM::Query::Path
    def normalize_includes(includes)
      # TODO: normalize Array of Symbol, String, DM::Property 1-jump-away or DM::Query::Path
      # NOTE: :includes can only be and array of DM::Query::Path objects now. This method
      #       can go away after review of what has been done.
      includes
    end

    # validate that all the links or includes are present for the given DM::Query::Path
    #
    def validate_query_path_links(path)
      path.relationships.map do |relationship|
        @links << relationship unless (@links.include?(relationship) || @includes.include?(relationship))
      end
    end

    def append_condition(clause, bind_value)
      operator = :eql
      bind_value = bind_value.call if bind_value.is_a?(Proc)

      property = case clause
        when Property
          clause
        when Query::Path
          validate_query_path_links(clause)
          clause
        when Operator
          operator = clause.operator
          return if operator == :not && bind_value == []
          if clause.target.is_a?(Symbol)
            @properties[clause.target]
          elsif clause.target.is_a?(Query::Path)
            validate_query_path_links(clause.target)
            clause.target
          end
        when Symbol
          @properties[clause]
        when String
          if clause =~ /\w\.\w/
            query_path = @model
            clause.split(".").each { |piece| query_path = query_path.send(piece) }
            append_condition(query_path, bind_value)
            return
          else
            @properties[clause]
          end
        else
          raise ArgumentError, "Condition type #{clause.inspect} not supported", caller(2)
      end

      if property.nil?
        raise ArgumentError, "Clause #{clause.inspect} does not map to a DataMapper::Property", caller(2)
      end

      bind_value = dump_custom_value(property, bind_value)

      @conditions << [ operator, property, bind_value ]
    end

    def dump_custom_value(property_or_path, bind_value)
      case property_or_path
      when DataMapper::Query::Path
        dump_custom_value(property_or_path.property, bind_value)
      when Property
        if property_or_path.custom?
          property_or_path.type.dump(bind_value, property_or_path)
        else
          bind_value
        end
      else
        bind_value
      end
    end

    # TODO: check for other mutually exclusive operator + property
    # combinations.  For example if self's conditions were
    # [ :gt, :amount, 5 ] and the other's condition is [ :lt, :amount, 2 ]
    # there is a conflict.  When in conflict the other's conditions
    # overwrites self's conditions.

    # TODO: Another condition is when the other condition operator is
    # eql, this should over-write all the like,range and list operators
    # for the same property, since we are now looking for an exact match.
    # Vice versa, passing in eql should overwrite all of those operators.

    def update_conditions(other)
      @conditions = @conditions.dup

      # build an index of conditions by the property and operator to
      # avoid nested looping
      conditions_index = {}
      @conditions.each do |condition|
        operator, property = *condition
        next if :raw == operator
        conditions_index[property] ||= {}
        conditions_index[property][operator] = condition
      end

      # loop over each of the other's conditions, and overwrite the
      # conditions when in conflict
      other.conditions.each do |other_condition|
        other_operator, other_property, other_bind_value = *other_condition

        unless :raw == other_operator
          conditions_index[other_property] ||= {}
          if condition = conditions_index[other_property][other_operator]
            operator, property, bind_value = *condition

            next if bind_value == other_bind_value

            # overwrite the bind value in the existing condition
            condition[2] = case operator
              when :eql, :like then other_bind_value
              when :gt,  :gte  then [ bind_value, other_bind_value ].min
              when :lt,  :lte  then [ bind_value, other_bind_value ].max
              when :not, :in
                if bind_value.kind_of?(Array)
                  bind_value |= other_bind_value
                elsif other_bind_value.kind_of?(Array)
                  other_bind_value |= bind_value
                else
                  other_bind_value
                end
            end

            next  # process the next other condition
          end
        end

        # otherwise append the other condition
        @conditions << other_condition.dup
      end

      @conditions
    end

    class Direction
      include Assertions

      attr_reader :property, :direction

      def ==(other)
        return true if super
        hash == other.hash
      end

      alias eql? ==

      def hash
        @property.hash + @direction.hash
      end

      def reverse
        self.class.new(@property, @direction == :asc ? :desc : :asc)
      end

      def inspect
        "#<#{self.class.name} #{@property.inspect} #{@direction}>"
      end

      private

      def initialize(property, direction = :asc)
        assert_kind_of 'property',  property,  Property
        assert_kind_of 'direction', direction, Symbol

        @property  = property
        @direction = direction
      end
    end # class Direction

    class Operator
      include Assertions

      attr_reader :target, :operator

      def to_sym
        @property_name
      end

      def ==(other)
        return true if super
        return false unless other.kind_of?(self.class)
        @operator == other.operator && @target == other.target
      end

      private

      def initialize(target, operator)
        assert_kind_of 'operator', operator, Symbol

        @target   = target
        @operator = operator
      end
    end # class Operator

    class Path
      include Assertions

      instance_methods.each { |m| undef_method m if %w[ id type ].include?(m.to_s) }

      attr_reader :relationships, :model, :property, :operator

      [ :gt, :gte, :lt, :lte, :not, :eql, :like, :in ].each do |sym|
        class_eval <<-EOS, __FILE__, __LINE__
          def #{sym}
            Operator.new(self, :#{sym})
          end
        EOS
      end

      # duck type the DM::Query::Path to act like a DM::Property
      def field(*args)
        @property ? @property.field(*args) : nil
      end

      # more duck typing
      def to_sym
        @property ? @property.name.to_sym : @model.storage_name(@repository).to_sym
      end

      private

      def initialize(repository, relationships, model, property_name = nil)
        assert_kind_of 'repository',    repository,    Repository
        assert_kind_of 'relationships', relationships, Array
        assert_kind_of 'model',         model,         Model
        assert_kind_of 'property_name', property_name, Symbol unless property_name.nil?

        @repository    = repository
        @relationships = relationships
        @model         = model
        @property      = @model.properties(@repository.name)[property_name] if property_name
      end

      def method_missing(method, *args)
        if relationship = @model.relationships(@repository.name)[method]
          klass = klass = model == relationship.child_model ? relationship.parent_model : relationship.child_model
          return Query::Path.new(@repository, @relationships + [ relationship ], klass)
        end

        if @model.properties(@repository.name)[method]
          @property = @model.properties(@repository.name)[method] unless @property
          return self
        end

        raise NoMethodError, "undefined property or association `#{method}' on #{@model}"
      end
    end # class Path
  end # class Query
end # module DataMapper
