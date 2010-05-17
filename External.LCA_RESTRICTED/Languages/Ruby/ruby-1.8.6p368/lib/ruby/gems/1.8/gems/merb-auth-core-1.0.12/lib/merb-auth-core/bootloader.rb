# This is not intended to be modified.  It is for use with
# Merb::Authentication.default_customizations
class Merb::BootLoader::MerbAuthBootLoader < Merb::BootLoader
  before Merb::BootLoader::AfterAppLoads
  
  def self.run
    Merb::Authentication.default_customizations.each { |c| c.call }
  end
  
end