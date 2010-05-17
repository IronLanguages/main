require 'merb-assets/assets'
require 'merb-assets/assets_mixin'

Merb::BootLoader.before_app_loads do
  Merb::Controller.send(:include, Merb::AssetsMixin)
end


Merb::Plugins.config[:asset_helpers] = {
    :max_hosts => 4,
    :asset_domain => "assets%s",
    :domain => "my-awesome-domain.com",
    :use_ssl => false
  } if Merb::Plugins.config[:asset_helpers].nil?