module Gem
  def self.default_exec_format
    exec_format = ConfigMap[:ruby_install_name].sub('ir', '%s') rescue '%s'

    unless exec_format =~ /%s/ then
      raise Gem::Exception,
        "[BUG] invalid exec_format #{exec_format.inspect}, no %s"
    end

    exec_format
  end
end
