###################################
#
# vrtvitem.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2000-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################


VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vrcomctl'

class VRTreeview

  class VRTreeviewItem
=begin
== VRTreeview::VRTreeviewItem
Represents an item in Treeview.
This is just a referencing object so that only one treeview item entity
is referenced by many referencing objects.

=== Class Methods
--- new(treeview,hitem,lparam=0)
    You may not need this method in your script.

=== Methods
--- insertChildFirst(text,lparam=0)
    Adds a new child item with ((|text|)) and ((|lparam|)) for the first item.
--- insertChildLast(text,lparam=0)
    Adds a new child item with ((|text|)) and ((|lparam|)) for the last item.
--- insertChildAfter(item,text,lparam=0)
    Adds a new child item after ((|item|))
--- parent
    Returns the parent item.
--- firstChild
    Returns the first child item.
--- nextSibling
    Returns the next sibling item of the treeview item.
--- eachChild
    Yields each child items.
--- icon
--- icon=
    Sets/Gets the icon for the treeview item.
--- text
--- text=
    Sets/Gets the text for the treeview item.
--- lparam
--- lparam=
    Sets/Gets the lparam for the treeview item.
--- state
--- state=
    Sets/Gets the state for the treeview item.
=end


    attr :treeview
    attr :hitem
  private
    def initialize(tv,hitem,lparam=0)
      @treeview = tv
      @hitem=hitem
    end

    def _vr_addChild(it,text,lparam)
      VRTreeviewItem.new(@treeview,@treeview.insertItem(@hitem,it,text,lparam))
    end

  public
    def insertChildFirst(text,lparam=0)
      _vr_addChild(WConst::TVI_FIRST,text,lparam)
    end
    def insertChildLast(text,lparam=0)
      _vr_addChild(WConst::TVI_LAST,text,lparam)
    end
    def insertChildAfter(item,text,lparam=0)
      _vr_addChild(item.hitem,text,lparam)
    end
    alias addChild insertChildLast

    def parent
      VRTreeviewItem.new( @treeview,@treeview.getParentOf(@hitem) )
    end
    def firstChild
      VRTreeviewItem.new( @treeview,@treeview.getChildOf(@hitem) )
    end
    def nextSibling
      VRTreeviewItem.new( @treeview,@treeview.getNextSiblingOf(@hitem) )
    end

    def eachChild
      r = firstChild
      while r.hitem do
        yield r 
        r = r.nextSibling
      end
    end


    ["Icon","LParam","Text","State"].each do |nm|
eval <<"EEOOFF"
      def #{nm.downcase}
        @treeview.getItem#{nm}Of(@hitem)
      end
      def #{nm.downcase}=(p)
        @treeview.setItem#{nm}Of(@hitem,p)
      end
EEOOFF
    end

  end

    def rootItem
      VRTreeviewItem.new(self,WConst::TVGN_ROOT)
    end
end

