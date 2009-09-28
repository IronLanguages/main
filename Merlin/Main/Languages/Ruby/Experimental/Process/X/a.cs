class C {
  static void Main() {
    System.Threading.Thread.Sleep(5000);
    System.Console.WriteLine("cmd line: '" + System.Environment.CommandLine + "'");
    System.Console.WriteLine("2: OUT");
    System.Console.Error.WriteLine("2: ERROR");
  }
}