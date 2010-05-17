require 'cases/sqlserver_helper'
require 'models/person'
require 'models/reader'

class PessimisticLockingTestSqlserver < ActiveRecord::TestCase
  
  self.use_transactional_fixtures = false
  fixtures :people, :readers
  
  def setup
    Person.columns; Reader.columns # Avoid introspection queries during tests.
  end
  
  context 'For simple finds with default lock option' do

    should 'lock with simple find' do
      assert_nothing_raised do
        Person.transaction do
          Person.find 1, :lock => true
        end
      end
    end

    should 'lock with scoped find' do
      assert_nothing_raised do
        Person.transaction do
          Person.with_scope(:find => { :lock => true }) do
            Person.find 1
          end
        end
      end
    end

    should 'lock with eager find' do
      assert_nothing_raised do
        Person.transaction do
          Person.find 1, :include => :readers, :lock => true
        end
      end
    end

    should 'reload with lock when #lock! called' do
      assert_nothing_raised do
        Person.transaction do
          person = Person.find 1
          old, person.first_name = person.first_name, 'fooman'
          person.lock!
          assert_equal old, person.first_name
        end
      end
    end
    
    should 'simply add lock to find all' do
      assert_sql %r|SELECT \* FROM \[people\] WITH \(NOLOCK\)| do
        Person.all(:lock => 'WITH (NOLOCK)')
      end
    end

  end
  
  context 'For paginated finds' do
    
    setup do
      20.times { |n| Person.create!(:first_name => "Thing_#{n}") }
    end
    
    should 'cope with un-locked paginated results' do
      tally_not_locked = %r|SELECT count\(\*\) as TotalRows from \(SELECT TOP 1000000000 \* FROM \[people\]\s+WITH \(NOLOCK\) \) tally|
      inner_tmp_not_locked = %r|SELECT TOP 15 \* FROM \[people\] WITH \(NOLOCK\)|
      # Currently association limiting is not locked like the parent.
      association_limiting_not_locked = %r|SELECT \[readers\]\.\* FROM \[readers\] WITH \(NOLOCK\) WHERE \(\[readers\]\.person_id IN \(1,2,3,4,5\)\)|
      assert_sql(tally_not_locked,inner_tmp_not_locked) do
        Person.all(:include => :readers, :lock => 'WITH (NOLOCK)', :limit => 5, :offset => 10)
      end
    end

  end
  
  
  context 'For dueling concurrent connections' do
    
    use_concurrent_connections

    should 'no locks does not wait' do
      first, second = duel { Person.find 1 }
      assert first.end > second.end
    end
    
    should 'that second lock waits' do
      assert [0.2, 1, 5].any? { |zzz|
        first, second = duel(zzz) { Person.find 1, :lock => true }
        second.end > first.end
      }
    end
    
  end
  
    
  protected
  
  def duel(zzz = 5)
    t0, t1, t2, t3 = nil, nil, nil, nil
    a = Thread.new do
      t0 = Time.now
      Person.transaction do
        yield
        sleep zzz       # block thread 2 for zzz seconds
      end
      t1 = Time.now
    end
    b = Thread.new do
      sleep zzz / 2.0   # ensure thread 1 tx starts first
      t2 = Time.now
      Person.transaction { yield }
      t3 = Time.now
    end
    a.join
    b.join
    assert t1 > t0 + zzz
    assert t2 > t0
    assert t3 > t2
    [t0.to_f..t1.to_f, t2.to_f..t3.to_f]
  end
  
end
