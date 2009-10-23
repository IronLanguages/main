load_assembly 'System.Xml'
include System::Xml

class WxsFile
  def initialize(file)
    @filename = file
    @doc = XmlDocument.new
    @doc.load(@filename)

    @ns = XmlNamespaceManager.new @doc.name_table
    @ns.add_namespace("wix", "http://schemas.microsoft.com/wix/2003/01/wi")
    @components = []
    @file_collisions = []
    @dir_collisions = []
  end

  def transform(feature)
    @feature = feature.gsub("Feature_", "")
    #require File.expand_path("~/desktop/repl"); repl binding
    f = @doc.document_element.select_single_node("wix:Fragment", @ns)
    root_dir= f.select_single_node "wix:DirectoryRef", @ns
    root_dir.set_attribute "Id", "INSTALLDIR"
    root_dir.set_attribute "FileSource", "."

    root_dir.select_nodes("wix:Directory", @ns).each do |dir|
      process_dir dir, ""
    end

    feature_ref = create_feature_ref(feature)
    f.insert_before(feature_ref, root_dir)
  end

  def save(filename)
    @doc.save(filename)
  end

  def process_dir(dir, parent_dir)
    name = dir.get_attribute("LongName") == '' ? dir.get_attribute("Name") : dir.get_attribute("LongName")
    full_name = parent_dir + "/" + name
    puts "Processing... ", full_name
    if $excluded.include?(full_name)
      dir.parent_node.remove_child(dir)
      return
    end

    name = name.gsub("-", "_")
    if @dir_collisions.include?(name)
      name += "_" + dir.get_attribute("Id")
    end
    dir.set_attribute("Id", "Dir_#{@feature}_#{name}")
    @dir_collisions << name

    child_nodes = []
    dir.child_nodes.each do |node|
      child_nodes << node
    end
    child_nodes.each do |node|
      if node.name == "Component"
        process_component(node, name, full_name)
      elsif node.name == "Directory"
        process_dir(node, full_name)
      end
    end
  end

  def process_component(component, name, parent_dir)
    component.set_attribute("Id", "Comp_#{@feature}_#{name}")
    component.set_attribute("Guid", System::Guid.new_guid().to_string)
    @components << name

    children = []
    component.child_nodes.each do |node|
      children << node
    end
    children.each do |node|
      if node.name == "File"
        process_file(node, name, parent_dir)
      end
    end
  end

  def process_file(file, comp_name, parent_dir)
    name = file.get_attribute("LongName") == '' ? file.get_attribute("Name") : file.get_attribute("LongName")
    
    full_name = parent_dir + "/" + name
    if $excluded.include?(full_name)
      file.parent_node.remove_child(file)
      return
    end

    name = name.gsub("-", "_")
    if @file_collisions.include? name
      name = comp_name + "_" + name
    end
    temp_name = name
    i = 1
    while @file_collisions.include? temp_name
      temp_name = name << i.to_s
      i+= 1
    end
    name = temp_name

    file.set_attribute("Id", "File_#{@feature}_#{name}")
    file.remove_attribute "src"
    @file_collisions << name
  end

  def create_feature_ref(feature_name)
    ref = @doc.create_element("FeatureRef")
    ref.set_attribute("Id", feature_name)

    feature = @doc.create_element("Feature")
    feature.set_attribute("Id", feature_name)
    feature.set_attribute("AllowAdvertise", "no")
    feature.set_attribute("Level", "1")
    feature.set_attribute("Title", $feature_title)
    feature.set_attribute("Description", $feature_description)

    @components.each do |comp_name|
      comp = @doc.create_element("ComponentRef")
      comp.set_attribute("Id", "Comp_#{@feature}_#{comp_name}")
      feature.append_child comp
    end

    ref.append_child feature
    ref
  end
end

if $0 == __FILE__
  if ARGV.length < 3
    puts "Usage: ir transform_wix.rb <input_file> <out> <feature_name>"
    exit -1
  end
  infile, out, feature_name = ARGV
  if feature_name == "Feature_Lib"
    $feature_title = "Standard Library"
    $feature_description = "Ruby Standard library"
  elsif feature_name == "Feature_Samples"
    $feature_title = "Samples"
    $feature_description = "Samples showing IronRuby working with various .NET apps and a Tutorial application"
  elsif feature_name == "Feature_Silverlight"
    $feature_title = "Silverlight binaries"
    $feature_description = "Binaries and scripts to do Silverlight development with IronRuby"
  else
    puts "Unknown feature_name: #{feature_name}"
    exit -1
  end
  $excluded ||= []

  wxs = WxsFile.new(infile)
  wxs.transform(feature_name)
  wxs.save(out)
end

