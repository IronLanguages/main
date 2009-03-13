load File.dirname(__FILE__) / "form" / "helpers.rb"
load File.dirname(__FILE__) / "form" / "builder.rb"

module Merb::GlobalHelpers
  include Merb::Helpers::Form
end

class Merb::AbstractController
  class_inheritable_accessor :_default_builder
end

Merb::BootLoader.after_app_loads do
  class Merb::AbstractController
    self._default_builder =
      Object.full_const_get(Merb::Plugins.config[:helpers][:default_builder]) rescue Merb::Helpers::Form::Builder::ResourcefulFormWithErrors
  end
end
