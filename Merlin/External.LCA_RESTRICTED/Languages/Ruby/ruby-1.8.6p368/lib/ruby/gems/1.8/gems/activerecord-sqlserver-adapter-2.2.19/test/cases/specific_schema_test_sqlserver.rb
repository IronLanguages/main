require 'cases/sqlserver_helper'

class StringDefault < ActiveRecord::Base; end;
class SqlServerEdgeSchema < ActiveRecord::Base; end;

class SpecificSchemaTestSqlserver < ActiveRecord::TestCase
  
  should 'default strings before save' do
    default = StringDefault.new
    assert_equal nil, default.string_with_null_default
    assert_equal 'null', default.string_with_pretend_null_one
    assert_equal '(null)', default.string_with_pretend_null_two
    assert_equal 'NULL', default.string_with_pretend_null_three
    assert_equal '(NULL)', default.string_with_pretend_null_four
    assert_equal '(3)', default.string_with_pretend_paren_three
  end

  should 'default strings after save' do
    default = StringDefault.create
    assert_equal nil, default.string_with_null_default
    assert_equal 'null', default.string_with_pretend_null_one
    assert_equal '(null)', default.string_with_pretend_null_two
    assert_equal 'NULL', default.string_with_pretend_null_three
    assert_equal '(NULL)', default.string_with_pretend_null_four
  end
  
  context 'Testing edge case schemas' do
    
    setup do
      @edge_class = SqlServerEdgeSchema
    end
    
    context 'with description column' do

      setup do
        @da = @edge_class.create! :description => 'A'
        @db = @edge_class.create! :description => 'B'
        @dc = @edge_class.create! :description => 'C'
      end
      
      teardown { @edge_class.delete_all }

      should 'allow all sorts of ordering without adapter munging it up' do
        assert_equal ['A','B','C'], @edge_class.all(:order => 'description').map(&:description)
        assert_equal ['A','B','C'], @edge_class.all(:order => 'description asc').map(&:description)
        assert_equal ['A','B','C'], @edge_class.all(:order => 'description ASC').map(&:description)
        assert_equal ['C','B','A'], @edge_class.all(:order => 'description desc').map(&:description)
        assert_equal ['C','B','A'], @edge_class.all(:order => 'description DESC').map(&:description)
      end

    end
    

  end
  
  
end
