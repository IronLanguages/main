describe "auto_addref - clr.References" do
  it "should verify that the reference count is correct" do
    python 'import clr'
	  python("clr.References", :expression).size.should >= 7
  end
end
