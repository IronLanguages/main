$;.unshift('lib')
require "dbi/trace"
require 'stringio'
require "test/unit"

$stringio = StringIO.new

class TC_DBI_Trace < Test::Unit::TestCase
    def setup
        $stringio = StringIO.new
    end
end

class DBI::DBD::Dummy
    class Database < DBI::BaseDatabase
        DBI::BaseDatabase.instance_methods.each do |x|
            next if x.to_sym == :initialize
            define_method(x.to_sym) do |*args|
                $stringio << "[test] call:#{x} args:#{args.inspect}"
            end
        end

        def initialize
        end
    end
end
