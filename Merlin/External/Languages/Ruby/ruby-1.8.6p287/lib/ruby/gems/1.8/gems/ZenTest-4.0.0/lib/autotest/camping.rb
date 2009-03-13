require 'autotest'

##
# CampingAutotest is an Autotest subclass designed for use with Camping projects.
#
# To use CampingAutotest pass the -camping flag to autotest.
#
# Contributed by Geoffrey Grosenbach http://nubyonrails.com

class Autotest::Camping < Autotest

  def initialize # :nodoc:
    super
    @exceptions = %r%\.(log|db)$%

    @test_mappings = {
      %r%^test/fixtures/([^_]+)_.*s\.yml% => proc { |_, m|
        "test/#{m[1]}_test.rb"
      },
      %r%^test/.*rb$% => proc { |filename, m|
        filename
      },
      %r%^public/([^\/]+)/(models|controllers|views)\.rb$% => proc { |_, m|
        "test/#{m[1]}_test.rb"
      },
      %r%^public/(.*)\.rb$% => proc { |_, m|
        "test/#{m[1]}_test.rb"
      },
    }

    return functional_tests
  end

  def tests_for_file(filename)
    super.select { |f| @files.has_key? f }
  end
end
