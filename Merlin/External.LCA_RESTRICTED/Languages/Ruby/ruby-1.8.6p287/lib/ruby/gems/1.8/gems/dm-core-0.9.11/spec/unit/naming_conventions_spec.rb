require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe "DataMapper::NamingConventions" do
  describe "Resource" do
    it "should coerce a string into the Underscored convention" do
      DataMapper::NamingConventions::Resource::Underscored.call('User').should == 'user'
      DataMapper::NamingConventions::Resource::Underscored.call('UserAccountSetting').should == 'user_account_setting'
    end

    it "should coerce a string into the UnderscoredAndPluralized convention" do
      DataMapper::NamingConventions::Resource::UnderscoredAndPluralized.call('User').should == 'users'
      DataMapper::NamingConventions::Resource::UnderscoredAndPluralized.call('UserAccountSetting').should == 'user_account_settings'
    end

    it "should coerce a string into the UnderscoredAndPluralized convention joining namespace with underscore" do
      DataMapper::NamingConventions::Resource::UnderscoredAndPluralized.call('Model::User').should == 'model_users'
      DataMapper::NamingConventions::Resource::UnderscoredAndPluralized.call('Model::UserAccountSetting').should == 'model_user_account_settings'
    end

    it "should coerce a string into the  UnderscoredAndPluralizedWithoutModule convention" do
      DataMapper::NamingConventions::Resource::UnderscoredAndPluralizedWithoutModule.call('Model::User').should == 'users'
      DataMapper::NamingConventions::Resource::UnderscoredAndPluralizedWithoutModule.call('Model::UserAccountSetting').should == 'user_account_settings'
    end

    it "should coerce a string into the Yaml convention" do
      DataMapper::NamingConventions::Resource::Yaml.call('UserSetting').should == 'user_settings.yaml'
      DataMapper::NamingConventions::Resource::Yaml.call('User').should == 'users.yaml'
    end
  end

  describe "Field" do
    it "should accept a property as input" do
      DataMapper::NamingConventions::Field::Underscored.call(Article.blog_id).should == 'blog_id'
    end
  end
end
