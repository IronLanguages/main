class Merb::Generators::ResourceControllerGenerator
  # TODO: fix this for Datamapper, so that it returns the primary keys for the model
  def params_for_get
    "params[:id]"
  end

  # TODO: implement this for Datamapper so that we get the model properties
  def properties
    []
  end
end


Merb::Generators::ResourceControllerGenerator.template :controller_datamapper, :orm => :datamapper do |t|
  t.source = File.join(File.dirname(__FILE__), "templates/resource_controller.rb")
  t.destination = File.join("app/controllers", base_path, "#{file_name}.rb")
  
  self.add_resource_route(self.plural_model)
end

[:index, :show, :edit, :new].each do |view|
  Merb::Generators::ResourceControllerGenerator.template "view_#{view}_datamapper".to_sym,
      :orm => :datamapper, :template_engine => :erb do |t|
    t.source = File.join(File.dirname(__FILE__), "templates/views/#{view}.html.erb")
    t.destination = File.join("app/views", base_path, "#{file_name}/#{view}.html.erb")
  end
end
