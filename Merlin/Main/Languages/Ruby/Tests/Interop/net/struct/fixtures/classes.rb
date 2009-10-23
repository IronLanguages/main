csc <<-EOL
  public struct EmptyStruct {}
  public struct CStruct { public int m1() {return 1;}}
  public struct StructWithMethods {
    private short _shortField;
    public short ShortField {
      get { 
        return _shortField;
      }
      set {
        _shortField = value;
      }
    }
  }

EOL
