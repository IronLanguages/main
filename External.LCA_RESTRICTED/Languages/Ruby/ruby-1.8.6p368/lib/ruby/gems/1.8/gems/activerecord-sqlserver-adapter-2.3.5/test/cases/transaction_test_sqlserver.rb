require 'cases/sqlserver_helper'
require 'models/ship'
require 'models/developer'

class TransactionTestSqlserver < ActiveRecord::TestCase
  
  self.use_transactional_fixtures = false
  
  setup :delete_ships
  
  context 'Testing transaction basics' do
    
    should 'allow ActiveRecord::Rollback to work in 1 transaction block' do
      Ship.transaction do
        Ship.create! :name => 'Black Pearl'
        raise ActiveRecord::Rollback
      end
      assert_no_ships
    end
    
    should 'allow nested transactions to totally rollback' do
      begin
        Ship.transaction do
          Ship.create! :name => 'Black Pearl'
          Ship.transaction do
            Ship.create! :name => 'Flying Dutchman'
            raise 'HELL'
          end
        end
      rescue Exception => e
        assert_no_ships
      end
    end

  end
  
  context 'Testing #outside_transaction?' do
  
    should 'work in simple usage' do
      assert Ship.connection.outside_transaction?
      Ship.connection.begin_db_transaction
      assert !Ship.connection.outside_transaction?
      Ship.connection.rollback_db_transaction
      assert Ship.connection.outside_transaction?
    end
    
    should 'work inside nested transactions' do
      assert Ship.connection.outside_transaction?
      Ship.transaction do
        assert !Ship.connection.outside_transaction?
        Ship.transaction do
          assert !Ship.connection.outside_transaction?
        end
      end
      assert Ship.connection.outside_transaction?
    end
    
    should 'not call rollback if no transaction is active' do
      assert_raise RuntimeError do
        Ship.transaction do
          Ship.connection.rollback_db_transaction
          Ship.connection.expects(:rollback_db_transaction).never
          raise "Rails doesn't scale!"
        end
      end
    end
    
    should 'test_open_transactions_count_is_reset_to_zero_if_no_transaction_active' do
      Ship.transaction do
        Ship.transaction do
          Ship.connection.rollback_db_transaction
        end
        assert_equal 0, Ship.connection.open_transactions
      end
      assert_equal 0, Ship.connection.open_transactions
    end
    
  end unless active_record_2_point_2?
  
  
  
  protected
  
  def delete_ships
    Ship.delete_all
  end
  
  def assert_no_ships
    assert Ship.count.zero?, "Expected Ship to have no models but it did have:\n#{Ship.all.inspect}"
  end
  
end

