module DataMapperRest
  # TODO: Abstract XML support out from the protocol
  # TODO: Build JSON support

  # All http_"verb" (http_post) method calls use method missing in connection class which uses run_verb
  class Adapter < DataMapper::Adapters::AbstractAdapter
    include Extlib

    def connection
      @connection ||= Connection.new(@uri, @format)
    end

    # Creates a new resource in the specified repository.
    # TODO: map all remote resource attributes to this resource
    def create(resources)
      created = 0
      resources.each do |resource|
        response = connection.http_post(resource_name(resource), resource.to_xml)
        populate_resource_from_xml(response.body, resource)

        created += 1
      end

      created
    end

    # read_set
    #
    # Examples of query string:
    # A. []
    #    GET /books/
    #
    # B. [[:eql, #<Property:Book:id>, 4200]]
    #    GET /books/4200
    #
    # IN PROGRESS
    # TODO: Need to account for query.conditions (i.e., [[:eql, #<Property:Book:id>, 1]] for books/1)
    def read_many(query)
      resource_name = Inflection.underscore(query.model.name)
      ::DataMapper::Collection.new(query) do |collection|
        case query.conditions
        when []
          resources_meta = read_set_all(repository, query, resource_name)
        else
          resources_meta = read_set_for_condition(repository, query, resource_name)
        end
        resources_meta.each do |resource_meta|
          if resource_meta.has_key?(:associations)
            load_nested_resources_from resource_meta[:associations], query
          end
          collection.load(resource_meta[:values])
        end
      end
    end

    def read_one(query)
      resource = nil
      resource_name = resource_name_from_query(query)
      resources_meta = nil
      if query.conditions.empty? && query.limit == 1
        results = read_set_all(repository, query, resource_name)
        resource_meta = results.first unless results.empty?
      else
        id = query.conditions.first[2]
        # KLUGE: Again, we're assuming below that we're dealing with a pluralized resource mapping

        response = connection.http_get("#{resource_name.pluralize}/#{id}")

        data = response.body
        resource_meta = parse_resource(data, query.model, query)
      end
      if resource_meta
        if resource_meta.has_key?(:associations)
          load_nested_resources_from resource_meta[:associations], query
        end
        resource = query.model.load(resource_meta[:values], query)
      end
      resource
    end

    def update(attributes, query)
      # TODO What if we have a compound key?
      raise NotImplementedError.new unless is_single_resource_query? query
      id = query.conditions.first[2]
      resource = nil
      query.repository.scope do
        resource = query.model.get(id)
      end
      attributes.each do |attr, val|
        resource.send("#{attr.name}=", val)
      end
      # KLUGE: Again, we're assuming below that we're dealing with a pluralized resource mapping
      res = connection.http_put("#{resource_name_from_query(query).pluralize}/#{id}", resource.to_xml)
      # TODO: Raise error if cannot reach server
      res.kind_of?(Net::HTTPSuccess) ? 1 : 0
    end

    def delete(query)
      raise NotImplementedError.new unless is_single_resource_query? query
      id = query.conditions.first[2]
      res = connection.http_delete("#{resource_name_from_query(query).pluralize}/#{id}")
      res.kind_of?(Net::HTTPSuccess) ? 1 : 0
    end

  protected

    def normalize_uri(uri_or_options)
      @format = uri_or_options[:format].nil? ? "xml" : uri_or_options[:format]

      if uri_or_options.kind_of?(String) || uri_or_options.kind_of?(Addressable::URI)
        uri_or_options = DataObjects::URI.parse(uri_or_options)
      end

      if uri_or_options.kind_of?(DataObjects::URI)
        return uri_or_options
      end

      query = uri_or_options.except(:adapter, :username, :password, :host, :port, :format, :login).map { |pair| pair.join('=') }.join('&')
      query = nil if query.blank? # not sure if the query is usable

      return DataObjects::URI.parse(Addressable::URI.new(
        :scheme       => "http",
        :adapter      => uri_or_options[:adapter].to_s,
        :user         => uri_or_options[:login],
        :password     => uri_or_options[:password],
        :host         => uri_or_options[:host],
        :port         => uri_or_options[:port]
      ))
    end

    def load_nested_resources_from(nested_resources, query)
      nested_resources.each do |resource_meta|
        # TODO: Houston, we have a problem.  Model#load expects a Query.  When we're nested, we don't have a query yet...
        #resource_meta[:model].load(resource_meta[:values])
        #if resource_meta.has_key? :associations
        #  load_nested_resources_from resource_meta, query
        #end
      end
    end

    def read_set_all(repository, query, resource_name)
      # TODO: how do we know whether the resource we're talking to is singular or plural?
      res = connection.http_get("#{resource_name.pluralize}")
      data = res.body
      parse_resources(data, query.model, query)
      # TODO: Raise error if cannot reach server
    end

    #    GET /books/4200
    def read_set_for_condition(repository, query, resource_name)
      # More complex conditions
      raise NotImplementedError.new
    end

    # query.conditions like [[:eql, #<Property:Book:id>, 4200]]
    def is_single_resource_query?(query)
      query.conditions.length == 1 && query.conditions.first.first == :eql && query.conditions.first[1].name == :id
    end

    def values_from_rexml(entity_element, dm_model_class)
      resource = {}
      resource[:values] = []
      entity_element.elements.each do |field_element|
        attribute = dm_model_class.properties(repository.name).find do |property|
          property.name.to_s == field_element.name.to_s.tr('-', '_')
        end
        if attribute
          resource[:values] << field_element.text
          next
        end
        association = dm_model_class.relationships.find do |name, dm_relationship|
          field_element.name.to_s == Inflection.pluralize(Inflection.underscore(dm_relationship.child_model.to_s))
        end
        if association
          field_element.each_element do |associated_element|
            model = association[1].child_model
            (resource[:associations] ||= []) << {
              :model => model,
              :value => values_from_rexml(associated_element, association[1].child_model)
            }
          end
        end
      end
      resource
    end

    def parse_resource(xml, dm_model_class, query = nil)
      doc = REXML::Document::new(xml)
      # TODO: handle singular resource case as well....
      entity_element = REXML::XPath.first(doc, "/#{resource_name_from_model(dm_model_class)}")
      return nil unless entity_element
      values_from_rexml(entity_element, dm_model_class)
    end

    def parse_resources(xml, dm_model_class, query = nil)
      doc = REXML::Document::new(xml)
      # # TODO: handle singular resource case as well....
      # array = XPath(doc, "/*[@type='array']")
      # if array
      #   parse_resources()
      # else
      resource_name = resource_name_from_model dm_model_class
      doc.elements.collect("#{resource_name.pluralize}/#{resource_name}") do |entity_element|
        values_from_rexml(entity_element, dm_model_class)
      end
    end

    def resource_name_from_model(model)
      Inflection.underscore(model.name)
    end

    def resource_name(resource)
      Inflection.underscore(resource.class.name).pluralize
    end

    def resource_name_from_query(query)
      resource_name_from_model(query.model)
    end

    def populate_resource_from_xml(xml, resource)
      doc = REXML::Document::new(xml)
      entity_element = REXML::XPath.first(doc, "/#{resource_name_from_model(resource.class)}")
      raise "No root element matching #{resource_name_from_model(resource.class)} in xml" unless entity_element

      entity_element.elements.each do |field_element|
        attribute = resource.class.properties(repository.name).find { |property| property.name.to_s == field_element.name.to_s.tr('-', '_') }
        resource.send("#{attribute.name.to_s}=", field_element.text) if attribute && !field_element.text.nil?
        # TODO: add association saving
      end
      resource
    end

    # TODO: this is a temporary hack to allow applications using models with dm-rest-adapter
    # together with models using other adapters
    module Migration
      #
      # Returns whether the storage_name exists.
      #
      # @param storage_name<String> a String defining the name of a storage,
      #   for example a table name.
      #
      # @return <Boolean> true if the storage exists
      #
      def storage_exists?(storage_name)
        true
      end

      #
      # Returns whether the field exists.
      #
      # @param storage_name<String> a String defining the name of a storage, for example a table name.
      # @param field_name<String> a String defining the name of a field, for example a column name.
      #
      # @return <Boolean> true if the field exists.
      #
      def field_exists?(storage_name, field_name)
        true
      end

      def upgrade_model_storage(repository, model)
        true
      end

      def create_model_storage(repository, model)
        true
      end

      def destroy_model_storage(repository, model)
        true
      end

      def alter_model_storage(repository, *args)
        true
      end

      def create_property_storage(repository, property)
        true
      end

      def destroy_property_storage(repository, property)
        true
      end

      def alter_property_storage(repository, *args)
        true
      end

    end
    include Migration
  end
end
