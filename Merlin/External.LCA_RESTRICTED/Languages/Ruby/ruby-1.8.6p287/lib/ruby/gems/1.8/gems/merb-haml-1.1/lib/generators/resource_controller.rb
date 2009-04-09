[:activerecord, :datamapper, :sequel, :none].each do |orm|
  
  [:show, :index, :edit, :new].each do |view|
  
    Merb::Generators::ResourceControllerGenerator.template "view_#{view}_haml", :orm => orm, :template_engine => :haml do |t|
      t.source = File.join(File.dirname(__FILE__), "templates/resource_controller/#{orm}/app/views/%file_name%/#{view}.html.haml")
      t.destination = File.join("app/views", base_path, "#{file_name}/#{view}.html.haml")
    end

  end

end
