require 'autotest/sqlserver'

class Autotest::Railssqlserver < Autotest::Sqlserver

  def initialize
    super
    self.libs << "#{File::PATH_SEPARATOR}../../../rails/activerecord/test/"
    self.extra_files = ['../../../rails/activerecord/test/']
    self.add_mapping %r%../../../rails/activerecord/test/.*/.*_test.rb$% do |filename, _|
      filename
    end
  end
  

end

