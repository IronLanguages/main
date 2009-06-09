require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXImage < TestCase
  def setup
    super(self.class.name)
  end

  def test_default_constructor_args_1
    img = FXImage.new(app)
    assert_same(nil, img.data)
    assert_equal(0, img.options)
    assert_equal(1, img.width)
    assert_equal(1, img.height)
  end
  
  def test_default_constructor_args_2
    img = FXImage.new(app, nil)
    assert_same(nil, img.data)
    assert_equal(0, img.options)
    assert_equal(1, img.width)
    assert_equal(1, img.height)
  end

  def test_default_constructor_args_3
    img = FXImage.new(app, nil, 0)
    assert_same(nil, img.data)
    assert_equal(0, img.options)
    assert_equal(1, img.width)
    assert_equal(1, img.height)
  end

  def test_default_constructor_args_4
    img = FXImage.new(app, nil, 0, 1)
    assert_same(nil, img.data)
    assert_equal(0, img.options)
    assert_equal(1, img.width)
    assert_equal(1, img.height)
  end

  def test_default_constructor_args_5
    img = FXImage.new(app, nil, 0, 1, 1)
    assert_same(nil, img.data)
    assert_equal(0, img.options)
    assert_equal(1, img.width)
    assert_equal(1, img.height)
  end

  def test_new_image_nil_pixels_owned
    img = FXImage.new(app, nil, IMAGE_OWNED)
    assert_equal(1, img.width)
    assert_equal(1, img.height)
    data = img.data
    assert_instance_of(FXMemoryBuffer, data)
    assert_equal(1*1, data.size)
    assert_equal(IMAGE_OWNED, img.options)
  end

  def test_create
    #
    # If the image owns its pixel data and IMAGE_KEEP was not specified,
    # the data should go away after we call create.
    #
    img = FXImage.new(app, nil, IMAGE_OWNED)
    assert_not_nil(img.data)
    img.create
    assert_nil(img.data)

    #
    # If the image owns its pixel data and IMAGE_KEEP was specified,
    # the data should stay after we call create.
    #
    img = FXImage.new(app, nil, IMAGE_KEEP|IMAGE_OWNED)
    assert_not_nil(img.data)
    img.create
    assert_not_nil(img.data)
  end

  #
  # Restore client-side pixel buffer from image.
  #
  def test_restore
    #
    # If no client-side pixel buffer exists at the time that
    # restore() is called, this should create one and set the
    # IMAGE_OWNED option.
    #
    img = FXImage.new(app)
    img.create
    assert_nil(img.data)
    assert_equal(0, img.options&IMAGE_OWNED)
    img.restore
    assert_not_nil(img.data)
    assert_not_equal(0, img.options&IMAGE_OWNED)
  end
  
  # Render client-side pixel buffer into pixmap
  def test_render
    # Test without client-side pixel buffer
    img = FXImage.new(app)
    img.render
  end

=begin

  def test_scale
    img.scale(2, 2, 0)
    img.scale(2, 2, 1)
  end
  
  def test_mirror
  end
  
  def test_rotate
  end
  
  def test_crop
  end
  
  def test_fill
  end
  
  def test_fade
  end
  
  def test_xshear
  end
  
  def test_yshear
  end
  
  def test_hgradient
  end
  
  def test_vgradient
  end
  
  def test_gradient
  end
  
  def test_blend
  end
  
  def test_savePixels
  end
  
  def test_loadPixels
  end
  
=end

end
