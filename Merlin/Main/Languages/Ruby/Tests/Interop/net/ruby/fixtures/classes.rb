csc <<-EOL
  namespace CLRNew {
    public class Ctor {
      public int Tracker {get; set;}

      public Ctor() {
        Tracker = 1; 
      }
    }
  }
  public class PublicNameHolder {
    public string a() { return "a";}
    public string A() { return "A";}
    public string Unique() { return "Unique"; }
    public string snake_case() {return "snake_case";}
    public string CamelCase() {return "CamelCase";}
    public string Mixed_Snake_case() {return "Mixed_Snake_case";}
    public string CAPITAL() { return "CAPITAL";}
    public string PartialCapitalID() { return "PartialCapitalID";}
    public string PartialCapitalId() { return "PartialCapitalId";}
    public string __LeadingCamelCase() { return "__LeadingCamelCase";}
    public string __leading_snake_case() { return "__leading_snake_case";}
    public string foNBar() { return "foNBar"; }
    public string fNNBar() { return "fNNBar"; }
    public string NNNBar() { return "NNNBar"; }
    public string MyUIApp() { return "MyUIApp"; }
    public string MyIdYA() { return "MyIdYA"; }
    public string NaN() { return "NaN"; }
    public string NaNa() { return "NaNa"; }
  }

  public class SubPublicNameHolder : PublicNameHolder {
  }
EOL
no_csc do
  class CLRNew::Ctor
    def initialize
      tracker = 2
    end
  end
end


