csc <<-EOL
  public interface IEmptyInterfaceGroup { }
  public interface IEmptyInterfaceGroup<T> { }

  public interface IEmptyInterfaceGroup1<T> {}
  public interface IEmptyInterfaceGroup1<T,V> {}

  public interface IInterfaceGroup {void m1();}
  public interface IInterfaceGroup<T> {void m1();}

  public interface IInterfaceGroup1<T> {void m1();}
  public interface IInterfaceGroup1<T,V> {void m1();}
EOL
