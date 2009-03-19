Merb::Generators::ControllerGenerator.template :index_haml, :template_engine => :haml do |t|
  t.source = File.join(File.dirname(__FILE__), 'templates/controller/app/views/%file_name%/index.html.haml')
  t.destination = File.join("app/views", base_path, "#{file_name}/index.html.haml")
end