namespace :audit do

  desc "Print out the named and anonymous routes"
  task :routes => :merb_env do
    seen = []
    unless Merb::Router.named_routes.empty?
      puts "Named Routes"
      Merb::Router.named_routes.each do |name,route|
        puts "  #{name}: #{route}"
        seen << route
      end
    end
    puts "Anonymous Routes"
    (Merb::Router.routes - seen).each do |route|
      puts "  #{route}"
    end
    nil
  end

  desc "Print out all controllers"
  task :controllers => :merb_env do
    puts "\nControllers:\n\n"
    abstract_controller_classes.each do |klass|
      if klass.respond_to?(:subclasses_list)
        puts "#{klass} < #{klass.superclass}"
        subklasses = klass.subclasses_list.sort.map { |x| Object.full_const_get(x) }
        unless subklasses.empty?
          subklasses.each { |subklass| puts "- #{subklass}" }
        else
          puts "~ no subclasses"
        end
        puts
      end
    end
  end
  
  desc "Print out controllers and their actions (use CONTROLLER=Foo,Bar to be selective)"
  task :actions => :merb_env do
    puts "\nControllers and their actions:\n\n"
    filter_controllers = ENV['CONTROLLER'] ? ENV['CONTROLLER'].split(',') : nil
    abstract_controllers = abstract_controller_classes
    classes = Merb::AbstractController.subclasses_list.sort.map { |x| Object.full_const_get(x) }
    classes = classes.select { |k| k.name.in?(filter_controllers) } if filter_controllers
    classes.each do |subklass|
      next if subklass.in?(abstract_controllers) || !subklass.respond_to?(:callable_actions)
      puts "#{subklass} < #{subklass.superclass}"
      unless subklass.callable_actions.empty?
        subklass.callable_actions.sort.each do |action, null|
          if subklass.respond_to?(:action_argument_list)
            arguments, defaults = subklass.action_argument_list[action]
            args = arguments.map { |name, value| value ? "#{name} = #{value.inspect}" : name.to_s }.join(', ')
            puts args.empty? ? "- #{action}" : "- #{action}(#{args})"
          else
            puts "- #{action}"
          end
        end
      else
        puts "~ no callable actions"
      end
      puts
    end    
  end
  
  def abstract_controller_classes
    ObjectSpace.classes.select { |x| x.superclass == Merb::AbstractController }.sort_by { |x| x.name }
  end
  
end