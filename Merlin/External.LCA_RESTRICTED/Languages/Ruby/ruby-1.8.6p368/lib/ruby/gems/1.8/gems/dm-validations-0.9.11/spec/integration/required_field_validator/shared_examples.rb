describe GitOperation do
  before :each do
    @operation = GitOperation.new
  end

  describe "unnamed SCM operation", :shared => true do
    before :each do
      @operation.name = nil
      @operation.valid?
    end

    it "is not valid" do
      @operation.should_not be_valid
    end

    it "is not valid in default validation context" do
      @operation.should_not be_valid(:default)
    end

    it "points to blank name in the error message" do
      @operation.errors.on(:name).should include('Name must not be blank')
    end
  end
end
