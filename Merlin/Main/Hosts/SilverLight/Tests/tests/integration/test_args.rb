describe "args - Python splat args" do
  
  it 'call exec to create f(*args)' do
    python "exec 'def f(*args): return repr(args)'"
    python("f(1,2,3,4)", :expression).
      should.equal '(1, 2, 3, 4)'.to_clr_string
  end
  
end
