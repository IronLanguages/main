describe :repeated_net_assembly, :shared => true do 
  it "only loads once with require followed by require" do
    @engine.should be_able_to_load(@assembly).with(:require).once
  end

  it "loads twice with require followed by load" do
    @engine.should be_able_to_load(@assembly).with(:require).followed_by(:load)
  end

  it "loads twice with require followed by load_assembly" do
    @engine.should be_able_to_load(@assembly).with(:require).followed_by(:load_assembly)
  end
  
  it "loads twice with load followed by require" do
    @engine.should be_able_to_load(@assembly).with(:load).followed_by(:require)
  end
  
  it "loads twice with load followed by load" do
    @engine.should be_able_to_load(@assembly).with(:load).twice
  end

  it "loads twice with load followed by load_assembly" do
    @engine.should be_able_to_load(@assembly).with(:load).followed_by(:load_assembly)
  end

  it "loads twice with load_assembly followed by require" do
    @engine.should be_able_to_load(@assembly).with(:load_assembly).followed_by(:require)
  end
  
  it "loads twice with load_assembly followed by load" do
    @engine.should be_able_to_load(@assembly).with(:load_assembly).followed_by(:load)
  end

  it "loads twice with load_assembly followed by load_assembly" do
    @engine.should be_able_to_load(@assembly).with(:load_assembly).twice
  end
end
