class Merb::Generators::ModelGenerator
  ##
  # Corrects case of common datamapper arguments.
  def datamapper_type(type)
    return 'DateTime' if type == 'datetime'
    return type.camel_case
  end
  
  def after_generation
     STDOUT << message("Don't forget to define the model schema in your #{ Extlib::Inflection.camelize(file_name) } class")
  end
  
end

Merb::Generators::ModelGenerator.template :model_datamapper, :orm => :datamapper do |t|
  t.source = File.join(File.dirname(__FILE__), "templates", "model.rb")
  t.destination = File.join("app/models", base_path, "#{file_name}.rb")
end
