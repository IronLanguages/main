describe "exec file" do
  should 'define methods from execfile' do
    python "execfile('#{File.dirname(__FILE__)}/fixtures/mod1.py')"
    python('proof1()', :expression).should.equal "MOD1".to_clr_string
  end

  should 'define methods from a exec' do
    python "exec \"def proof2(): return \\\"MOD2\\\"\""
    python('proof2()', :expression).should.equal "MOD2".to_clr_string
  end
end
