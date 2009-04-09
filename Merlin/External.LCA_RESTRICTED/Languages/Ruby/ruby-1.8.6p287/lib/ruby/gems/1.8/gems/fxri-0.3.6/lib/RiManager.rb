require "yaml"
require 'rdoc/ri/ri_driver'
require 'pp'

class NameDescriptor
  CLASS = 0
  INSTANCE_METHOD = 1
  CLASS_METHOD = 2
  def type
    @method_name ||= false
    @is_class_method ||= false
    if @method_name
      if @is_class_method
        CLASS_METHOD
      else
        INSTANCE_METHOD
      end
    else
      CLASS
    end
  end

  def to_s
    str = ""
    str << @class_names.join("::")
    if @method_name && str != ""
      str << (@is_class_method ? "::" : "#")
    end
    str << @method_name if @method_name
    str
  end
end

# This basically is a stripped down version of RiDriver.
# I cannot use RiDriver directly, because I need to set the driver.
class RiManager
  def initialize(display, path = RI::Paths::PATH)
    # See if search_paths exists, if so, append it to path
    begin 
      path << $cfg.search_paths.split(';')
    rescue
    end
    @reader = RI::RiReader.new(RI::RiCache.new(path))
    @display = display
    @display.reader = @reader
    # prepare all names
    @all_names = prepare_all_names
  end

  def prepare_all_names
    names = Array.new
    @reader.all_names.each do |name|
      begin
        names.push NameDescriptor.new(name)
      rescue RiError => e
        # silently ignore errors
      end
    end
    names
  end

  def check_names
    @reader.all_names.each do |name|
      begin
        if (NameDescriptor.new(name).to_s != name)
          p [name, NameDescriptor.new(name).to_s, NameDescriptor.new(name)]
        end
      rescue RiError => e
        puts e
      end
    end
  end

  # Returns all fully names as name descriptors
  def all_names
    @all_names
  end

  # Searches for the description of a name and shows it using +display+.
  def show(name_descriptor, width)
    @display.width = width
    # narrow down namespace
    namespace = @reader.top_level_namespace
    name_descriptor.class_names.each do |classname|
      namespace = @reader.lookup_namespace_in(classname, namespace)
      if namespace.empty?
        raise RiError.new("Nothing known about #{name_descriptor}")
      end
    end

    # At this point, if we have multiple possible namespaces, but one
    # is an exact match for our requested class, prune down to just it
    # PS: this comment is shamlessly stolen from ri_driver.rb
    entries = namespace.find_all {|m| m.full_name == name_descriptor.full_class_name}
    namespace = entries if entries.size == 1

    if name_descriptor.method_name
      methods = @reader.find_methods(name_descriptor.method_name, name_descriptor.is_class_method, namespace)
      report_method_stuff(name_descriptor.method_name, methods)
    else
      report_class_stuff(namespace)
    end
  end

  def report_class_stuff(namespace)
    raise RiError.new("namespace") unless namespace.size==1
    @display.display_class_info @reader.get_class(namespace[0])
  end

  def report_method_stuff(requested_method_name, methods)
    if methods.size == 1
      method = @reader.get_method(methods[0])
      @display.display_method_info(method)
    else
      entries = methods.find_all {|m| m.name == requested_method_name}
      if entries.size == 1
        method = @reader.get_method(entries[0])
        @display.display_method_info(method)
      else
        puts methods.map {|m| m.full_name}.join(", ")
      end
    end
=begin

    method = if (methods.size == 1)
      @reader.get_method(methods[0])
    else
      entries = methods.find_all {|m| m.name == requested_method_name}
      entries.size
      # there really should be just *one* method that matches.
      raise RiError.new("got a strange method") unless entries.size == 1
      @reader.get_method(entries[0])
    end
    @display.display_method_info(method)
=end
  end
end

if __FILE__ == $0
  display = Displayer.new
  ri = RiManager.new(display)
  ri.all_names.each do |name|
    p [name.type, name.to_s] if name.type==0
  end
end


=begin
# iterate through everything
reader.full_class_names.sort.each do |class_name|
  classDesc = reader.find_class_by_name(class_name)

  if class_name=="Integer"
    pp classDesc.instance_methods
    puts classDesc.to_yaml
    #puts classDesc.methods
    gets
  end
end
=end