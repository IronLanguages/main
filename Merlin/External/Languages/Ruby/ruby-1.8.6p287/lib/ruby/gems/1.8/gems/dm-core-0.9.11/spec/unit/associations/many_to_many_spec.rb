require File.expand_path(File.join(File.dirname(__FILE__), '..', '..', 'spec_helper'))

describe DataMapper::Associations::ManyToMany do

  load_models_for_metaphor :vehicles, :content

  it 'should allow a declaration' do
    lambda do
      class ::Supplier
        has n, :manufacturers, :through => Resource
      end
    end.should_not raise_error
  end

  it 'should handle models inside modules' do
    lambda do
      module ::Content
        class Dialect
          has n, :locales, :through => Resource, :class_name => "Language::Locale"
        end

        class Locale
          has n, :dialects, :through => Resource, :class_name => "Language::Dialect"
        end
      end
    end.should_not raise_error
  end

end

describe DataMapper::Associations::ManyToMany::Proxy do
end
