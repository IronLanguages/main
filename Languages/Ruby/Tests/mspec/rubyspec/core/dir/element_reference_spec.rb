require File.expand_path('../../../spec_helper', __FILE__)
require File.expand_path('../fixtures/common', __FILE__)
require File.expand_path('../shared/glob', __FILE__)

describe "Dir.[]" do
  before :all do
    DirSpecs.create_mock_dirs
  end

  after :all do
    DirSpecs.delete_mock_dirs
  end

  it_behaves_like :dir_glob, :[]
end

describe "Dir.[]" do
  before :all do
    DirSpecs.create_mock_dirs
  end

  after :all do
    DirSpecs.delete_mock_dirs
  end

  it_behaves_like :dir_glob_recursive, :[]
end

describe "Dir.[]" do
  before :all do
    DirSpecs.create_mock_dirs

    @cwd = Dir.pwd
    Dir.chdir DirSpecs.mock_dir
  end

  after :all do
    Dir.chdir @cwd

    DirSpecs.delete_mock_dirs
  end
  
  it "accepts multiple patters" do
    expected = %w[
      .dotsubdir
      deeply
      deeply/nested
      dir
      dir_filename_ordering
    ]

    Dir['.dotsubd*', 'deeply', 'deeply/nested', 'di*'].sort.should == expected
  end
end