csc <<-EOL
  #pragma warning disable 67
  public delegate void EventHandler(object source, int count);
  public partial class BasicEventClass {
    public event EventHandler OnEvent;
  }
  #pragma warning restore 67
  public class ClassWithEvents {
    public event EventHandler FullEvent;
    public static event EventHandler StaticFullEvent; 

    public void InvokeFullEvent(int count) {
      if (FullEvent != null) FullEvent(this, count);
    }

    public static void InvokeStaticFullEvent(int count) {
      if (StaticFullEvent != null) StaticFullEvent(new object(), count);
    }
  }
EOL
no_csc do
  class EventHandlerHelper
    def initialize
      @store = Hash.new(0)
    end

    def foo(s, count)
      @store[:method] += count
    end

    def [](key)
      @store[key]
    end

    def []=(key, value)
      @store[key] = value
    end
  end
end
