module Merb

  module Plugins

    # Returns the configuration settings hash for plugins. This is prepopulated from
    # Merb.root / "config/plugins.yml" if it is present.
    #
    # ==== Returns
    # Hash::
    #   The configuration loaded from Merb.root / "config/plugins.yml" or, if
    #   the load fails, an empty hash whose default value is another Hash.
    #
    # :api: plugin
    def self.config
      @config ||= begin
        # this is so you can do Merb.plugins.config[:helpers][:awesome] = "bar"
        config_hash = Hash.new {|h,k| h[k] = {}}
        file = Merb.root / "config" / "plugins.yml"

        if File.exists?(file)
          require 'yaml'
          to_merge = YAML.load_file(file)
        else
          to_merge = {}
        end
        
        config_hash.merge(to_merge)
      end
    end

    # ==== Returns
    # Array(String):: All Rakefile load paths Merb uses for plugins.
    #
    # :api: plugin
    def self.rakefiles
      Merb.rakefiles
    end
    
    # ==== Returns
    # Array(String):: All Generator load paths Merb uses for plugins.
    #
    # :api: plugin
    def self.generators
      Merb.generators
    end

    # ==== Parameters
    # *rakefiles:: Rakefiles to add to the list of plugin Rakefiles.
    #
    # ==== Notes
    #
    # This is a recommended way to register your plugin's Raketasks
    # in Merb.
    #
    # ==== Examples
    # From merb_sequel plugin:
    #
    # if defined(Merb::Plugins)
    #   Merb::Plugins.add_rakefiles "merb_sequel" / "merbtasks"
    # end
    #
    # :api: plugin
    def self.add_rakefiles(*rakefiles)
      Merb.add_rakefiles(*rakefiles)
    end
    
    # ==== Parameters
    # *generators:: Generator paths to add to the list of plugin generators.
    #
    # ==== Notes
    #
    # This is the recommended way to register your plugin's generators
    # in Merb.
    #
    # :api: plugin
    def self.add_generators(*generators)
      Merb.add_generators(*generators)
    end
  end
end
