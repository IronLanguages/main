# encoding: ascii-8bit
require 'test/unit'
require 'memcache'

class TestEventMachine < Test::Unit::TestCase

  def test_concurrent_fibers
    return puts("Skipping EventMachine test, not Ruby 1.9") if RUBY_VERSION < '1.9'
    return puts("Skipping EventMachine test, no live server") if !live_server?

    require 'eventmachine'
    require 'memcache/event_machine'
    ex = nil
    m = MemCache.new(['127.0.0.1:11211', 'localhost:11211'])
    within_em(3) do
      begin
        key1 = 'foo'
        key2 = 'bar'*50
        key3 = '£∞'*45
        value1 = 'abc'
        value2 = 'xyz'*1000
        value3 = '∞§¶•ª'*1000

        100.times do
          assert_equal "STORED\r\n", m.set(key1, value1)
          assert_equal "STORED\r\n", m.set(key2, value2)
          assert_equal "STORED\r\n", m.set(key3, value3)
          m.get(key1)
          m.get(key2)
          m.get(key3)
          assert m.delete(key1)
          assert_equal "STORED\r\n", m.set(key1, value2)
          m.get(key1)
          assert_equal "STORED\r\n", m.set(key2, value3)
          m.get(key2)
          assert_equal "STORED\r\n", m.set(key3, value1)
          m.get(key3)
          h = m.get_multi(key1, key2, key3)
          assert h
          assert_equal Hash, h.class
          assert h.size > 0
        end
      rescue Exception => exp
        puts exp.message
        ex = exp
      ensure
        EM.stop
      end
    end
    raise ex if ex
  end

  def test_live_server
    return puts("Skipping EventMachine test, not Ruby 1.9") if RUBY_VERSION < '1.9'
    return puts("Skipping EventMachine test, no live server") if !live_server?

    require 'eventmachine'
    require 'memcache/event_machine'
    ex = nil
    within_em do
      begin
        m = MemCache.new(['127.0.0.1:11211', 'localhost:11211'])
        key1 = 'foo'
        key2 = 'bar'*50
        key3 = '£∞'*50
        value1 = 'abc'
        value2 = 'xyz'*1000
        value3 = '∞§¶•ª'*1000
    
        1000.times do
          assert_equal "STORED\r\n", m.set(key1, value1)
          assert_equal "STORED\r\n", m.set(key2, value2)
          assert_equal "STORED\r\n", m.set(key3, value3)
          assert_equal value1, m.get(key1)
          assert_equal value2, m.get(key2)
          assert_equal value3, m.get(key3)
          assert_equal "DELETED\r\n", m.delete(key1)
          assert_equal "STORED\r\n", m.set(key1, value2)
          assert_equal value2, m.get(key1)
          assert_equal "STORED\r\n", m.set(key2, value3)
          assert_equal value3, m.get(key2)
          assert_equal "STORED\r\n", m.set(key3, value1)
          assert_equal value1, m.get(key3)
          assert_equal({ key1 => value2, key2 => value3, key3 => value1 }, 
                       m.get_multi(key1, key2, key3))
        end
      rescue Exception => exp
        puts exp.message
        ex = exp
      ensure
        EM.stop
      end
    end
    raise ex if ex
  end
  
  private
  
  def within_em(count=1, &block)
    EM.run do
      count.times do
        Fiber.new(&block).resume
      end
    end
  end
  
  def live_server?
    TCPSocket.new('localhost', 11211) rescue nil
  end
end