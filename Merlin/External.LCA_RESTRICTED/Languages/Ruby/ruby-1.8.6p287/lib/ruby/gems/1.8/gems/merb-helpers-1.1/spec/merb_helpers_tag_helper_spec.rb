require File.dirname(__FILE__) + '/spec_helper'

# Merb::Router.prepare do
#   default_routes
# end

describe Merb::Helpers::Tag do
  include Merb::Helpers::Tag
  
  describe "#tag" do
    it 'generates <div>content</div> from tag :div, "content"' do
      response = request "/tag_helper/tag_with_content"

      response.should have_selector("div")
      response.body.to_s.should include("Astral Projection ~ Dancing Galaxy")
    end

    it 'outputs content returned by the block when block is given'  do
      response = request "/tag_helper/tag_with_content_in_the_block"

      response.should have_selector("div")
      response.body.should include("Astral Projection ~ Trust in Trance 1")
    end

    it 'generates tag attributes for all of keys of last Hash' do
      response = request "/tag_helper/tag_with_attributes"

      response.should have_selector("div.psy")
      response.should have_selector("div#bands")
      response.should have_selector("div[invalid_attr='at least in html']")
    end    

    it 'handles nesting of tags/blocks' do
      response = request "/tag_helper/nested_tags"

      response.should have_selector("div.discography ul.albums li.first")
      response.should have_selector("#tit:contains('Trust in Trance 2')")
    end
  end
end
