# the mixin to provide the exceptions controller action for Unauthenticated
module MerbAuthSlicePassword::ExceptionsMixin
  def unauthenticated
    provides :xml, :js, :json, :yaml

    case content_type
    when :html
      render
    else
      basic_authentication.request!
      ""
    end
  end # unauthenticated
end

Merb::Authentication.customize_default do
  
  Exceptions.class_eval do
    include Merb::Slices::Support # Required to provide slice_url
  
    # # This stuff allows us to provide a default view
    the_view_path = File.expand_path(File.dirname(__FILE__) / ".." / "views")
    self._template_roots ||= []
    self._template_roots << [the_view_path, :_template_location]
    self._template_roots << [Merb.dir_for(:view), :_template_location]
    
    include MerbAuthSlicePassword::ExceptionsMixin
    
    show_action :unauthenticated

  end# Exceptions.class_eval
  
end # Customize default