require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe "DataMapper::Is" do
  describe ".is" do

    module ::DataMapper

      module Is
        module Example

          def is_example(*args)
            @args = args

            extend DataMapper::Is::Example::ClassMethods
          end

          def is_example_args
            @args
          end

          module ClassMethods
            def example_class_method

            end
          end

        end
      end

      module Model
        include DataMapper::Is::Example
      end # module Model
    end # module DataMapper

    class ::House
      include DataMapper::Resource
    end

    class ::Cabin
      include DataMapper::Resource
    end

    it "should raise error unless it finds the plugin" do
      lambda do
        class ::House
          is :no_plugin_by_this_name
        end
      end.should raise_error(DataMapper::PluginNotFoundError)
    end

    it "should call plugin is_* method" do
      lambda do
        class ::House
          is :example
        end
      end.should_not raise_error
    end

    it "should pass through arguments to plugin is_* method" do
      class ::House
        is :example ,:option1 => :ping, :option2 => :pong
      end

      House.is_example_args.length.should == 1
      House.is_example_args.first[:option2].should == :pong
    end

    it "should not add class_methods before the plugin is activated" do
      Cabin.respond_to?(:example_class_method).should be_false

      class ::Cabin
        is :example
      end

      Cabin.respond_to?(:example_class_method).should be_true

    end

  end
end
