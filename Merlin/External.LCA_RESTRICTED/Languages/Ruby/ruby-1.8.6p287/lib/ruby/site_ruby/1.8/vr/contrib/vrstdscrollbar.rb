###############################
#
# contrib/vrctlcolor.rb
#
# These module is contributed by yas-san.
# Modified by nyasu <nyasu@osk.3web.ne.jp>
# Distributed at http://vruby.sourceforge.net/index.html
#
###############################


=begin
== VRStdScrollbar
ウインドウは垂直と水平のスクロールバーを持っています。
スクロールバーはウインドウスタイルのWS_VSCROLLとWS_HSCROLLを指定することで表示されます。
このモジュールはウインドウのスクロールバーを処理します。

このモジュールをコントロールに組み込んだときの結果を下記に示します。

* 問題なし
  * VRForm
  * VRPanel
  * VRCanvasPanel
  * VRButton
  * VRCheckbox
  * VRRadiobutton
* SDKで定義済み。組み込む必要はない
  * VREdit
  * VRText
  * VRRichedit
  * VRList
* 問題あり
  * VRCombobox
  * VREditCombobox
  * VRGroupbox
  * VRStatic
  
=== Methods
--- getVScrollRange
    垂直スクロールバーの範囲を返します。[((|min|)),((|max|))]のフォーマットで返ります。
    ((|min|))は最小値、((|max|))は最大値です。

--- setVScrollRange(min,max)
    垂直スクロールバーの範囲を設定します。 ((|min|)) は最小値、 ((|max|)) は最大値です。

--- getHScrollRange
--- setHScrollRange
    水平スクロールバーの範囲を取得または設定します。 ((<getVScrollRange>)),
    ((<setVScrollRange>)) を参照してください。
    
--- vscroll_position
--- vscroll_position=(val)
    垂直スクロールバーの位置を取得または設定します。

--- hscroll_position
--- hscroll_position=(val)
    水平スクロールバーの位置を取得または設定します。

--- vscroll_pagesize
--- vscroll_pagesize=(val)
    垂直スクロールバーのページサイズを取得または設定します。
    スクロールバーは、全体の大きさとこの値の比から適切なつまみの大きさを求めます。

--- hscroll_pagesize
--- hscroll_pagesize=(val)
    水平スクロールバーのページサイズを取得または設定します。
    ((<vscroll_pagesize>)),((<vscroll_pagesize=>))を参照してください。

--- vscroll_smallstep
--- vscroll_smallstep=(val)
    垂直スクロールバーの小さい移動量を取得または設定します。
    スクロールバーの矢印ボタンがクリックされた場合、この値はスクロールバーの
    位置へ加えられます。
    
--- hscroll_smallstep
--- hscroll_smallstep=(val)
    水平スクロールバーの小さい移動量を取得または設定します。
    ((<hscroll_smallstep>)),((<hscroll_smallstep=>))を参照してください。
    
--- vscroll_longstep
--- vscroll_longstep=(val)
    垂直スクロールバーの大きい移動量を取得または設定します。
    スクロールバーの背景がクリックされた場合、この値はスクロールバーの
    位置へ加えられます。

--- hscroll_longstep
--- hscroll_longstep=(val)
    水平スクロールバーの大きい移動量を取得または設定します。
    ((<hscroll_longstep>)),((<hscroll_longstep=>))を参照してください。

=== Events
--- vscroll(code)
--- hscroll(code)
    垂直または水平スクロールバーが動かされた。((|code|))はユーザーが要求した
    スクロール動作の意味を示します。詳しくはMSDNのWM_VSCROLLの項を参照してください。

== VRStdScrollbar
The windows have vertical and the horizontal scroll bar.
The scroll bar is displayed by specifying WS_VSCROLL or WS_HSCROLL of the window style. 
This module processes the scroll bars on the window. 

Result of using this module

* no problem
  * VRForm
  * VRPanel
  * VRCanvasPanel
  * VRButton
  * VRCheckbox
  * VRRadiobutton
* defined by SDK. no necessity to include. 
  * VREdit
  * VRText
  * VRRichedit
  * VRList
* not effective. 
  * VRCombobox
  * VREditCombobox
  * VRGroupbox
  * VRStatic

=== Methods
--- getVScrollRange
    returns the range of the vertical scroll bars. It returns by the format of 
    [((|min|)), ((|max|))] .((|min|)) is the minimum value and ((|max|)) is maximum.

