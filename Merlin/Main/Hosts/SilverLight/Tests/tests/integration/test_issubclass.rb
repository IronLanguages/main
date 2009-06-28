describe "issubclass" do
  before do
    python <<-PYTHON
import clr
import System
def x(): 
  return 2
def y(): 
  return 'abc'
PYTHON
  end

  should 'Verify IsSubclassOf returns false for same class as itself' do
    python("x().GetType().IsSubclassOf(System.Int32)", :expression).should.be.false
    python("y().GetType().IsSubclassOf(System.String)", :expression).should.be.false
  end

  should 'Verify IsSubclassOf returns False for other non-parent class' do
    python("x().GetType().IsSubclassOf(System.Array)", :expression).should.be.false
    python("y().GetType().IsSubclassOf(System.Int32)", :expression).should.be.false
  end

  should("Verify IsSubclassOf returns True for ancestor") do
    python("x().GetType().IsSubclassOf(System.Object)", :expression).should.be.true
    python("y().GetType().IsSubclassOf(System.Object)", :expression).should.be.true
  end
end
