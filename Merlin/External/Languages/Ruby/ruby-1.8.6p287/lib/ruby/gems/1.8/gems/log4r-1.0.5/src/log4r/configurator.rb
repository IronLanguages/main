# :include: rdoc/configurator
#
# == Other Info
#
# Version:: $Id: configurator.rb,v 1.12 2004/03/17 19:13:07 fando Exp $

require "log4r/logger"
require "log4r/outputter/staticoutputter"
require "log4r/lib/xmlloader"
require "log4r/logserver"
require "log4r/outputter/remoteoutputter"

# TODO: catch unparsed parameters #{FOO} and die
module Log4r
  # Gets raised when Configurator encounters bad XML.
  class ConfigError < Exception
  end

  # See log4r/configurator.rb
  class Configurator
    include REXML if HAVE_REXML
    @@params = Hash.new

    # Get a parameter's value
    def self.[](param); @@params[param] end
    # Define a parameter with a value
    def self.[]=(param, value); @@params[param] = value end

    # Sets the custom levels. This method accepts symbols or strings.
    #
    #   Configurator.custom_levels('My', 'Custom', :Levels)
    #
    # Alternatively, you can specify custom levels in XML:
    # 
    #   <log4r_config>
    #     <pre_config>
    #       <custom_levels>
    #         My, Custom, Levels
    #       </custom_levels>
    #       ...

    def self.custom_levels(*levels)
      return Logger.root if levels.size == 0
      for i in 0...levels.size
        name = levels[i].to_s
        if name =~ /\s/ or name !~ /^[A-Z]/
          raise TypeError, "#{name} is not a valid Ruby Constant name", caller
        end
      end
      Log4r.define_levels *levels
    end

    # Given a filename, loads the XML configuration for Log4r.
    def self.load_xml_file(filename)
      detect_rexml
      actual_load Document.new(File.new(filename))
    end

    # You can load a String XML configuration instead of a file.
    def self.load_xml_string(string)
      detect_rexml
      actual_load Document.new(string)
    end

    #######
    private
    #######

    def self.detect_rexml
      unless HAVE_REXML
        raise LoadError,
        "Need REXML to load XML configuration", caller[1..-1]
      end
    end

    def self.actual_load(doc)
      confignode = doc.elements['//log4r_config']
      if confignode.nil?
        raise ConfigError, 
        "<log4r_config> element not defined", caller[1..-1]
      end
      decode_xml(confignode)
    end
    
    def self.decode_xml(doc)
      decode_pre_config(doc.elements['pre_config'])
      doc.elements.each('outputter') {|e| decode_outputter(e)}
      doc.elements.each('logger') {|e| decode_logger(e)}
      doc.elements.each('logserver') {|e| decode_logserver(e)}
    end

    def self.decode_pre_config(e)
      return Logger.root if e.nil?
      decode_custom_levels(e.elements['custom_levels'])
      global_config(e.elements['global'])
      global_config(e.elements['root'])
      decode_parameters(e.elements['parameters'])
      e.elements.each('parameter') {|p| decode_parameter(p)}
    end

    def self.decode_custom_levels(e)
      return Logger.root if e.nil? or e.text.nil?
      begin custom_levels *Log4rTools.comma_split(e.text)
      rescue TypeError => te
        raise ConfigError, te.message, caller[1..-4]
      end
    end
    
    def self.global_config(e)
      return if e.nil?
      globlev = e.value_of 'level'
      return if globlev.nil?
      lev = LNAMES.index(globlev)     # find value in LNAMES
      Log4rTools.validate_level(lev, 4)  # choke on bad level
      Logger.global.level = lev
    end

    def self.decode_parameters(e)
      e.elements.each{|p| @@params[p.name] = p.text} unless e.nil?
    end

    def self.decode_parameter(e)
      @@params[e.value_of('name')] = e.value_of 'value'
    end

    def self.decode_outputter(e)
      # fields
      name = e.value_of 'name'
      type = e.value_of 'type'
      level = e.value_of 'level'
      only_at = e.value_of 'only_at'
      # validation
      raise ConfigError, "Outputter missing name", caller[1..-3] if name.nil?
      raise ConfigError, "Outputter missing type", caller[1..-3] if type.nil?
      Log4rTools.validate_level(LNAMES.index(level)) unless level.nil?
      only_levels = []
      unless only_at.nil?
        for lev in Log4rTools.comma_split(only_at)
          alev = LNAMES.index(lev)
          Log4rTools.validate_level(alev, 3)
          only_levels.push alev
        end
      end
      formatter = decode_formatter(e.elements['formatter'])
      # build the eval string
      buff = "Outputter[name] = #{type}.new name"
      buff += ",:level=>#{LNAMES.index(level)}" unless level.nil?
      buff += ",:formatter=>formatter" unless formatter.nil?
      params = decode_hash_params(e)
      buff += "," + params.join(',') if params.size > 0
      begin eval buff
      rescue Exception => ae
        raise ConfigError, 
        "Problem creating outputter: #{ae.message}", caller[1..-3]
      end
      Outputter[name].only_at *only_levels if only_levels.size > 0
      Outputter[name]
    end

    def self.decode_formatter(e)
      return nil if e.nil?
      type = e.value_of 'type' 
      raise ConfigError, "Formatter missing type", caller[1..-4] if type.nil?
      buff = "#{type}.new " + decode_hash_params(e).join(',')
      begin return eval(buff)
      rescue Exception => ae
        raise ConfigError,
        "Problem creating outputter: #{ae.message}", caller[1..-4]
      end
    end

    ExcludeParams = %w{formatter level name type}

    # Does the fancy parameter to hash argument transformation
    def self.decode_hash_params(e)
      buff = []
      e.attributes.each_attribute {|p| 
        next if ExcludeParams.include? p.name
        buff << ":" + p.name + "=>" + paramsub(p.value)
      }
      e.elements.each {|p| 
        next if ExcludeParams.include? p.name
        buff << ":" + p.name + "=>" + paramsub(p.text)
      }
      buff
    end
    
    # Substitues any #{foo} in the XML with Parameter['foo']
    def self.paramsub(str)
      return nil if str.nil?
      @@params.each {|param, value| str.sub! '#{'+param+'}', value}
      "'" + str + "'"
    end

    def self.decode_logger(e)
      l = Logger.new e.value_of('name')
      decode_logger_common(l, e)
    end

    def self.decode_logserver(e)
      return unless HAVE_REXML
      name = e.value_of 'name'
      uri = e.value_of 'uri'
      l = LogServer.new name, uri
      decode_logger_common(l, e)
    end

    def self.decode_logger_common(l, e)
      level = e.value_of 'level'
      additive = e.value_of 'additive'
      trace = e.value_of 'trace'
      l.level = LNAMES.index(level) unless level.nil?
      l.additive = additive unless additive.nil?
      l.trace = trace unless trace.nil?
      # and now for outputters
      outs = e.value_of 'outputters'
      Log4rTools.comma_split(outs).each {|n| l.add n.strip} unless outs.nil?
      e.elements.each('outputter') {|e|
        name = (e.value_of 'name' or e.text)
        l.add Outputter[name]
      }
    end
  end
end
