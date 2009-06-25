describe "Window" do
  it "should have a current field" do
    Window.current.class.should.equal Window
  end
  
  it 'should have a contents field' do
    Window.current.contents.class.should.equal System::Windows::Browser::HtmlElement
    Window.current.contents.get_property('id').should.equal "silverlightDlrWindow".to_clr_string
  end
  
  it 'should have a menu field' do
    Window.current.menu.class.should.equal System::Windows::Browser::HtmlElement
    Window.current.menu.get_property('id').should.equal "silverlightDlrWindowMenu".to_clr_string
  end
end