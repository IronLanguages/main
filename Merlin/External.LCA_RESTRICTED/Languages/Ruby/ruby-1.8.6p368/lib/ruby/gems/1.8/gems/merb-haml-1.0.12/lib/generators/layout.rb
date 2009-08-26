Merb::Generators::LayoutGenerator.template :layout_haml, :template_engine => :haml do |t|
  t.source = File.join(File.dirname(__FILE__), 'templates/layout/app/views/layout/%file_name%.html.haml')
  t.destination = "app/views/layout/#{file_name}.html.haml"
end
