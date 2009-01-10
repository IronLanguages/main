class Module
  def method_added name    
    puts "#{self.name}##{name}"
  end
end

require 'yaml'
p [1,2,3].to_yaml
