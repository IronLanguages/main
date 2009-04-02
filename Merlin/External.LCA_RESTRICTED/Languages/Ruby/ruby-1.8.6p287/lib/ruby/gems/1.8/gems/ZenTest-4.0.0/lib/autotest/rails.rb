require 'autotest'

class Autotest::Rails < Autotest

  def initialize # :nodoc:
    super

    add_exception %r%^\./(?:db|doc|log|public|script|tmp|vendor)%

    clear_mappings

    self.add_mapping(/^lib\/.*\.rb$/) do |filename, _|
      impl = File.basename(filename, '.rb')
      files_matching %r%^test/unit/#{impl}_test.rb$%
      # TODO: (unit|functional|integration) maybe?
    end

    add_mapping %r%^test/fixtures/(.*)s.yml% do |_, m|
      ["test/unit/#{m[1]}_test.rb",
       "test/controllers/#{m[1]}_controller_test.rb",
       "test/views/#{m[1]}_view_test.rb",
       "test/functional/#{m[1]}_controller_test.rb"]
    end

    add_mapping %r%^test/(unit|integration|controllers|views|functional)/.*rb$% do |filename, _|
      filename
    end

    add_mapping %r%^app/models/(.*)\.rb$% do |_, m|
      "test/unit/#{m[1]}_test.rb"
    end

    add_mapping %r%^app/helpers/application_helper.rb% do
      files_matching %r%^test/(views|functional)/.*_test\.rb$%
    end

    add_mapping %r%^app/helpers/(.*)_helper.rb% do |_, m|
      if m[1] == "application" then
        files_matching %r%^test/(views|functional)/.*_test\.rb$%
      else
        ["test/views/#{m[1]}_view_test.rb",
         "test/functional/#{m[1]}_controller_test.rb"]
      end
    end

    add_mapping %r%^app/views/(.*)/% do |_, m|
      ["test/views/#{m[1]}_view_test.rb",
       "test/functional/#{m[1]}_controller_test.rb"]
    end

    add_mapping %r%^app/controllers/(.*)\.rb$% do |_, m|
      if m[1] == "application" then
        files_matching %r%^test/(controllers|views|functional)/.*_test\.rb$%
      else
        ["test/controllers/#{m[1]}_test.rb",
         "test/functional/#{m[1]}_test.rb"]
      end
    end

    add_mapping %r%^app/views/layouts/% do
      "test/views/layouts_view_test.rb"
    end

    add_mapping %r%^config/routes.rb$% do # FIX:
      files_matching %r%^test/(controllers|views|functional)/.*_test\.rb$%
    end

    add_mapping %r%^test/test_helper.rb|config/((boot|environment(s/test)?).rb|database.yml)% do
      files_matching %r%^test/(unit|controllers|views|functional)/.*_test\.rb$%
    end
  end

  # Convert the pathname s to the name of class.
  def path_to_classname(s)
    sep = File::SEPARATOR
    f = s.sub(/^test#{sep}((unit|functional|integration|views|controllers|helpers)#{sep})?/, '').sub(/\.rb$/, '').split(sep)
    f = f.map { |path| path.split(/_/).map { |seg| seg.capitalize }.join }
    f = f.map { |path| path =~ /Test$/ ? path : "#{path}Test"  }
    f.join('::')
  end
end
