require 'cases/sqlserver_helper'

class StringDefault < ActiveRecord::Base; end;
class SqlServerEdgeSchema < ActiveRecord::Base; end;

class SpecificSchemaTestSqlserver < ActiveRecord::TestCase
  
  should 'cope with multi line defaults' do
    default = StringDefault.new
    assert_equal "Some long default with a\nnew line.", default.string_with_multiline_default
  end
  
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
    
    context 'with bigint column' do

      setup do
        @b5k   = 5000
        @bi5k  = @edge_class.create! :bigint => @b5k, :description => 'Five Thousand'
        @bnum  = 9_000_000_000_000_000_000
        @bimjr = @edge_class.create! :bigint => @bnum, :description => 'Close to max bignum'
      end

      should 'can find by biginit' do
        assert_equal @bi5k,  @edge_class.find_by_bigint(@b5k)
        assert_equal @b5k,   @edge_class.find(:first, :select => 'bigint', :conditions => {:bigint => @b5k}).bigint
        assert_equal @bimjr, @edge_class.find_by_bigint(@bnum)
        assert_equal @bnum,  @edge_class.find(:first, :select => 'bigint', :conditions => {:bigint => @bnum}).bigint
      end

    end
    
    context 'with tinyint column' do

      setup do
        @tiny1 = @edge_class.create! :tinyint => 1
        @tiny255 = @edge_class.create! :tinyint => 255
      end

      should 'not treat tinyint like boolean as mysql does' do
        assert_equal 1, @edge_class.find_by_tinyint(1).tinyint
        assert_equal 255, @edge_class.find_by_tinyint(255).tinyint
      end
      
      should 'throw an error when going out of our tiny int bounds' do
        assert_raise(ActiveRecord::StatementInvalid) { @edge_class.create! :tinyint => 256 }
      end
      
    end
    
  end
  
  
end
