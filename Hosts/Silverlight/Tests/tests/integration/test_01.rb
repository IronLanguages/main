include System::Windows::Browser

# port of test_01
describe "01 - System.Windows.Browser" do
  before do
    unless @div
      @div = tag 'div', :id => 'testDiv'
      @div.append_child tag('a', :id => 'a1', :innerHTML => "Link should be enabled...")
      @div.append_child tag('span', {:id => 'h3', :innerHTML => "Needs to be updated"}, {:'background-color' => 'pink'})
      HtmlPage.Document.Body.append_child @div
    end
  end
  
  after do
    HtmlPage.document.body.remove_child @div
    @div = nil
  end

  it 'verifies GetElementById works' do
    ["testDiv", "a1", "h3"].each do |id|
      HtmlPage.Document.GetElementById("testDiv").should.not.be.nil
    end
  end
  
	it 'verifies GetStyleAttribute works' do
    color = HtmlPage.BrowserInformation.user_agent.to_s =~ /AppleWebKit/ ? 'rgb(255, 192, 203)' : 'pink'
    HtmlPage.Document.GetElementById('h3').
      GetStyleAttribute("background-color").should.equal color.to_clr_string
  end

	it 'verifies CreateElement works' do
    HtmlPage.Document.CreateElement('div').should.not.be.nil
	end

	it 'verifies SetAttribute works' do
    new_ctl = HtmlPage.Document.CreateElement('div')
    new_ctl.SetAttribute("id", "new_ctl")
    new_ctl.id.should.equal "new_ctl".to_clr_string
  end
  
  it 'verifies SetProperty works' do
    new_ctl = HtmlPage.Document.CreateElement('div')
	  new_value = "This is added by Merlin SL Test!"
	  new_ctl.SetProperty("innerHTML", new_value)
    new_ctl.GetProperty("innerHTML").should.equal new_value.to_clr_string
	end

	it 'verifies AppendChild works' do
	  old_cnt = @div.Children.Count
    new_ctl = HtmlPage.Document.CreateElement('div')
	  @div.AppendChild(new_ctl)
    @div.Children.Count.should.equal old_cnt + 1
  end
  
	it 'verifies Children collection returns correctly' do
    @div.AppendChild(tag("div"))
    @div.children.count.should.equal 3
  end
end