--- setVScrollRange(min,max)
    Specifies the range of the vertical scroll bar.
    ((|min|)) is the minimum value and ((|max|)) is maximum.

--- getHScrollRange
--- setHScrollRange
    Retrieves or specifies range of the horizontal scroll bar.
    Refer to ((<getVScrollRange>)) , ((<setVScrollRange>))
    
--- vscroll_position
--- vscroll_position=(val)
    Retrieves or Specifies position of the vertical scroll bar.

--- hscroll_position
--- hscroll_position=(val)
    Retrieves or Specifies position of the horizontal scroll bar.

--- vscroll_pagesize
--- vscroll_pagesize=(val)
    Retrieves or Specifies the page size of vertical scroll bar.
    A scroll bar uses this value to determine the appropriate size of 
    the proportional scroll box. 

--- hscroll_pagesize
--- hscroll_pagesize=(val)
    Retrieves or Specifies the page size of horizontal scroll bar.
    Refer to ((<vscroll_pagesize>)),((<vscroll_pagesize=>))

--- vscroll_smallstep
--- vscroll_smallstep=(val)
    Retrieves or Specifies the small step of the vertical scroll bar. 
    If the arrow button of the scroll bar was clicked, this value is added to
    the position of the scroll bar. 

--- hscroll_smallstep
--- hscroll_smallstep=(val)
    Retrieves or Specifies the small step of the horizontal scroll bar. 
    Refer to ((<hscroll_smallstep>)),((<hscroll_smallstep=>))

--- vscroll_longstep
--- vscroll_longstep=(val)
    Retrieves or Specifies the long step of the vertical scroll bar. 
    If the background of the scroll bar was clicked, this value is added to
    the position of the scroll bar. 

--- hscroll_longstep
--- hscroll_longstep=(val)
    Retrieves or Specifies the long step of the horizontal scroll bar. 
    Refer to ((<hscroll_longstep>)),((<hscroll_longstep=>))

=== Events
--- vscroll(code)
--- hscroll(code)
    the vertical or horizontal scroll bar was moved.((|code|)) that indicates 
    the user's scrolling request. Refer to the paragraph of WM_VSCROLL in MSDN
    
=end

