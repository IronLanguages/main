require 'mosquito'

Camping.goes :XhtmlTrans

module XhtmlTrans
  module Controllers
    class WithLayout < R '/with_layout'
      def get
        render :with_layout
      end
    end

    class WithoutLayout < R '/without_layout'
      def get 
        render :_without_layout
      end
    end
  end

  module Views
    def layout
      xhtml_transitional do
        head do title "title" end
        body do capture { yield } end
      end
    end

    def with_layout
      h1 "With layout"
    end

    def _without_layout
      xhtml_transitional do
        head do title "title" end
        body do h1 "Without layout" end
      end
    end
  end
end

class XhtmlTransTest < Camping::FunctionalTest
  def test_with_layout
    get '/with_layout'

    assert(@response.body =~ /DOCTYPE/, "No doctype defined")
  end

  def test_without_layout
    get '/without_layout'

    assert(@response.body =~ /DOCTYPE/, "No doctype defined")
  end

end

