module DataMapper
  class Collection
    attr_accessor :total_rows
  end
end

module DataMapper
  module CouchResource

    def self.included(mod)

      mod.class_eval do
        include DataMapper::Resource
        include DataMapper::CouchResource::Attachments

        property :attachments, DataMapper::Types::JsonObject, :field => '_attachments'
        property :id, String, :key => true, :field => '_id', :nullable => true
        property :rev, String, :field => '_rev'
        property :couchdb_type, DataMapper::Types::Discriminator

        class << self

          def couchdb_types
            [self.base_model] | self.descendants
          end

          def couchdb_types_condition
            couchdb_types.collect {|type| "doc.couchdb_type == '#{type}'"}.join(' || ')
          end

          def view(name, &block)
            @views ||= Hash.new { |h,k| h[k] = {} }
            view = View.new(self, name)
            @views[repository.name][name] = block_given? ? block : lambda {}
            view
          end

          def views(repository_name = default_repository_name)
            @views ||= Hash.new { |h,k| h[k] = {} }
            views = @views[repository_name].dup
            views.each_pair {|key, value| views[key] = value.call}
          end

        end

      end
    end

  end
end
