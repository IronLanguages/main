# Author:: Martin Ankerl (mailto:martin.ankerl@gmail.com)

# Recursive_Open_Struct provides a convenient interface to a hierarchy of configuration
# parameters. You do not need to define the accessors, they are created automatically
# on demand.
#
# Have a look at this example:
#
#    ac = Recursive_Open_Struct.new
#    ac.window.name = "SuperDuper"
#    ac.app.version = "2.1.3"
#    ac.this.is.automatically.created = "blabla"
#
# After you have created all of your configuration parameters, to prevent
# typos when using the parameters, the structure can be closed:
#
#    ac.close
#
# After closing,
#
#    ac.widnow.name = "UberSuperDuper"
#
# You get the usual NoMethodError, because 'widnow' does not exist.
#
class Recursive_Open_Struct
  # Create a new Recursive_Open_Struct.
  def initialize
    @methods = Hash.new
    @open = true
  end

  # automatically add parameters
  def method_missing(method, *params) # :nodoc:
    # setting or getting?
    is_setting = !params.empty?

    key = method.id2name
    # remove trailing =
    key.chop! if is_setting

    # if structure is closed, disable hierarchy creation
    super unless @methods.has_key?(key) || @open

    if is_setting
      # assigning a new value
      if @methods[key].class == Recursive_Open_Struct
        raise TypeError, "overwriting previously created hierarchy entry '#{key}' not allowed", caller(1)
      end
      @methods[key] = *params
    else
      # no param: create new Recursive_Open_Struct object, if nothing is set.
      unless @methods.has_key?(key)
        @methods[key] = Recursive_Open_Struct.new
      end
    end
    @methods[key]
  end

  # An alternative way to access the value of an attribute
  #  s = Recursive_Open_Struct.new
  #  s.name = "Hugo"
  #  s["name"] # "Hugo"
  def [](key)
    @methods[key]
  end

  # An alternative way to set the value of an attribute
  #  s = Recursive_Open_Struct.new
  #  s["name"] = "Hugo"
  #  s.name # "Hugo"
  def []=(key, value)
    self.send((key+"=").to_sym, value)
  end

  # call-seq:
  #  attrs() -> an_array
  #
  # Return a sorted array of attribute names, similar to #methods.
  #
  #  s = Recursive_Open_Struct.new
  #  s.name = "martinus"
  #  s.age = 25
  #  s.attrs # returns ["age", "name"]
  def attrs
    @methods.keys.sort
  end

  # After calling #close, no further modification of the configuration hierarchy
  # is allowed. This is not as strict as #freeze, because you are still allowed
  # to modify data.
  #
  #  s = Recursive_Open_Struct.new
  #  s.name = "Hugo"
  #  s.close
  #  s.name = "martinus" # does still work
  #  s.age = 25 # raises NoMethodError
  def close
    do_set_open_status(false)
  end

  # Reopens a Recursive_Open_Struct which was closed with #close earlier.
  # After this call, it is possible to modify the structure again.
  def re_open
    do_set_open_status(true)
  end

  # call-seq:
  #  open?() -> boolean
  #
  # Return whether the structure is still open or not.
  #
  #  s = Recursive_Open_Struct.new
  #  s.open? # returns true
  #  s.close
  #  s.open? # returns false
  def open?
    @open
  end

  # call-seq:
  #  each() { |elem| ... }
  #
  # Iterates through all elements of the Recursive_Open_Struct in alphabetic order.
  def each
    attrs.each do |attr|
      yield @methods[attr]
    end
  end

  protected

  def do_set_open_status(status)
    @methods.each_value do |val|
      val.do_set_open_status(status) if val.class == Recursive_Open_Struct
    end
    @open = status
  end
end

if __FILE__ == $0
  require 'test/unit'

  class TestRecursiveOpenStruct < Test::Unit::TestCase
    def setup
      @s = Recursive_Open_Struct.new
    end

    def setAndAssertValue(val)
      @s.test = val
      assert_equal(val, @s.test)
      @s.close
      assert_equal(val, @s.test)
      @s.test = "asdf"
      assert_equal("asdf", @s.test)
    end

    def testSetNil
      setAndAssertValue(nil)
    end

    def testSimple
      @s.test = "xx"
      @s.close
      assert_equal("xx", @s.test)
    end

    def testSetFalse
      setAndAssertValue(false)
    end

    def testSetStr
      setAndAssertValue("topfen")
    end

    def testSetClass
      setAndAssertValue(String)
    end

    def testSetTrue
      setAndAssertValue(true)
    end

    def testSet0
      setAndAssertValue(0)
    end

    def testRaiseTypeError
      @s.a.b = 1
      assert_raise(TypeError) do
        @s.a = 3
      end
    end

    def testAttrs
      assert_equal([], @s.attrs)
      @s.b = "x"
      @s.a = "a"
      assert_equal(["a", "b"], @s.attrs)
    end

    def testRecursive
      @s.a.b = 1
      @s.a.c = 2
      assert_equal(["a"], @s.attrs)
    end

    def testStrange
      @s.a
      assert_equal(["a"], @s.attrs)
      assert_equal(Recursive_Open_Struct, @s.a.class)
      @s.a.x = "asfd"
      assert_equal("asfd", @s.a.x)
    end

    def testKlammer
      @s.a = "asdf"
      assert_equal("asdf", @s["a"])
      @s.b_x = "hog"
      assert_equal("hog", @s["b_x"])
      @s.c.b.a = 1234
      assert_equal(1234, @s["c"]["b"]["a"])
    end

    def testDeep
      @s.a.b.c.d.e.f.g.h.i.j.k.l.m.n.o.p.q.r.s.t.u.v.w.x.y.z = false
      @s.close
      assert_raise(NoMethodError) do
        @s.blub = "hellow"
      end
      assert_equal(false, @s.a.b.c.d.e.f.g.h.i.j.k.l.m.n.o.p.q.r.s.t.u.v.w.x.y.z)
    end
  end
end