module VRStdScrollbar
  include VRMessageHandler
  
  SB_HORZ=0
  SB_VERT=1
  SB_CTL=2
  
  private
  
  #mask of scroll info
  SIF_RANGE=0x0001
  SIF_PAGE=0x0002
  SIF_POS=0x0004
  SIF_DISABLENOSCROLL=0x0008
  SIF_TRACKPOS=0x0010
  SIF_ALL=(SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS)
  
  SetScrollInfo=Win32API.new('user32','SetScrollInfo',%w(l l p l),'l')
  GetScrollInfo=Win32API.new('user32','GetScrollInfo',%w(l l p),'l')
  
  def stdscrollbarinit
    addHandler WMsg::WM_HSCROLL,"hstdscroll",MSGTYPE::ARGINTINT,nil
    addHandler WMsg::WM_VSCROLL,"vstdscroll",MSGTYPE::ARGINTINT,nil
    acceptEvents [ WMsg::WM_HSCROLL,WMsg::WM_VSCROLL]
    @scroll_smallstep_vert=1 ; @scroll_smallstep_horz=1
    @scroll_longstep_vert=10 ; @scroll_longstep_horz=10
  end
  
  def setScrollInfo(btyp,mask=SIF_ALL,min=0,max=100,page=100,pos=0)
    si=[4*7,mask,min,max,page,pos,0].pack(WStruct::SCROLLINFO)
    SetScrollInfo.call(self.hWnd,btyp,si,1)
  end
  
  def getScrollInfo(btyp)
    si=[4*7,SIF_ALL,0,0,0,0,0].pack(WStruct::SCROLLINFO)
    GetScrollInfo.call(self.hWnd,btyp,si)
    return si.unpack(WStruct::SCROLLINFO)
  end
  
  public
  
  def vrinit
    super
    stdscrollbarinit
  end
  
  def getScrollRange(bar_type)
    return getScrollInfo(bar_type)[2,3]
  end
  
  def getVScrollRange
    return getScrollRange(SB_VERT)
  end
  
  def getHScrollRange
    return getScrollRange(SB_HORZ)
  end
  
  def setScrollRange(min,max,bar_type)
    setScrollInfo(bar_type,SIF_RANGE,min.to_i,max.to_i)
  end
  
  def setVScrollRange(min,max)
    setScrollRange(min,max,SB_VERT)
  end
  
  def setHScrollRange(min,max)
    setScrollRange(min,max,SB_HORZ)
  end
  
  def scroll_position(bar_type)
    return getScrollInfo(bar_type)[5]
  end
  
  def vscroll_position
    return scroll_position(SB_VERT)
  end
  
  def hscroll_position
    return scroll_position(SB_HORZ)
  end
  
  def setScrollPosition(val,bar_type)
    setScrollInfo(bar_type,SIF_POS,0,0,0,val.to_i)
  end
  
  def vscroll_position=(val)
    setScrollPosition(val,SB_VERT)
  end
  
  def hscroll_position=(val)
    setScrollPosition(val,SB_HORZ)
  end
  
  def scroll_pagesize(bar_type)
    return getScrollInfo(bar_type)[4]
  end
  
  def vscroll_pagesize
    return scroll_pagesize(SB_VERT)
  end
  
  def hscroll_pagesize
    return scroll_pagesize(SB_HORZ)
  end
  
  def setScrollPagesize(p,bar_type)
    setScrollInfo(bar_type,SIF_PAGE,0,0,p.to_i,0)
  end
  
  def vscroll_pagesize=(p)
    setScrollPagesize(p,SB_VERT)
  end
  
  def hscroll_pagesize=(p)
    setScrollPagesize(p,SB_HORZ)
  end
  
  def scroll_smallstep(bar_type)
    res=case bar_type
    when SB_HORZ
      @scroll_smallstep_horz
    when SB_VERT
      @scroll_smallstep_vert
    else
      1
    end
    return res
  end
  
  def vscroll_smallstep
    return scroll_smallstep(SB_VERT)
  end
  
  def hscroll_smallstep
    return scroll_smallstep(SB_HORZ)
  end
  
  def setScrollSmallstep(stp,bar_type)
    case bar_type
    when SB_HORZ
      @scroll_smallstep_horz=stp
    when SB_VERT
      @scroll_smallstep_vert=stp
    end
  end
  
  def vscroll_smallstep=(stp)
    setScrollSmallstep(stp,SB_VERT)
  end
  
  def hscroll_smallstep=(stp)
    setScrollSmallstep(stp,SB_HORZ)
  end
  
  def scroll_longstep(bar_type)
    res=case bar_type
    when SB_HORZ
      @scroll_longstep_horz
    when SB_VERT
      @scroll_longstep_vert
    else
      10
    end
    return res
  end
  
  def vscroll_longstep
    return scroll_longstep(SB_VERT)
  end
  
  def hscroll_longstep
    return scroll_longstep(SB_HORZ)
  end
  
  def setScrollLongstep(stp,bar_type)
    case bar_type
    when SB_HORZ
      @scroll_longstep_horz=stp
    when SB_VERT
      @scroll_longstep_vert=stp
    end
  end
  
  def vscroll_longstep=(stp)
    setScrollLongstep(stp,SB_VERT)
  end
  
  def hscroll_longstep=(stp)
    setScrollLongstep(stp,SB_HORZ)
  end
  
  def self_vrstdscroll(wparam,hwnd,bar_type)
    srl=self
    code=LOWORD(wparam)
    if code==4 then
      pos=HIWORD(wparam)
    else
      pos=srl.scroll_position(bar_type)
    end
    new_pos=case code
    when 0 
      pos-srl.scroll_smallstep(bar_type)
    when 1
      pos+srl.scroll_smallstep(bar_type)
    when 2
      pos-srl.scroll_longstep(bar_type)
    when 3
      pos+srl.scroll_longstep(bar_type)
    when 4
      pos
    else
      nil
    end
    if new_pos
      srl.setScrollPosition(new_pos,bar_type)
    end
    return code
  end
  
  def self_hstdscroll(wparam,hwnd)
    code=self_vrstdscroll(wparam,hwnd,SB_HORZ)
    selfmsg_dispatching(:hscroll,code)
  end
  
  def self_vstdscroll(wparam,hwnd)
    code=self_vrstdscroll(wparam,hwnd,SB_VERT)
    selfmsg_dispatching(:vscroll,code)
  end
end
