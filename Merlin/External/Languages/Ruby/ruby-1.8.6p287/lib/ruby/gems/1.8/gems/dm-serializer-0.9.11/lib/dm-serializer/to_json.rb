require 'dm-serializer/common'

begin
  gem('json')
  require 'json/ext'
rescue LoadError
  gem('json_pure')
  require 'json/pure'
end

module DataMapper
  module Serialize
    # Serialize a Resource to JavaScript Object Notation (JSON; RFC 4627)
    #
    # @return <String> a JSON representation of the Resource
    def to_json(*args)
      options = args.first || {}
      result = '{ '
      fields = []

      propset = properties_to_serialize(options)

      fields += propset.map do |property|
        "#{property.name.to_json}: #{send(property.getter).to_json}"
      end

      # add methods
      (options[:methods] || []).each do |meth|
        if self.respond_to?(meth)
          fields << "#{meth.to_json}: #{send(meth).to_json}"
        end
      end

      # Note: if you want to include a whole other model via relation, use :methods
      # comments.to_json(:relationships=>{:user=>{:include=>[:first_name],:methods=>[:age]}})
      # add relationships
      # TODO: This needs tests and also needs to be ported to #to_xml and #to_yaml
      (options[:relationships] || {}).each do |rel,opts|
        if self.respond_to?(rel)
          fields << "#{rel.to_json}: #{send(rel).to_json(opts)}"
        end
      end

      result << fields.join(', ')
      result << ' }'
      result
    end
  end

  module Associations
    # the json gem adds Object#to_json, which breaks the DM proxies, since it
    # happens *after* the proxy has been blank slated. This code removes the added
    # method, so it is delegated correctly to the Collection
    proxies = []

    proxies << ManyToMany::Proxy if defined?(ManyToMany::Proxy)
    proxies << OneToMany::Proxy  if defined?(OneToMany::Proxy)
    proxies << ManyToOne::Proxy  if defined?(ManyToOne::Proxy)

    proxies.each do |proxy|
      if proxy.public_instance_methods.any? { |m| m.to_sym == :to_json }
        proxy.send(:undef_method, :to_json)
      end
    end
  end

  class Collection
    def to_json(*args)
      opts = args.first || {}
      "[" << map {|e| e.to_json(opts)}.join(",") << "]"
    end
  end
end
