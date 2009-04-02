require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

require "dm-types"
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'addressable'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'billable'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'commentable'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'article'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'image'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'user'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'viewable'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'topic'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'rating'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'taggable'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'bot'
require Pathname(__FILE__).dirname.expand_path.parent / 'data' / 'tag'
DataMapper.auto_migrate!

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  describe 'DataMapper::Is::Remixable' do
    describe 'DataMapper::Resource' do
      it "should know if it is remixable" do
        User.is_remixable?.should be(false)
        Image.is_remixable?.should be(true)
        Article.is_remixable?.should be(false)
        Commentable.is_remixable?.should be(true)
      end
    end

    it "should only allow remixables to be remixed" do
      lambda { User.remix 1, :articles }.should raise_error(Exception)
    end

    it "should not allow enhancements of modules that aren't remixed" do
      lambda {
        User.enhance :images
      }.should raise_error
    end

    it "should provide a default suffix values for models that do 'is :remixable'" do
      Image.suffix.should == "image"
    end

    it "should allow enhancing a model that is remixed" do
      Article.enhance :images do
        def self.test_enhance
          true
        end
      end

      ArticleImage.should respond_to("test_enhance")
    end

    it "should allow enhancing a model that was remixed from a nested module" do
      Article.enhance :ratings do
        def self.test_enhance
          true
        end
      end

      ArticleRating.should respond_to("test_enhance")
      ArticleRating.should respond_to("total_rating")
      ArticleRating.new.should respond_to("user_id")
      ArticleRating.new.should respond_to("rating")
    end

    it "should allow enhancing the same remixable twice with different class_name attributes" do
      Article.enhance :taggable, "UserTagging" do
        def self.test_enhance
          true
        end
      end

      UserTagging.should respond_to("test_enhance")
      UserTagging.should respond_to("related_tags")
      UserTagging.new.should respond_to("user_id")
      UserTagging.new.should respond_to("tag")

      Article.enhance :taggable, "BotTagging" do
        def self.test_enhance_2
          true
        end
      end
      BotTagging.should respond_to("test_enhance_2")
      BotTagging.should respond_to("related_tags")
      BotTagging.new.should respond_to("bot_id")
      BotTagging.new.should respond_to("tag")
    end

    it "should through exception when enhancing an unknown class" do
      lambda {
        Article.enhance :taggable, "NonExistentClass"
      }.should raise_error
    end

    it "should provided a map of Remixable Modules to Remixed Models names" do
      User.remixables.should_not be(nil)
    end

    it "should store the remixed model in the map of Remixable Modules to Remixed Models" do
      User.remixables[:billable][:account][:model].should == Account
      # nested remixables
      User.remixables[:rating][:user_rating][:model].should == UserRating
      Article.remixables[:rating][:article_rating][:model].should == ArticleRating
      Topic.remixables[:rating][:rating][:model].should == Rating
    end

    it "should store the remixee reader name in the map of Remixable Modules to Remixed Models" do
      User.remixables[:billable][:account][:reader].should == :accounts
      # nested remixables
      User.remixables[:rating][:user_rating][:reader].should == :user_ratings
      Article.remixables[:rating][:article_rating][:reader].should == :ratings
      Topic.remixables[:rating][:rating][:reader].should == :ratings_for_topic
    end

    it "should store the remixee writer name in the map of Remixable Modules to Remixed Models" do
      User.remixables[:billable][:account][:writer].should == :accounts=
      # nested remixables
      User.remixables[:rating][:user_rating][:writer].should == :user_ratings=
      Article.remixables[:rating][:article_rating][:writer].should == :ratings=
      Topic.remixables[:rating][:rating][:writer].should == :ratings_for_topic=
    end

    it "should allow specifying an alternate class name" do
      User.remixables[:billable][:account][:model].name.should_not == "UserBillable"
      User.remixables[:billable][:account][:model].name.should == "Account"
    end

    it "should create a storage name based on the class name" do

      Article.remixables[:image][:article_image][:model].storage_names[:default].should == "article_images"
      User.remixables[:billable][:account][:model].storage_names[:default].should == "accounts"
    end

    it "should allow creating an accessor alias" do
      article = Article.new
      article.should respond_to("pics")
    end

    it "should copy properties from the Remixable Module to the Remixed Model" do
      #Billabe => Account
      account = Account.new

      account.should respond_to("cc_num")
      account.should respond_to("cc_type")
      account.should respond_to("expiration")
    end

    it "should allow 1:M relationships with the Remixable Module" do
      user = User.new
      addy = UserAddress.new
      addy2 = UserAddress.new

      user.first_name = "Jack"
      user.last_name = "Tester"

      addy.address1 = "888 West Whatnot Ave."
      addy.city = "Los Angeles"
      addy.state = "CA"
      addy.zip = "90230"

      addy2.address1 = "325 East NoWhere Lane"
      addy2.city = "Fort Myers"
      addy2.state = "FL"
      addy2.zip = "33971"

      user.user_addresses << addy
      user.user_addresses << addy2

      user.user_addresses.length.should be(2)
    end

    it "should allow 1:1 relationships with the Remixable Module" do
      article = Article.new
      image1  = ArticleImage.new
      image2  = ArticleImage.new

      article.title = "Really important news!"
      article.url = "http://example.com/index.html"

      image1.description = "Shocking and horrific photo!"
      image1.path = "~/pictures/shocking.jpg"

      image2.description = "Other photo"
      image2.path = "~/pictures/mom_naked.yipes"

      begin
        article.pics << image1
        false
      rescue Exception => e
        e.class.should be(NoMethodError)
      end

      article.pics = image2
      article.pics.path.should == image2.path
    end

    # Example:
    #   Users are Commentable by many Users
    #
    it "should allow M:M unary relationships through the Remixable Module" do
      user = User.new
      user.first_name = "Tester"
      user2 = User.new
      user2.first_name = "Testy"

      comment = UserComment.new
      comment.comment = "YOU SUCK!"
      comment.commentor = user2

      user.comments << comment

      user2.comments.length.should be(0)

      comment.commentor.first_name.should == "Testy"

      user.comments.length.should be(1)
    end

    # Example:
    #   Articles are Commentable by many Users
    #
    it "should allow M:M relationships through the Remixable Module" do
      user = User.new
      article = Article.new

      ac = ArticleComment.new

      user.first_name = "Talker"
      user.last_name = "OnTheInternetz"

      article.url = "Http://example.com/"
      article.title = "Important internet thingz, lol"

      ac.comment = "This article sux!"

      article.comments << ac
      user.article_comments << ac

      article.comments.first.should be(ac)
      user.article_comments.first.should be(ac)
    end

    # Example:
    # Remixable Image add functionality to any class that remixes it
    #   Image::RemixerClassMethods defines a method called 'total_images' that counts the total number of images for the class
    #   Image::RemixerInstanceMethods defines a method called 'most_viewed_image' that find the most viewed image for an object
    #
    #   User.remixes n, :images
    #   User.total_images => count of all images owned by all users
    #   User.first.most_viewed_image => would return the most viewed image
    #
    it "should add a remixables' 'RemixerClassMethods' modules to the remixing class" do
      Article.respond_to?(:test_remixer_class_method).should be(true)
      Article.test_remixer_class_method.should == 'CLASS METHOD FOR REMIXER'
    end

    it "should add a remixables' 'RemixerInstanceMethods' modules to the remixing class" do
      Article.new.respond_to?(:test_remixer_instance_method).should be(true)
      Article.new.test_remixer_instance_method.should == 'INSTANCE METHOD FOR REMIXER'
    end

    # Example:
    # Remixable Image add functionality to any class that remixes it
    #   Image::RemixeeClassMethods defines a method called 'damaged_files' would return a list of all images with invalid checksums (or whatev)
    #   Image::RemixeeInstanceMethods defines a method called 'mime_type' that find the mime type of the particular image
    #
    #   Article.remixes n, :images
    #   # => yields and ArticleImage Class
    #   ArticleImage.damaged_files => list of all images with invalid checksums
    #   ArticleImage.first.mime_type => would return the mime type of that image
    #
    it "should add a remixables' 'RemixeeClassMethods' modules to the generated remixed class" do
      ArticleImage.respond_to?(:test_remixee_class_method).should be(true)
      ArticleImage.test_remixee_class_method.should == 'CLASS METHOD FOR REMIXEE'
    end

    it "should add a remixables' 'RemixeeInstanceMethods' modules to the generated remixed class" do
      ArticleImage.new.respond_to?(:test_remixee_instance_method).should be(true)
      ArticleImage.new.test_remixee_instance_method.should == 'INSTANCE METHOD FOR REMIXEE'
    end

    # Example:
    # User.remixes n, :images, :as => "pics"
    # User.first.pics would be the acessor for images
    # User.first.user_images should raise method not found
    #
    it 'should remove the original attribute accessor when attaching an optional one' do
      Article.new.respond_to?(:pics).should be(true)
      User.new.respond_to?(:user_addresses).should be(true)
    end

    # Currently:
    #   Submission.remixes n, :comments
    #   SubmissionComment.new.user = User.first => throws exception, accessor name is 'users' instead
    #
    # Example:
    #   User.remix 1, :images
    #     # => User.image & UserImage.user
    #
    #   User.remix n, :images
    #     # => User.images & UserImage.user
    #
    #   User.remix n, :comments, :for => 'User', :via => 'commentor'
    #     # => User.comments & UserComment.user & UserComment.commentor
    #
    it 'should pluralize accessor names with respect to cardinality' do
      pending
    end

    # Note:
    # Currently the :via flag allows one to specify another name for the field, but it always appends _id
    #
    # Example:
    #   User w/ PK being 'login_name'
    #   User.remixes n, :comments, :for => 'User', :via => 'commentor'
    #
    #   Comment Table:
    #     * id
    #     * text
    #     * user_login_name
    #     * commentor_id #=> should be able to specify it to be commentor_login_name
    #
    it 'should allow the primary and child field names to be specified while remixing' do
      pending
    end
  end
end
