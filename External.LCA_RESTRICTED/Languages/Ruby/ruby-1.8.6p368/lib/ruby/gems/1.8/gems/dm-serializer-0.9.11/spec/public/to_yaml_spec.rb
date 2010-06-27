require 'pathname'
require 'yaml'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Serialize, '#to_yaml' do
  #
  # ==== yummy YAML
  #

  before(:all) do
    @harness = Class.new(SerializerTestHarness) do
      def method_name
        :to_yaml
      end

      protected

      def deserialize(result)
        stringify_keys = lambda {|hash| hash.inject({}) {|a, (key, value)| a.update(key.to_s => value) }}
        result = YAML.load(result)
        (process = lambda {|object|
          if object.is_a?(Array)
            object.collect(&process)
          elsif object.is_a?(Hash)
            stringify_keys[object]
          else
            object
          end
        })[result]
      end
    end.new
  end

  it_should_behave_like 'A serialization method'
  it_should_behave_like 'A serialization method that also serializes core classes'

end
