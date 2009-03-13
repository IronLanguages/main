# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-rest-adapter}
  s.version = "0.9.12"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Scott Burton", "Andres Rodriguez"]
  s.date = %q{2009-02-11}
  s.description = %q{A RESTful adapter for DataMapper.}
  s.email = %q{scott.burton@joyent.com}
  s.files = ["History.txt", "LICENSE", "Manifest.txt", "README.txt", "Rakefile", "TODO", "lib/rest_adapter.rb", "lib/rest_adapter/version.rb", "lib/rest_adapter/adapter.rb", "lib/rest_adapter/connection.rb", "lib/rest_adapter/exceptions.rb", "lib/rest_adapter/formats.rb", "spec/crud_spec.rb", "spec/connection_spec.rb", "spec/ruby_forker.rb", "spec/spec.opts", "spec/spec_helper.rb", "stories/all.rb", "stories/crud/create", "stories/crud/delete", "stories/crud/read", "stories/crud/stories.rb", "stories/crud/update", "stories/helper.rb", "stories/resources/helpers/book.rb", "stories/resources/helpers/story_helper.rb", "stories/resources/steps/read.rb", "stories/resources/steps/using_rest_adapter.rb", "tasks/install.rb", "tasks/spec.rb", "tasks/stories.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://github.com/spurton/dm-more/tree/92ae43c4fc434ed169e8199a7037a0a675dbfa96/adapters/dm-rest-adapter}
  s.rdoc_options = ["--inline-source", "--charset=UTF-8"]
  s.require_paths = ["lib"]
  s.summary = %q{dm-rest-adapter is a DataMapper adapter which allows you to remotely use REST resources across applications.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, [">= 0.9.10"])
    else
      s.add_dependency(%q<dm-core>, [">= 0.9.10"])
    end
  else
    s.add_dependency(%q<dm-core>, [">= 0.9.10"])
  end
end
