# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{dm-is-remixable}
  s.version = "0.9.11"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Cory O'Daniel"]
  s.date = %q{2009-03-10}
  s.description = %q{dm-is-remixable allow you to create reusable data functionality}
  s.email = ["dm-is-remixable [a] coryodaniel [d] com"]
  s.extra_rdoc_files = ["README.txt", "LICENSE", "TODO", "History.txt"]
  s.files = ["History.txt", "LICENSE", "Manifest.txt", "README.txt", "Rakefile", "TODO", "lib/dm-is-remixable.rb", "lib/dm-is-remixable/is/remixable.rb", "lib/dm-is-remixable/is/version.rb", "spec/data/addressable.rb", "spec/data/article.rb", "spec/data/billable.rb", "spec/data/bot.rb", "spec/data/commentable.rb", "spec/data/image.rb", "spec/data/rating.rb", "spec/data/tag.rb", "spec/data/taggable.rb", "spec/data/topic.rb", "spec/data/user.rb", "spec/data/viewable.rb", "spec/integration/remixable_spec.rb", "spec/spec.opts", "spec/spec_helper.rb", "tasks/install.rb", "tasks/spec.rb"]
  s.has_rdoc = true
  s.homepage = %q{http://github.com/sam/dm-more/tree/master/dm-is-remixable}
  s.rdoc_options = ["--main", "README.txt"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{datamapper}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{dm-is-remixable allow you to create reusable data functionality}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dm-core>, ["~> 0.9.11"])
    else
      s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
    end
  else
    s.add_dependency(%q<dm-core>, ["~> 0.9.11"])
  end
end
