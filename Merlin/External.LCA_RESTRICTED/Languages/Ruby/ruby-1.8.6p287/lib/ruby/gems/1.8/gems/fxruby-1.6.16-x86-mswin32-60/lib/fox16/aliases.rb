module Fox
  # Module methods
  alias fxparseHotKey parseHotKey
  alias fxparseAccel parseAccel

  # Constants
  FONTPITCH_DEFAULT		= 0
  FONTPITCH_FIXED		= FXFont::Fixed
  FONTPITCH_VARIABLE		= FXFont::Variable

  FONTHINT_DONTCARE		= 0
  FONTHINT_DECORATIVE		= FXFont::Decorative
  FONTHINT_MODERN		= FXFont::Modern
  FONTHINT_ROMAN		= FXFont::Roman
  FONTHINT_SCRIPT		= FXFont::Script
  FONTHINT_SWISS		= FXFont::Swiss
  FONTHINT_SYSTEM		= FXFont::System
  FONTHINT_X11			= FXFont::X11
  FONTHINT_SCALABLE		= FXFont::Scalable
  FONTHINT_POLYMORPHIC		= FXFont::Polymorphic

  FONTSLANT_DONTCARE		= 0
  FONTSLANT_REVERSE_OBLIQUE	= FXFont::ReverseOblique
  FONTSLANT_REVERSE_ITALIC	= FXFont::ReverseItalic
  FONTSLANT_REGULAR		= FXFont::Straight
  FONTSLANT_ITALIC		= FXFont::Italic
  FONTSLANT_OBLIQUE		= FXFont::Oblique

  FONTWEIGHT_DONTCARE		= 0
  FONTWEIGHT_THIN		= FXFont::Thin
  FONTWEIGHT_EXTRALIGHT		= FXFont::ExtraLight
  FONTWEIGHT_LIGHT		= FXFont::Light
  FONTWEIGHT_NORMAL		= FXFont::Normal
  FONTWEIGHT_REGULAR		= FXFont::Normal
  FONTWEIGHT_MEDIUM		= FXFont::Medium
  FONTWEIGHT_DEMIBOLD		= FXFont::DemiBold
  FONTWEIGHT_BOLD		= FXFont::Bold
  FONTWEIGHT_EXTRABOLD		= FXFont::ExtraBold
  FONTWEIGHT_HEAVY		= FXFont::Black
  FONTWEIGHT_BLACK		= FXFont::Black

  FONTSETWIDTH_DONTCARE		= 0
  FONTSETWIDTH_ULTRACONDENSED	= FXFont::UltraCondensed
  FONTSETWIDTH_EXTRACONDENSED	= FXFont::ExtraCondensed
  FONTSETWIDTH_CONDENSED	= FXFont::Condensed
  FONTSETWIDTH_NARROW		= FXFont::Condensed
  FONTSETWIDTH_COMPRESSED	= FXFont::Condensed
  FONTSETWIDTH_SEMICONDENSED	= FXFont::SemiCondensed
  FONTSETWIDTH_MEDIUM		= FXFont::NonExpanded
  FONTSETWIDTH_NORMAL		= FXFont::NonExpanded
  FONTSETWIDTH_REGULAR		= FXFont::NonExpanded
  FONTSETWIDTH_SEMIEXPANDED	= FXFont::SemiExpanded
  FONTSETWIDTH_EXPANDED		= FXFont::Expanded
  FONTSETWIDTH_WIDE		= FXFont::ExtraExpanded
  FONTSETWIDTH_EXTRAEXPANDED	= FXFont::ExtraExpanded
  FONTSETWIDTH_ULTRAEXPANDED	= FXFont::UltraExpanded

  # Instance methods
  class FX4Splitter
    def topLeft # :nodoc:
      getTopLeft()
    end
    def topRight # :nodoc:
      getTopRight()
    end
    def bottomLeft # :nodoc:
      getBottomLeft()
    end
    def bottomRight # :nodoc:
      getBottomRight()
    end
    def hSplit # :nodoc:
      getHSplit()
    end
    def vSplit # :nodoc:
      getVSplit()
    end
    def hSplit=(s) # :nodoc:
      setHSplit(s)
    end
    def vSplit=(s) # :nodoc:
      setVSplit(s)
    end
    def splitterStyle # :nodoc:
      getSplitterStyle()
    end
    def splitterStyle=(style) # :nodoc:
      setSplitterStyle(style)
    end
    def barSize=(bs) # :nodoc:
      setBarSize(bs)
    end
    def barSize # :nodoc:
      getBarSize()
    end
    def expanded=(ex) # :nodoc:
      setExpanded(ex)
    end
    def expanded # :nodoc:
      getExpanded()
    end
  end
  
  class FXAccelTable
    def hasAccel?(*args) # :nodoc:
      hasAccel(*args)
    end
  end
  
  class FXApp
    def mainloop # :nodoc:
      run
    end
    def appName(*args) # :nodoc:
      getAppName(*args)
    end
    def vendorName(*args) # :nodoc:
      getVendorName(*args)
    end
    def initialized? # :nodoc:
      isInitialized()
    end
    def argc() # :nodoc:
      getArgc()
    end
    def argv() # :nodoc:
      getArgv()
    end
    def display # :nodoc:
      getDisplay()
    end
    def defaultVisual(*args) # :nodoc:
      getDefaultVisual(*args)
    end
    def defaultVisual=(*args) # :nodoc:
      setDefaultVisual(*args)
    end
    def monoVisual(*args) # :nodoc:
      getMonoVisual(*args)
    end
    def rootWindow(*args) # :nodoc:
      getRootWindow(*args)
    end
    def rootWindow=(*args) # :nodoc:
      setRootWindow(*args)
    end
    def cursorWindow(*args) # :nodoc:
      getCursorWindow(*args)
    end
    def modal?(*args) # :nodoc:
      isModal(*args)
    end
    def dragWindow # :nodoc:
      getDragWindow
    end
    def normalFont(*args) # :nodoc:
      getNormalFont(*args)
    end
    def normalFont=(*args) # :nodoc:
      setNormalFont(*args)
    end
    def waitCursor(*args) # :nodoc:
      getWaitCursor(*args)
    end
    def waitCursor=(*args) # :nodoc:
      setWaitCursor(*args)
    end
    def typingSpeed(*args) # :nodoc:
      getTypingSpeed(*args)
    end
    def typingSpeed=(*args) # :nodoc:
      setTypingSpeed(*args)
    end
    def clickSpeed(*args) # :nodoc:
      getClickSpeed(*args)
    end
    def clickSpeed=(*args) # :nodoc:
      setClickSpeed(*args)
    end
    def scrollSpeed(*args) # :nodoc:
      getScrollSpeed(*args)
    end
    def scrollSpeed=(*args) # :nodoc:
      setScrollSpeed(*args)
    end
    def scrollDelay(*args) # :nodoc:
      getScrollDelay(*args)
    end
    def scrollDelay=(*args) # :nodoc:
      setScrollDelay(*args)
    end
    def blinkSpeed(*args) # :nodoc:
      getBlinkSpeed(*args)
    end
    def blinkSpeed=(*args) # :nodoc:
      setBlinkSpeed(*args)
    end
    def animSpeed(*args) # :nodoc:
      getAnimSpeed(*args)
    end
    def animSpeed=(*args) # :nodoc:
      setAnimSpeed(*args)
    end
    def menuPause(*args) # :nodoc:
      getMenuPause(*args)
    end
    def menuPause=(*args) # :nodoc:
      setMenuPause(*args)
    end
    def tooltipPause # :nodoc:
      getTooltipPause()
    end
    def tooltipPause=(*args) # :nodoc:
      setTooltipPause(*args)
    end
    def tooltipTime(*args) # :nodoc:
      getTooltipTime(*args)
    end
    def tooltipTime=(*args) # :nodoc:
      setTooltipTime(*args)
    end
    def dragDelta(*args) # :nodoc:
      getDragDelta(*args)
    end
    def dragDelta=(*args) # :nodoc:
      setDragDelta(*args)
    end
    def wheelLines(*args) # :nodoc:
      getWheelLines(*args)
    end
    def wheelLines=(*args) # :nodoc:
      setWheelLines(*args)
    end
    def borderColor(*args) # :nodoc:
      getBorderColor(*args)
    end
    def borderColor=(*args) # :nodoc:
      setBorderColor(*args)
    end
    def baseColor(*args) # :nodoc:
      getBaseColor(*args)
    end
    def baseColor=(*args) # :nodoc:
      setBaseColor(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def backColor(*args) # :nodoc:
      getBackColor(*args)
    end
    def backColor=(*args) # :nodoc:
      setBackColor(*args)
    end
    def foreColor(*args) # :nodoc:
      getForeColor(*args)
    end
    def foreColor=(*args) # :nodoc:
      setForeColor(*args)
    end
    def selforeColor(*args) # :nodoc:
      getSelforeColor(*args)
    end
    def selforeColor=(*args) # :nodoc:
      setSelforeColor(*args)
    end
    def selbackColor(*args) # :nodoc:
      getSelbackColor(*args)
    end
    def selbackColor=(*args) # :nodoc:
      setSelbackColor(*args)
    end
    def tipforeColor(*args) # :nodoc:
      getTipforeColor(*args)
    end
    def tipforeColor=(*args) # :nodoc:
      setTipforeColor(*args)
    end
    def tipbackColor(*args) # :nodoc:
      getTipbackColor(*args)
    end
    def tipbackColor=(*args) # :nodoc:
      setTipbackColor(*args)
    end
    def selMenuTextColor=(*args) # :nodoc:
      setSelMenuTextColor(*args)
    end
    def selMenuTextColor(*args) # :nodoc:
      getSelMenuTextColor(*args)
    end
    def selMenuBackColor=(*args) # :nodoc:
      setSelMenuBackColor(*args)
    end
    def selMenuBackColor(*args) # :nodoc:
      getSelMenuBackColor(*args)
    end
    def sleepTime(*args) # :nodoc:
      getSleepTime(*args)
    end
    def sleepTime=(*args) # :nodoc:
      setSleepTime(*args)
    end
    def enableThreads # :nodoc:
      self.threadsEnabled = true
    end
    def disableThreads # :nodoc:
      self.threadsEnabled = false
    end
    def translator=(*args) # :nodoc:
      setTranslator(*args)
    end
    def translator # :nodoc:
      getTranslator()
    end
    def windowCount # :nodoc:
      getWindowCount()
    end
  end
  class FXArrowButton
    def state(*args) # :nodoc:
      getState(*args)
    end
    def state=(*args) # :nodoc:
      setState(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def arrowStyle(*args) # :nodoc:
      getArrowStyle(*args)
    end
    def arrowStyle=(*args) # :nodoc:
      setArrowStyle(*args)
    end
    def arrowSize(*args) # :nodoc:
      getArrowSize(*args)
    end
    def arrowSize=(*args) # :nodoc:
      setArrowSize(*args)
    end
    def justify(*args) # :nodoc:
      getJustify(*args)
    end
    def justify=(*args) # :nodoc:
      setJustify(*args)
    end
    def arrowColor(*args) # :nodoc:
      getArrowColor(*args)
    end
    def arrowColor=(*args) # :nodoc:
      setArrowColor(*args)
    end
  end

  class FXBitmapFrame
    def bitmap # :nodoc:
      getBitmap()
    end
    def bitmap=(bmp) # :nodoc:
      setBitmap(bmp)
    end
    def onColor # :nodoc:
      getOnColor()
    end
    def onColor=(clr) # :nodoc:
      setOnColor(clr)
    end
    def offColor # :nodoc:
      getOffColor()
    end
    def offColor=(clr) # :nodoc:
      setOffColor(clr)
    end
    def justify # :nodoc:
      getJustify()
    end
    def justify=(jst) # :nodoc:
      setJustify(jst)
    end
  end

  class FXBitmapView
    def bitmap # :nodoc:
      getBitmap()
    end
    def bitmap=(bmp) # :nodoc:
      setBitmap(bmp)
    end
    def onColor # :nodoc:
      getOnColor()
    end
    def onColor=(clr) # :nodoc:
      setOnColor(clr)
    end
    def offColor # :nodoc:
      getOffColor()
    end
    def offColor=(clr) # :nodoc:
      setOffColor(clr)
    end
    def alignment # :nodoc:
      getAlignment()
    end
    def alignment=(jst) # :nodoc:
      setAlignment(jst)
    end
  end

  class FXButton
    def state=(*args) # :nodoc:
      setState(*args)
    end
    def state(*args) # :nodoc:
      getState(*args)
    end
    def buttonStyle=(*args) # :nodoc:
      setButtonStyle(*args)
    end
    def buttonStyle(*args) # :nodoc:
      getButtonStyle(*args)
    end
  end
  class FXCheckButton
    def checkState # :nodoc:
      getCheckState
    end
    def setCheckState(*args) # :nodoc:
      setCheck(*args)
    end
    def checkState=(*args) # :nodoc:
      setCheck(*args)
    end
    def check # :nodoc:
      getCheck
    end			# deprecated
    def check=(*args) # :nodoc:
      setCheck(*args)
    end	# deprecated
    def checkButtonStyle=(*args) # :nodoc:
      setCheckButtonStyle(*args)
    end
    def checkButtonStyle(*args) # :nodoc:
      getCheckButtonStyle(*args)
    end
    def boxColor(*args) # :nodoc:
      getBoxColor(*args)
    end
    def boxColor=(*args) # :nodoc:
      setBoxColor(*args)
    end
    def checkColor(*args) # :nodoc:
      getCheckColor(*args)
    end
    def checkColor=(*args) # :nodoc:
      setCheckColor(*args)
    end
  end
  class FXColorBar
    def hue=(*args) # :nodoc:
      setHue(*args)
    end
    def hue(*args) # :nodoc:
      getHue(*args)
    end
    def sat=(*args) # :nodoc:
      setSat(*args)
    end
    def sat(*args) # :nodoc:
      getSat(*args)
    end
    def val=(*args) # :nodoc:
      setVal(*args)
    end
    def val(*args) # :nodoc:
      getVal(*args)
    end
    def barStyle=(*args) # :nodoc:
      setBarStyle(*args)
    end
    def barStyle(*args) # :nodoc:
      getBarStyle(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
  end
  class FXColorDialog
    def rgba=(*args) # :nodoc:
      setRGBA(*args)
    end
    def rgba(*args) # :nodoc:
      getRGBA(*args)
    end
    def opaqueOnly?(*args) # :nodoc:
      isOpaqueOnly(*args)
    end
    def opaqueOnly=(*args) # :nodoc:
      setOpaqueOnly(*args)
    end
  end
  class FXColorSelector
    def rgba=(*args) # :nodoc:
      setRGBA(*args)
    end
    def rgba(*args) # :nodoc:
      getRGBA(*args)
    end
    def opaqueOnly=(*args) # :nodoc:
      setOpaqueOnly(*args)
    end
    def opaqueOnly?(*args) # :nodoc:
      isOpaqueOnly(*args)
    end
  end
  class FXColorWell
    def rgba=(*args) # :nodoc:
      setRGBA(*args)
    end
    def rgba(*args) # :nodoc:
      getRGBA(*args)
    end
    def opaqueOnly=(*args) # :nodoc:
      setOpaqueOnly(*args)
    end
    def opaqueOnly?(*args) # :nodoc:
      isOpaqueOnly(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
  end
  class FXColorWheel
    def hue=(*args) # :nodoc:
      setHue(*args)
    end
    def hue(*args) # :nodoc:
      getHue(*args)
    end
    def sat=(*args) # :nodoc:
      setSat(*args)
    end
    def sat(*args) # :nodoc:
      getSat(*args)
    end
    def val=(*args) # :nodoc:
      setVal(*args)
    end
    def val(*args) # :nodoc:
      getVal(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
  end
  class FXComboBox
    def editable?(*args) # :nodoc:
      isEditable(*args)
    end
    def editable=(*args) # :nodoc:
      setEditable(*args)
    end
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def numColumns=(*args) # :nodoc:
      setNumColumns(*args)
    end
    def numColumns(*args) # :nodoc:
      getNumColumns(*args)
    end
    def numItems(*args) # :nodoc:
      getNumItems(*args)
    end
    def numVisible(*args) # :nodoc:
      getNumVisible(*args)
    end
    def numVisible=(*args) # :nodoc:
      setNumVisible(*args)
    end
    def currentItem=(*args) # :nodoc:
      setCurrentItem(*args)
    end
    def currentItem(*args) # :nodoc:
      getCurrentItem(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def comboStyle=(*args) # :nodoc:
      setComboStyle(*args)
    end
    def comboStyle(*args) # :nodoc:
      getComboStyle(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def paneShown?(*args) # :nodoc:
      isPaneShown(*args)
    end
    def itemCurrent?(*args) # :nodoc:
      isItemCurrent(*args)
    end
  end
  class FXCursor
    def width(*args) # :nodoc:
      getWidth(*args)
    end
    def height(*args) # :nodoc:
      getHeight(*args)
    end
    def hotX(*args) # :nodoc:
      getHotX(*args)
    end
    def hotY(*args) # :nodoc:
      getHotY(*args)
    end
    def color? # :nodoc:
      isColor()
    end
  end
  class FXDataTarget
    def target(*args) # :nodoc:
      getTarget(*args)
    end
    def target=(*args) # :nodoc:
      setTarget(*args)
    end
    def selector(*args) # :nodoc:
      getSelector(*args)
    end
    def selector=(*args) # :nodoc:
      setSelector(*args)
    end
  end
  class FXDataTarget
    def value(*args) # :nodoc:
      getValue(*args)
    end
    def value=(*args) # :nodoc:
      setValue(*args)
    end
  end
  class FXDC
    def app(*args) # :nodoc:
      getApp(*args)
    end
    def foreground(*args) # :nodoc:
      getForeground(*args)
    end
    def foreground=(*args) # :nodoc:
      setForeground(*args)
    end
    def background(*args) # :nodoc:
      getBackground(*args)
    end
    def background=(*args) # :nodoc:
      setBackground(*args)
    end
    def dashPattern(*args) # :nodoc:
      getDashPattern(*args)
    end
    def dashOffset(*args) # :nodoc:
      getDashOffset(*args)
    end
    def lineWidth(*args) # :nodoc:
      getLineWidth(*args)
    end
    def lineWidth=(*args) # :nodoc:
      setLineWidth(*args)
    end
    def lineCap(*args) # :nodoc:
      getLineCap(*args)
    end
    def lineCap=(*args) # :nodoc:
      setLineCap(*args)
    end
    def lineJoin(*args) # :nodoc:
      getLineJoin(*args)
    end
    def lineJoin=(*args) # :nodoc:
      setLineJoin(*args)
    end
    def lineStyle(*args) # :nodoc:
      getLineStyle(*args)
    end
    def lineStyle=(*args) # :nodoc:
      setLineStyle(*args)
    end
    def fillStyle(*args) # :nodoc:
      getFillStyle(*args)
    end
    def fillStyle=(*args) # :nodoc:
      setFillStyle(*args)
    end
    def fillRule(*args) # :nodoc:
      getFillRule(*args)
    end
    def fillRule=(*args) # :nodoc:
      setFillRule(*args)
    end
    def function(*args) # :nodoc:
      getFunction(*args)
    end
    def function=(*args) # :nodoc:
      setFunction(*args)
    end
    def tile=(*args) # :nodoc:
      setTile(*args)
    end
    def tile(*args) # :nodoc:
      getTile(*args)
    end
    def stipple=(*args) # :nodoc:
      setStipple(*args)
    end
    def stipple
      stippleBitmap.nil? ? stipplePattern : stippleBitmap
    end
    def stippleBitmap(*args) # :nodoc:
      getStippleBitmap(*args)
    end
    def stipplePattern(*args) # :nodoc:
      getStipplePattern(*args)
    end
    def clipRegion=(*args) # :nodoc:
      setClipRegion(*args)
    end
    def clipRectangle=(*args) # :nodoc:
      setClipRectangle(*args)
    end
    def clipRectangle(*args) # :nodoc:
      getClipRectangle(*args)
    end
    def clipX(*args) # :nodoc:
      getClipX(*args)
    end
    def clipY(*args) # :nodoc:
      getClipY(*args)
    end
    def clipWidth(*args) # :nodoc:
      getClipWidth(*args)
    end
    def clipHeight(*args) # :nodoc:
      getClipHeight(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
  end
  class FXDelegator
    def delegate(*args) # :nodoc:
      getDelegate(*args)
    end
    def delegate=(*args) # :nodoc:
      setDelegate(*args)
    end
  end
  class FXDial
    def range(*args) # :nodoc:
      getRange(*args)
    end
    def range=(*args) # :nodoc:
      setRange(*args)
    end
    def value(*args) # :nodoc:
      getValue(*args)
    end
    def value=(*args) # :nodoc:
      setValue(*args)
    end
    def revolutionIncrement=(*args) # :nodoc:
      setRevolutionIncrement(*args)
    end
    def revolutionIncrement(*args) # :nodoc:
      getRevolutionIncrement(*args)
    end
    def notchSpacing=(*args) # :nodoc:
      setNotchSpacing(*args)
    end
    def notchSpacing(*args) # :nodoc:
      getNotchSpacing(*args)
    end
    def notchOffset=(*args) # :nodoc:
      setNotchOffset(*args)
    end
    def notchOffset(*args) # :nodoc:
      getNotchOffset(*args)
    end
    def dialStyle=(*args) # :nodoc:
      setDialStyle(*args)
    end
    def dialStyle(*args) # :nodoc:
      getDialStyle(*args)
    end
    def notchColor=(*args) # :nodoc:
      setNotchColor(*args)
    end
    def notchColor(*args) # :nodoc:
      getNotchColor(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
  end
  class FXDict
    def marked? # :nodoc:
      mark
    end
    def size # :nodoc:
      length
    end
    def include?(*args) # :nodoc:
      has_key?(*args)
    end
    def member?(*args) # :nodoc:
      has_key?(*args)
    end
  end
  class FXDirBox
    def associations # :nodoc:
      getAssociations()
    end
    def associations=(assoc) # :nodoc:
      setAssociations(assoc)
    end
    def directory(*args) # :nodoc:
      getDirectory(*args)
    end
    def directory=(*args) # :nodoc:
      setDirectory(*args)
    end
  end
  class FXDirDialog
    def directory(*args) # :nodoc:
      getDirectory(*args)
    end
    def directory=(*args) # :nodoc:
      setDirectory(*args)
    end
    def filesShown=(*args) # :nodoc:
      setShowFiles(*args)
    end
    def filesShown? # :nodoc:
      getShowFiles()
    end
    def hiddenFilesShown=(*args) # :nodoc:
      setShowHiddenFiles(*args)
    end
    def hiddenFilesShown? # :nodoc:
      getShowHiddenFiles()
    end
    def matchMode # :nodoc:
      getMatchMode()
    end
    def matchMode=(m) # :nodoc:
      setMatchMode(m)
    end
    def dirBoxStyle(*args) # :nodoc:
      getDirBoxStyle(*args)
    end
    def dirBoxStyle=(*args) # :nodoc:
      setDirBoxStyle(*args)
    end
  end
  class FXDirItem
    def directory? # :nodoc:
      isDirectory()
    end
    def executable? # :nodoc:
      isExecutable()
    end
    def symlink? # :nodoc:
      isSymlink()
    end
    def chardev? # :nodoc:
      isChardev()
    end
    def blockdev? # :nodoc:
      isBlockdev()
    end
    def fifo? # :nodoc:
      isFifo()
    end
    def socket? # :nodoc:
      isSocket()
    end
    def assoc # :nodoc:
      getAssoc()
    end
    def size # :nodoc:
      getSize()
    end
    def date # :nodoc:
      getDate()
    end
  end
  class FXDirList
    def currentFile(*args) # :nodoc:
      getCurrentFile(*args)
    end
    def currentFile=(*args) # :nodoc:
      setCurrentFile(*args)
    end
    def directory(*args) # :nodoc:
      getDirectory(*args)
    end
    def directory=(*args) # :nodoc:
      setDirectory(*args)
    end
    def pattern(*args) # :nodoc:
      getPattern(*args)
    end
    def pattern=(*args) # :nodoc:
      setPattern(*args)
    end
    def matchMode(*args) # :nodoc:
      getMatchMode(*args)
    end
    def matchMode=(*args) # :nodoc:
      setMatchMode(*args)
    end
    def filesShown=(*args) # :nodoc:
      setShowFiles(*args)
    end
    def filesShown? # :nodoc:
      getShowFiles()
    end
    def hiddenFilesShown=(*args) # :nodoc:
      setShowHiddenFiles(*args)
    end
    def hiddenFilesShown? # :nodoc:
      getShowHiddenFiles()
    end
    def associations(*args) # :nodoc:
      getAssociations(*args)
    end
    def associations=(*args) # :nodoc:
      setAssociations(*args)
    end
    def itemDirectory?(*args) # :nodoc:
      isItemDirectory(*args)
    end
    def itemFile?(*args) # :nodoc:
      isItemFile(*args)
    end
    def itemExecutable?(*args) # :nodoc:
      isItemExecutable(*args)
    end
    def itemPathname(*args) # :nodoc:
      getItemPathname(*args)
    end
    def pathnameItem(*args) # :nodoc:
      getPathnameItem(*args)
    end
  end
  class FXDirSelector
    def directory(*args) # :nodoc:
      getDirectory(*args)
    end
    def directory=(*args) # :nodoc:
      setDirectory(*args)
    end
    def filesShown=(*args) # :nodoc:
      setShowFiles(*args)
    end
    def filesShown? # :nodoc:
      getShowFiles()
    end
    def hiddenFilesShown=(*args) # :nodoc:
      setShowHiddenFiles(*args)
    end
    def hiddenFilesShown? # :nodoc:
      getShowHiddenFiles()
    end
    def matchMode # :nodoc:
      getMatchMode()
    end
    def matchMode=(m) # :nodoc:
      setMatchMode(m)
    end
    def dirBoxStyle(*args) # :nodoc:
      getDirBoxStyle(*args)
    end
    def dirBoxStyle=(*args) # :nodoc:
      setDirBoxStyle(*args)
    end
  end
  class FXDocument
    def modified?(*args) # :nodoc:
      isModified(*args)
    end
    def modified=(*args) # :nodoc:
      setModified(*args)
    end
    def title(*args) # :nodoc:
      getTitle(*args)
    end
    def title=(*args) # :nodoc:
      setTitle(*args)
    end
    def filename(*args) # :nodoc:
      getFilename(*args)
    end
    def filename=(*args) # :nodoc:
      setFilename(*args)
    end
  end
  class FXDragCorner
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
  end
  class FXDrawable
    def width(*args) # :nodoc:
      getWidth(*args)
    end
    def height(*args) # :nodoc:
      getHeight(*args)
    end
    def visual(*args) # :nodoc:
      getVisual(*args)
    end
    def visual=(*args) # :nodoc:
      setVisual(*args)
    end
  end
  class FXDriveBox
    def associations # :nodoc:
      getAssociations()
    end
    def associations=(assoc) # :nodoc:
      setAssociations(assoc)
    end
    def drive(*args) # :nodoc:
      getDrive(*args)
    end
    def drive=(*args) # :nodoc:
      setDrive(*args)
    end
  end
  class FXEvent
    def moved?(*args) # :nodoc:
      moved(*args)
    end
    def synthetic?(*args) # :nodoc:
      synthetic(*args)
    end
  end
  class FXFileDialog
    def filename=(*args) # :nodoc:
      setFilename(*args)
    end
    def filename(*args) # :nodoc:
      getFilename(*args)
    end
    def filenames # :nodoc:
      getFilenames()
    end
    def pattern=(*args) # :nodoc:
      setPattern(*args)
    end
    def pattern(*args) # :nodoc:
      getPattern(*args)
    end
    def patternList(*args) # :nodoc:
      getPatternList(*args)
    end
    def patternList=(*args) # :nodoc:
      setPatternList(*args)
    end
    def currentPattern=(*args) # :nodoc:
      setCurrentPattern(*args)
    end
    def currentPattern(*args) # :nodoc:
      getCurrentPattern(*args)
    end
    def directory=(*args) # :nodoc:
      setDirectory(*args)
    end
    def directory(*args) # :nodoc:
      getDirectory(*args)
    end
    def itemSpace=(*args) # :nodoc:
      setItemSpace(*args)
    end
    def itemSpace(*args) # :nodoc:
      getItemSpace(*args)
    end
    def fileBoxStyle=(*args) # :nodoc:
      setFileBoxStyle(*args)
    end
    def fileBoxStyle(*args) # :nodoc:
      getFileBoxStyle(*args)
    end
    def selectMode=(*args) # :nodoc:
      setSelectMode(*args)
    end
    def selectMode(*args) # :nodoc:
      getSelectMode(*args)
    end
    def matchMode # :nodoc:
      getMatchMode()
    end
    def matchMode=(m) # :nodoc:
      setMatchMode(m)
    end
    def hiddenFilesShown=(*args) # :nodoc:
      setShowHiddenFiles(*args)
    end
    def hiddenFilesShown? # :nodoc:
      getShowHiddenFiles()
    end
    def readOnlyShown=(*args) # :nodoc:
      showReadOnly(*args)
    end
    def readOnlyShown? # :nodoc:
      shownReadOnly()
    end
    def readOnly=(*args) # :nodoc:
      setReadOnly(*args)
    end
    def readOnly? # :nodoc:
      getReadOnly()
    end
    def allowsPatternEntry=(*args) # :nodoc:
      setAllowPatternEntry(*args)
    end
    def allowsPatternEntry? # :nodoc:
      getAllowPatternEntry()
    end
    def imagesShown=(shown) # :nodoc:
      setShowImages(shown)
    end
    def imagesShown? # :nodoc:
      getShowImages()
    end
    def imageSize=(size) # :nodoc:
      setImageSize(size)
    end
    def imageSize # :nodoc:
      getImageSize
    end
  end
  class FXIcon
    def transparentColor(*args) # :nodoc:
      getTransparentColor(*args)
    end
    def transparentColor=(*args) # :nodoc:
      setTransparentColor(*args)
    end
  end
  class FXFileItem
    def file? # :nodoc:
      isFile()
    end
    def directory? # :nodoc:
      isDirectory()
    end
    def share? # :nodoc:
      isShare()
    end
    def executable? # :nodoc:
      isExecutable()
    end
    def symlink? # :nodoc:
      isSymlink()
    end
    def chardev? # :nodoc:
      isChardev()
    end
    def blockdev? # :nodoc:
      isBlockdev()
    end
    def fifo? # :nodoc:
      isFifo()
    end
    def socket? # :nodoc:
      isSocket()
    end
    def assoc(*args) # :nodoc:
      getAssoc(*args)
    end
    def size(*args) # :nodoc:
      getSize(*args)
    end
    def date(*args) # :nodoc:
      getDate(*args)
    end
  end
  class FXFileList
    def currentFile=(*args) # :nodoc:
      setCurrentFile(*args)
    end
    def currentFile(*args) # :nodoc:
      getCurrentFile(*args)
    end
    def directory=(*args) # :nodoc:
      setDirectory(*args)
    end
    def directory(*args) # :nodoc:
      getDirectory(*args)
    end
    def pattern=(*args) # :nodoc:
      setPattern(*args)
    end
    def pattern(*args) # :nodoc:
      getPattern(*args)
    end
    def itemDirectory?(*args) # :nodoc:
      isItemDirectory(*args)
    end
    def itemShare?(*args) # :nodoc:
      isItemShare(*args)
    end
    def itemFile?(*args) # :nodoc:
      isItemFile(*args)
    end
    def itemExecutable?(*args) # :nodoc:
      isItemExecutable(*args)
    end
    def itemFilename(*args) # :nodoc:
      getItemFilename(*args)
    end
    def itemPathname(*args) # :nodoc:
      getItemPathname(*args)
    end
    def itemAssoc(*args) # :nodoc:
      getItemAssoc(*args)
    end
    def matchMode(*args) # :nodoc:
      getMatchMode(*args)
    end
    def matchMode=(*args) # :nodoc:
      setMatchMode(*args)
    end
    def hiddenFilesShown?(*args) # :nodoc:
      getHiddenFilesShown(*args)
    end
    def hiddenFilesShown=(*args) # :nodoc:
      setHiddenFilesShown(*args)
    end
    def onlyDirectoriesShown?(*args) # :nodoc:
      getOnlyDirectoriesShown(*args)
    end
    def onlyDirectoriesShown=(*args) # :nodoc:
      setOnlyDirectoriesShown(*args)
    end
    def associations=(*args) # :nodoc:
      setAssociations(*args)
    end
    def associations(*args) # :nodoc:
      getAssociations(*args)
    end
    def onlyFilesShown? # :nodoc:
      showOnlyFiles()
    end
    def onlyFilesShown=(*args) # :nodoc:
      showOnlyFiles(*args)
    end
    def imagesShown=(shown) # :nodoc:
      setShowImages(shown)
    end
    def imagesShown? # :nodoc:
      getShowImages()
    end
    def imageSize=(size) # :nodoc:
      setImageSize(size)
    end
    def imageSize # :nodoc:
      getImageSize
    end
  end
  class FXFileSelector
    def filename=(*args) # :nodoc:
      setFilename(*args)
    end
    def filename(*args) # :nodoc:
      getFilename(*args)
    end
    def pattern=(*args) # :nodoc:
      setPattern(*args)
    end
    def pattern(*args) # :nodoc:
      getPattern(*args)
    end
    def patternList=(*args) # :nodoc:
      setPatternList(*args)
    end
    def patternList(*args) # :nodoc:
      getPatternList(*args)
    end
    def currentPattern=(*args) # :nodoc:
      setCurrentPattern(*args)
    end
    def currentPattern(*args) # :nodoc:
      getCurrentPattern(*args)
    end
    def directory=(*args) # :nodoc:
      setDirectory(*args)
    end
    def directory(*args) # :nodoc:
      getDirectory(*args)
    end
    def itemSpace=(*args) # :nodoc:
      setItemSpace(*args)
    end
    def itemSpace(*args) # :nodoc:
      getItemSpace(*args)
    end
    def fileBoxStyle=(*args) # :nodoc:
      setFileBoxStyle(*args)
    end
    def fileBoxStyle(*args) # :nodoc:
      getFileBoxStyle(*args)
    end
    def selectMode=(*args) # :nodoc:
      setSelectMode(*args)
    end
    def selectMode(*args) # :nodoc:
      getSelectMode(*args)
    end
    def readOnlyShown=(*args) # :nodoc:
      setReadOnlyShown(*args)
    end
    def readOnlyShown?(*args) # :nodoc:
      getReadOnlyShown(*args)
    end
    def readOnly=(*args) # :nodoc:
      setReadOnly(*args)
    end
    def readOnly?(*args) # :nodoc:
      getReadOnly(*args)
    end
    def allowsPatternEntry=(*args) # :nodoc:
      setAllowPatternEntry(*args)
    end
    def allowsPatternEntry? # :nodoc:
      getAllowPatternEntry()
    end
    def hiddenFilesShown=(*args) # :nodoc:
      setShowHiddenFiles(*args)
    end
    def hiddenFilesShown? # :nodoc:
      getShowHiddenFiles()
    end
    def imagesShown=(shown) # :nodoc:
      setShowImages(shown)
    end
    def imagesShown? # :nodoc:
      getShowImages()
    end
    def imageSize=(size) # :nodoc:
      setImageSize(size)
    end
    def imageSize # :nodoc:
      getImageSize
    end
  end
  
  class FXFoldingItem
    def parent(*args) # :nodoc:
      getParent(*args)
    end
    def next(*args) # :nodoc:
      getNext(*args)
    end
    def prev(*args) # :nodoc:
      getPrev(*args)
    end
    def first(*args) # :nodoc:
      getFirst(*args)
    end
    def last(*args) # :nodoc:
      getLast(*args)
    end
    def below(*args) # :nodoc:
      getBelow(*args)
    end
    def above(*args) # :nodoc:
      getAbove(*args)
    end
    def numChildren(*args) # :nodoc:
      getNumChildren(*args)
    end
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def openIcon=(*args) # :nodoc:
      setOpenIcon(*args)
    end
    def openIcon(*args) # :nodoc:
      getOpenIcon(*args)
    end
    def closedIcon=(*args) # :nodoc:
      setClosedIcon(*args)
    end
    def closedIcon(*args) # :nodoc:
      getClosedIcon(*args)
    end
    def data=(*args) # :nodoc:
      setData(*args)
    end
    def data(*args) # :nodoc:
      getData(*args)
    end
    def selected=(*args) # :nodoc:
      setSelected(*args)
    end
    def selected?(*args) # :nodoc:
      isSelected(*args)
    end
    def opened=(*args) # :nodoc:
      setOpened(*args)
    end
    def opened?(*args) # :nodoc:
      isOpened(*args)
    end
    def expanded=(*args) # :nodoc:
      setExpanded(*args)
    end
    def expanded?(*args) # :nodoc:
      isExpanded(*args)
    end
    def enabled=(*args) # :nodoc:
      setEnabled(*args)
    end
    def enabled?(*args) # :nodoc:
      isEnabled(*args)
    end
    def draggable=(*args) # :nodoc:
      setDraggable(*args)
    end
    def draggable?(*args) # :nodoc:
      isDraggable(*args)
    end
    def childOf?(*args) # :nodoc:
      isChildOf(*args)
    end
    def parentOf?(*args) # :nodoc:
      isParentOf(*args)
    end
  end

  class FXFoldingList
    def header # :nodoc:
      getHeader()
    end
    def numHeaders # :nodoc:
      getNumHeaders()
    end
    def itemSelected?(*args) # :nodoc:
      isItemSelected(*args)
    end
    def itemCurrent?(*args) # :nodoc:
      isItemCurrent(*args)
    end
    def itemVisible?(*args) # :nodoc:
      isItemVisible(*args)
    end
    def itemOpened?(*args) # :nodoc:
      isItemOpened(*args)
    end
    def itemExpanded?(*args) # :nodoc:
      isItemExpanded(*args)
    end
    def itemLeaf?(*args) # :nodoc:
      isItemLeaf(*args)
    end
    def itemEnabled?(*args) # :nodoc:
      isItemEnabled(*args)
    end
    def numItems(*args) # :nodoc:
      getNumItems(*args)
    end
    def numVisible(*args) # :nodoc:
      getNumVisible(*args)
    end
    def numVisible=(*args) # :nodoc:
      setNumVisible(*args)
    end
    def firstItem(*args) # :nodoc:
      getFirstItem(*args)
    end
    def lastItem(*args) # :nodoc:
      getLastItem(*args)
    end
    def currentItem=(*args) # :nodoc:
      setCurrentItem(*args)
    end
    def currentItem(*args) # :nodoc:
      getCurrentItem(*args)
    end
    def anchorItem=(*args) # :nodoc:
      setAnchorItem(*args)
    end
    def anchorItem(*args) # :nodoc:
      getAnchorItem(*args)
    end
    def cursorItem(*args) # :nodoc:
      getCursorItem(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def indent=(*args) # :nodoc:
      setIndent(*args)
    end
    def indent(*args) # :nodoc:
      getIndent(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def lineColor(*args) # :nodoc:
      getLineColor(*args)
    end
    def lineColor=(*args) # :nodoc:
      setLineColor(*args)
    end
    def listStyle(*args) # :nodoc:
      getListStyle(*args)
    end
    def listStyle=(*args) # :nodoc:
      setListStyle(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
  end

  class FXFont
    def name(*args) # :nodoc:
      getName(*args)
    end
    def actualName # :nodoc:
      getActualName()
    end
    def size(*args) # :nodoc:
      getSize(*args)
    end
    def actualSize # :nodoc:
      getActualSize()
    end
    def weight(*args) # :nodoc:
      getWeight(*args)
    end
    def actualWeight # :nodoc:
      getActualWeight()
    end
    def slant(*args) # :nodoc:
      getSlant(*args)
    end
    def actualSlant # :nodoc:
      getActualSlant()
    end
    def encoding(*args) # :nodoc:
      getEncoding(*args)
    end
    def actualEncoding # :nodoc:
      getActualEncoding()
    end
    def setWidth(*args) # :nodoc:
      getSetWidth(*args)
    end
    def actualSetWidth # :nodoc:
      getActualSetWidth()
    end
    def hints(*args) # :nodoc:
      getHints(*args)
    end
    def fontDesc(*args) # :nodoc:
      getFontDesc(*args)
    end
    def fontDesc=(*args) # :nodoc:
      setFontDesc(*args)
    end
    def minChar(*args) # :nodoc:
      getMinChar(*args)
    end
    def maxChar(*args) # :nodoc:
      getMaxChar(*args)
    end
    def fontWidth(*args) # :nodoc:
      getFontWidth(*args)
    end
    def fontHeight(*args) # :nodoc:
      getFontHeight(*args)
    end
    def fontAscent(*args) # :nodoc:
      getFontAscent(*args)
    end
    def fontDescent(*args) # :nodoc:
      getFontDescent(*args)
    end
    def fontLeading(*args) # :nodoc:
      getFontLeading(*args)
    end
    def fontSpacing(*args) # :nodoc:
      getFontSpacing(*args)
    end
    alias hasChar? hasChar
  end
  class FXFontDialog
    def fontSelection(*args) # :nodoc:
      getFontSelection(*args)
    end
    def fontSelection=(*args) # :nodoc:
      setFontSelection(*args)
    end
  end
  class FXFontSelector
    def fontSelection(*args) # :nodoc:
      getFontSelection(*args)
    end
    def fontSelection=(*args) # :nodoc:
      setFontSelection(*args)
    end
  end
  class FXFrame
    def frameStyle(*args) # :nodoc:
      getFrameStyle(*args)
    end
    def frameStyle=(*args) # :nodoc:
      setFrameStyle(*args)
    end
    def borderWidth(*args) # :nodoc:
      getBorderWidth(*args)
    end
    def padTop(*args) # :nodoc:
      getPadTop(*args)
    end
    def padTop=(*args) # :nodoc:
      setPadTop(*args)
    end
    def padBottom(*args) # :nodoc:
      getPadBottom(*args)
    end
    def padBottom=(*args) # :nodoc:
      setPadBottom(*args)
    end
    def padLeft(*args) # :nodoc:
      getPadLeft(*args)
    end
    def padLeft=(*args) # :nodoc:
      setPadLeft(*args)
    end
    def padRight(*args) # :nodoc:
      getPadRight(*args)
    end
    def padRight=(*args) # :nodoc:
      setPadRight(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def borderColor(*args) # :nodoc:
      getBorderColor(*args)
    end
    def borderColor=(*args) # :nodoc:
      setBorderColor(*args)
    end
    def baseColor(*args) # :nodoc:
      getBaseColor(*args)
    end
    def baseColor=(*args) # :nodoc:
      setBaseColor(*args)
    end
  end

  class FXGLCanvas
    def context # :nodoc:
      getContext()
    end
    def current? # :nodoc:
      isCurrent()
    end
    def isShared # :nodoc:
      shared?
    end
  end

  class FXGLContext
    def shared? # :nodoc:
      isShared()
    end
    def visual(*args) # :nodoc:
      getVisual(*args)
    end
  end
  class FXGLShape
    def position # :nodoc:
      getPosition
    end
    def position=(*args) # :nodoc:
      setPosition(*args)
    end
    def range=(*args) # :nodoc:
      setRange(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
  end
  class FXGLViewer
    def viewport(*args) # :nodoc:
      getViewport(*args)
    end
    def material(*args) # :nodoc:
      getMaterial(*args)
    end
    def material=(*args) # :nodoc:
      setMaterial(*args)
    end
    def fieldOfView=(*args) # :nodoc:
      setFieldOfView(*args)
    end
    def fieldOfView(*args) # :nodoc:
      getFieldOfView(*args)
    end
    def zoom=(*args) # :nodoc:
      setZoom(*args)
    end
    def zoom(*args) # :nodoc:
      getZoom(*args)
    end
    def distance=(*args) # :nodoc:
      setDistance(*args)
    end
    def distance(*args) # :nodoc:
      getDistance(*args)
    end
    def scale=(*args) # :nodoc:
      setScale(*args)
    end
    def scale(*args) # :nodoc:
      getScale(*args)
    end
    def orientation=(*args) # :nodoc:
      setOrientation(*args)
    end
    def orientation(*args) # :nodoc:
      getOrientation(*args)
    end
    def center=(*args) # :nodoc:
      setCenter(*args)
    end
    def center(*args) # :nodoc:
      getCenter(*args)
    end
    def eyeVector(*args) # :nodoc:
      getEyeVector(*args)
    end
    def eyePosition(*args) # :nodoc:
      getEyePosition(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def transform(*args) # :nodoc:
      getTransform(*args)
    end
    def invTransform(*args) # :nodoc:
      getInvTransform(*args)
    end
    def scene=(*args) # :nodoc:
      setScene(*args)
    end
    def scene(*args) # :nodoc:
      getScene(*args)
    end
    def selection=(*args) # :nodoc:
      setSelection(*args)
    end
    def selection(*args) # :nodoc:
      getSelection(*args)
    end
    def projection=(*args) # :nodoc:
      setProjection(*args)
    end
    def projection(*args) # :nodoc:
      getProjection(*args)
    end
    def backgroundColor=(*args) # :nodoc:
      setBackgroundColor(*args)
    end
    def backgroundColor(*args) # :nodoc:
      getBackgroundColor(*args)
    end
    def ambientColor=(*args) # :nodoc:
      setAmbientColor(*args)
    end
    def ambientColor(*args) # :nodoc:
      getAmbientColor(*args)
    end
    def zSortFunc=(*args) # :nodoc:
      setZSortFunc(*args)
    end
    def zSortFunc(*args) # :nodoc:
      getZSortFunc(*args)
    end
    def maxHits=(*args) # :nodoc:
      setMaxHits(*args)
    end
    def maxHits(*args) # :nodoc:
      getMaxHits(*args)
    end
    def turboMode(*args) # :nodoc:
      getTurboMode(*args)
    end
    def turboMode=(*args) # :nodoc:
      setTurboMode(*args)
    end
    def doesTurbo? # :nodoc:
      doesTurbo()
    end
    def light(*args) # :nodoc:
      getLight(*args)
    end
    def light=(*args) # :nodoc:
      setLight(*args)
    end
  end
  class FXGLVisual
    def FXGLVisual.supported?(*args) # :nodoc:
      FXGLVisual.supported(*args)
    end
    def redSize(*args) # :nodoc:
      getRedSize(*args)
    end
    def greenSize(*args) # :nodoc:
      getGreenSize(*args)
    end
    def blueSize(*args) # :nodoc:
      getBlueSize(*args)
    end
    def alphaSize(*args) # :nodoc:
      getAlphaSize(*args)
    end
    def depthSize(*args) # :nodoc:
      getDepthSize(*args)
    end
    def stencilSize(*args) # :nodoc:
      getStencilSize(*args)
    end
    def accumRedSize(*args) # :nodoc:
      getAccumRedSize(*args)
    end
    def accumGreenSize(*args) # :nodoc:
      getAccumGreenSize(*args)
    end
    def accumBlueSize(*args) # :nodoc:
      getAccumBlueSize(*args)
    end
    def accumAlphaSize(*args) # :nodoc:
      getAccumAlphaSize(*args)
    end
    def redSize=(*args) # :nodoc:
      setRedSize(*args)
    end
    def greenSize=(*args) # :nodoc:
      setGreenSize(*args)
    end
    def blueSize=(*args) # :nodoc:
      setBlueSize(*args)
    end
    def alphaSize=(*args) # :nodoc:
      setAlphaSize(*args)
    end
    def depthSize=(*args) # :nodoc:
      setDepthSize(*args)
    end
    def stencilSize=(*args) # :nodoc:
      setStencilSize(*args)
    end
    def accumRedSize=(*args) # :nodoc:
      setAccumRedSize(*args)
    end
    def accumGreenSize=(*args) # :nodoc:
      setAccumGreenSize(*args)
    end
    def accumBlueSize=(*args) # :nodoc:
      setAccumBlueSize(*args)
    end
    def accumAlphaSize=(*args) # :nodoc:
      getAccumAlphaSize(*args)
    end
    def actualRedSize(*args) # :nodoc:
      getActualRedSize(*args)
    end
    def actualGreenSize(*args) # :nodoc:
      getActualGreenSize(*args)
    end
    def actualBlueSize(*args) # :nodoc:
      getActualBlueSize(*args)
    end
    def actualAlphaSize(*args) # :nodoc:
      getActualAlphaSize(*args)
    end
    def actualDepthSize(*args) # :nodoc:
      getActualDepthSize(*args)
    end
    def actualStencilSize(*args) # :nodoc:
      getActualStencilSize(*args)
    end
    def actualAccumRedSize(*args) # :nodoc:
      getActualAccumRedSize(*args)
    end
    def actualAccumGreenSize(*args) # :nodoc:
      getActualAccumGreenSize(*args)
    end
    def actualAccumBlueSize(*args) # :nodoc:
      getActualAccumBlueSize(*args)
    end
    def actualAccumAlphaSize(*args) # :nodoc:
      getActualAccumAlphaSize(*args)
    end
    def doubleBuffer?(*args) # :nodoc:
      isDoubleBuffer(*args)
    end
    def doubleBuffered?(*args) # :nodoc:
      isDoubleBuffer(*args)
    end
    def stereo?(*args) # :nodoc:
      isStereo(*args)
    end
    def accelerated?(*args) # :nodoc:
      isAccelerated(*args)
    end
    def bufferSwapCopy? # :nodoc:
      isBufferSwapCopy()
    end
  end
  class FXGradientBar
    def numSegments # :nodoc:
      getNumSegments
    end
    def gradients=(*args) # :nodoc:
      setGradients(*args)
    end
    def gradients # :nodoc:
      getGradients
    end
    def currentSegment=(*args) # :nodoc:
      setCurrentSegment(*args)
    end
    def currentSegment # :nodoc:
      getCurrentSegment
    end
    def anchorSegment=(*args) # :nodoc:
      setAnchorSegment(*args)
    end
    def anchorSegment # :nodoc:
      getAnchorSegment
    end
    def segmentSelected?(*args) # :nodoc:
      isSegmentSelected(*args)
    end
    def barStyle # :nodoc:
      getBarStyle
    end
    def barStyle=(*args) # :nodoc:
      setBarStyle(*args)
    end
    def selectColor # :nodoc:
      getSelectColor
    end
    def selectColor=(*args) # :nodoc:
      setSelectColor(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText # :nodoc:
      getHelpText
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText # :nodoc:
      getTipText
    end
  end
  class FXGroupBox
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def groupBoxStyle=(*args) # :nodoc:
      setGroupBoxStyle(*args)
    end
    def groupBoxStyle(*args) # :nodoc:
      getGroupBoxStyle(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
  end
  class FXHeaderItem
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def icon(*args) # :nodoc:
      getIcon(*args)
    end
    def icon=(*args) # :nodoc:
      setIcon(*args)
    end
    def data(*args) # :nodoc:
      getData(*args)
    end
    def data=(*args) # :nodoc:
      setData(*args)
    end
    def size(*args) # :nodoc:
      getSize(*args)
    end
    def size=(*args) # :nodoc:
      setSize(*args)
    end
    def arrowDir(*args) # :nodoc:
      getArrowDir(*args)
    end
    def arrowDir=(*args) # :nodoc:
      setArrowDir(*args)
    end
    def pos # :nodoc:
      getPos()
    end
    def pos=(p) # :nodoc:
      setPos(p)
    end
    def justification # :nodoc:
      getJustify()
    end
    def justification=(j) # :nodoc:
      setJustify(j)
    end
    def iconPosition # :nodoc:
      getIconPosition()
    end
    def iconPosition=(p) # :nodoc:
      setIconPosition(p)
    end
    def pressed=(*args) # :nodoc:
      setPressed(*args)
    end
    def pressed? # :nodoc:
      isPressed()
    end
  end
  class FXHeader
    def numItems(*args) # :nodoc:
      getNumItems(*args)
    end
    def totalSize # :nodoc:
      getTotalSize()
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def headerStyle(*args) # :nodoc:
      getHeaderStyle(*args)
    end
    def headerStyle=(*args) # :nodoc:
      setHeaderStyle(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
  end
  class FXIconItem
    def hasFocus? # :nodoc:
      hasFocus()
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def bigIcon(*args) # :nodoc:
      getBigIcon(*args)
    end
    def bigIcon=(*args) # :nodoc:
      setBigIcon(*args)
    end
    def miniIcon(*args) # :nodoc:
      getMiniIcon(*args)
    end
    def miniIcon=(*args) # :nodoc:
      setMiniIcon(*args)
    end
    def data(*args) # :nodoc:
      getData(*args)
    end
    def data=(*args) # :nodoc:
      setData(*args)
    end
    def selected?(*args) # :nodoc:
      isSelected(*args)
    end
    def selected=(*args) # :nodoc:
      setSelected(*args)
    end
    def enabled?(*args) # :nodoc:
      isEnabled(*args)
    end
    def enabled=(*args) # :nodoc:
      setEnabled(*args)
    end
    def draggable?(*args) # :nodoc:
      isDraggable(*args)
    end
    def draggable=(*args) # :nodoc:
      setDraggable(*args)
    end
  end
  class FXIconList
    def numItems(*args) # :nodoc:
      getNumItems(*args)
    end
    def numRows(*args) # :nodoc:
      getNumRows(*args)
    end
    def numCols(*args) # :nodoc:
      getNumCols(*args)
    end
    def header(*args) # :nodoc:
      getHeader(*args)
    end
    def numHeaders(*args) # :nodoc:
      getNumHeaders(*args)
    end
    def itemWidth(*args) # :nodoc:
      getItemWidth(*args)
    end
    def itemHeight(*args) # :nodoc:
      getItemHeight(*args)
    end
    def itemAt(x, y) # :nodoc:
      getItemAt(x, y)
    end
    def itemSelected?(index) # :nodoc:
      isItemSelected(index)
    end
    def itemCurrent?(index) # :nodoc:
      isItemCurrent(index)
    end
    def itemVisible?(index) # :nodoc:
      isItemVisible(index)
    end
    def itemEnabled?(index) # :nodoc:
      isItemEnabled(index)
    end
    def currentItem(*args) # :nodoc:
      getCurrentItem(*args)
    end
    def currentItem=(*args) # :nodoc:
      setCurrentItem(*args)
    end
    def anchorItem(*args) # :nodoc:
      getAnchorItem(*args)
    end
    def anchorItem=(*args) # :nodoc:
      setAnchorItem(*args)
    end
    def cursorItem(*args) # :nodoc:
      getCursorItem(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def itemSpace(*args) # :nodoc:
      getItemSpace(*args)
    end
    def itemSpace=(*args) # :nodoc:
      setItemSpace(*args)
    end
    def listStyle(*args) # :nodoc:
      getListStyle(*args)
    end
    def listStyle=(*args) # :nodoc:
      setListStyle(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
  end
  class FXId
    def app(*args) # :nodoc:
      getApp(*args)
    end
    def userData(*args) # :nodoc:
      getUserData(*args)
    end
    def userData=(*args) # :nodoc:
      setUserData(*args)
    end
  end
  class FXImage
    def data(*args) # :nodoc:
      getData(*args)
    end
    def options(*args) # :nodoc:
      getOptions(*args)
    end
    def options=(*args) # :nodoc:
      setOptions(*args)
    end
  end
  class FXImageFrame
    def image # :nodoc:
      getImage()
    end
    def image=(img) # :nodoc:
      setImage(img)
    end
    def justify # :nodoc:
      getJustify()
    end
    def justify=(mode) # :nodoc:
      setJustify(mode)
    end
  end
  class FXImageView
    def image(*args) # :nodoc:
      getImage(*args)
    end
    def image=(*args) # :nodoc:
      setImage(*args)
    end
    def alignment # :nodoc:
      getAlignment()
    end
    def alignment=(mode) # :nodoc:
      setAlignment(mode)
    end
  end
  class FXInputDialog
    def text(*args) # :nodoc:
      getText(*args)
    end
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def numColumns(*args) # :nodoc:
      getNumColumns(*args)
    end
    def numColumns=(*args) # :nodoc:
      setNumColumns(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
  end
  class FXJPGIcon
    def quality(*args) # :nodoc:
      getQuality(*args)
    end
    def quality=(*args) # :nodoc:
      setQuality(*args)
    end
  end
  class FXJPGImage
    def quality(*args) # :nodoc:
      getQuality(*args)
    end
    def quality=(*args) # :nodoc:
      setQuality(*args)
    end
  end
  class FXLabel
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def icon(*args) # :nodoc:
      getIcon(*args)
    end
    def icon=(*args) # :nodoc:
      setIcon(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def justify(*args) # :nodoc:
      getJustify(*args)
    end
    def justify=(*args) # :nodoc:
      setJustify(*args)
    end
    def iconPosition(*args) # :nodoc:
      getIconPosition(*args)
    end
    def iconPosition=(*args) # :nodoc:
      setIconPosition(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
  end
  class FXListItem
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def icon=(*args) # :nodoc:
      setIcon(*args)
    end
    def icon(*args) # :nodoc:
      getIcon(*args)
    end
    def data=(*args) # :nodoc:
      setData(*args)
    end
    def data(*args) # :nodoc:
      getData(*args)
    end
    def selected=(*args) # :nodoc:
      setSelected(*args)
    end
    def selected?(*args) # :nodoc:
      isSelected(*args)
    end
    def enabled=(*args) # :nodoc:
      setEnabled(*args)
    end
    def enabled?(*args) # :nodoc:
      isEnabled(*args)
    end
    def draggable=(*args) # :nodoc:
      setDraggable(*args)
    end
    def draggable?(*args) # :nodoc:
      isDraggable(*args)
    end
  end
  class FXList
    def numItems(*args) # :nodoc:
      getNumItems(*args)
    end
    def numVisible(*args) # :nodoc:
      getNumVisible(*args)
    end
    def numVisible=(*args) # :nodoc:
      setNumVisible(*args)
    end
    def itemSelected?(*args) # :nodoc:
      isItemSelected(*args)
    end
    def itemCurrent?(*args) # :nodoc:
      isItemCurrent(*args)
    end
    def itemVisible?(*args) # :nodoc:
      isItemVisible(*args)
    end
    def itemEnabled?(*args) # :nodoc:
      isItemEnabled(*args)
    end
    def currentItem=(*args) # :nodoc:
      setCurrentItem(*args)
    end
    def currentItem(*args) # :nodoc:
      getCurrentItem(*args)
    end
    def anchorItem=(*args) # :nodoc:
      setAnchorItem(*args)
    end
    def anchorItem(*args) # :nodoc:
      getAnchorItem(*args)
    end
    def cursorItem(*args) # :nodoc:
      getCursorItem(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def listStyle(*args) # :nodoc:
      getListStyle(*args)
    end
    def listStyle=(*args) # :nodoc:
      setListStyle(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
  end
  class FXListBox
    def numItems(*args) # :nodoc:
      getNumItems(*args)
    end
    def numVisible(*args) # :nodoc:
      getNumVisible(*args)
    end
    def numVisible=(*args) # :nodoc:
      setNumVisible(*args)
    end
    def currentItem=(*args) # :nodoc:
      setCurrentItem(*args)
    end
    def currentItem(*args) # :nodoc:
      getCurrentItem(*args)
    end
    def itemCurrent?(*args) # :nodoc:
      isItemCurrent(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def paneShown?(*args) # :nodoc:
      isPaneShown(*args)
    end
  end
  class FXMatrix
    def matrixStyle=(*args) # :nodoc:
      setMatrixStyle(*args)
    end
    def matrixStyle(*args) # :nodoc:
      getMatrixStyle(*args)
    end
    def numRows=(*args) # :nodoc:
      setNumRows(*args)
    end
    def numRows(*args) # :nodoc:
      getNumRows(*args)
    end
    def numColumns=(*args) # :nodoc:
      setNumColumns(*args)
    end
    def numColumns(*args) # :nodoc:
      getNumColumns(*args)
    end
  end
  class FXMDIChild
    def normalX=(*args) # :nodoc:
      setNormalX(*args)
    end
    def normalY=(*args) # :nodoc:
      setNormalY(*args)
    end
    def normalWidth=(*args) # :nodoc:
      setNormalWidth(*args)
    end
    def normalHeight=(*args) # :nodoc:
      setNormalHeight(*args)
    end
    def normalX(*args) # :nodoc:
      getNormalX(*args)
    end
    def normalY(*args) # :nodoc:
      getNormalY(*args)
    end
    def normalWidth(*args) # :nodoc:
      getNormalWidth(*args)
    end
    def normalHeight(*args) # :nodoc:
      getNormalHeight(*args)
    end
    def iconX=(*args) # :nodoc:
      setIconX(*args)
    end
    def iconY=(*args) # :nodoc:
      setIconY(*args)
    end
    def iconWidth=(*args) # :nodoc:
      setIconWidth(*args)
    end
    def iconHeight=(*args) # :nodoc:
      setIconHeight(*args)
    end
    def iconX(*args) # :nodoc:
      getIconX(*args)
    end
    def iconY(*args) # :nodoc:
      getIconY(*args)
    end
    def iconWidth(*args) # :nodoc:
      getIconWidth(*args)
    end
    def iconHeight(*args) # :nodoc:
      getIconHeight(*args)
    end
    def title=(*args) # :nodoc:
      setTitle(*args)
    end
    def title(*args) # :nodoc:
      getTitle(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def baseColor(*args) # :nodoc:
      getBaseColor(*args)
    end
    def borderColor(*args) # :nodoc:
      getBorderColor(*args)
    end
    def titleColor(*args) # :nodoc:
      getTitleColor(*args)
    end
    def titleBackColor(*args) # :nodoc:
      getTitleBackColor(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def baseColor=(*args) # :nodoc:
      setBaseColor(*args)
    end
    def borderColor=(*args) # :nodoc:
      setBorderColor(*args)
    end
    def titleColor=(*args) # :nodoc:
      setTitleColor(*args)
    end
    def titleBackColor=(*args) # :nodoc:
      setTitleBackColor(*args)
    end
    def maximized?(*args) # :nodoc:
      isMaximized(*args)
    end
    def minimized?(*args) # :nodoc:
      isMinimized(*args)
    end
    def icon(*args) # :nodoc:
      getIcon(*args)
    end
    def icon=(*args) # :nodoc:
      setIcon(*args)
    end
    def menu(*args) # :nodoc:
      getMenu(*args)
    end
    def menu=(*args) # :nodoc:
      setMenu(*args)
    end
    alias tracking=   setTracking
    alias isTracking? getTracking
    alias tracking?   getTracking
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
  end
  class FXMDIClient
    def activeChild(*args) # :nodoc:
      getActiveChild(*args)
    end
    def activeChild=(*args) # :nodoc:
      setActiveChild(*args)
    end
    def cascadeX=(*args) # :nodoc:
      setCascadeX(*args)
    end
    def cascadeY=(*args) # :nodoc:
      setCascadeY(*args)
    end
    def cascadeX(*args) # :nodoc:
      getCascadeX(*args)
    end
    def cascadeY(*args) # :nodoc:
      getCascadeY(*args)
    end
  end
  class FXMemoryBuffer
    alias size getSize
    alias data getData
    alias to_a getData
  end
  class FXMemoryStream
    alias space  getSpace
    alias space= setSpace
  end
  class FXMenuButton
    def menu=(*args) # :nodoc:
      setMenu(*args)
    end
    def menu(*args) # :nodoc:
      getMenu(*args)
    end
    def xOffset=(*args) # :nodoc:
      setXOffset(*args)
    end
    def xOffset(*args) # :nodoc:
      getXOffset(*args)
    end
    def yOffset=(*args) # :nodoc:
      setYOffset(*args)
    end
    def yOffset(*args) # :nodoc:
      getYOffset(*args)
    end
    def buttonStyle=(*args) # :nodoc:
      setButtonStyle(*args)
    end
    def buttonStyle(*args) # :nodoc:
      getButtonStyle(*args)
    end
    def popupStyle=(*args) # :nodoc:
      setPopupStyle(*args)
    end
    def popupStyle(*args) # :nodoc:
      getPopupStyle(*args)
    end
    def attachment=(*args) # :nodoc:
      setAttachment(*args)
    end
    def attachment(*args) # :nodoc:
      getAttachment(*args)
    end
  end
  class FXMenuCaption
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def icon=(*args) # :nodoc:
      setIcon(*args)
    end
    def icon(*args) # :nodoc:
      getIcon(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def menuStyle=(*args) # :nodoc:
      setMenuStyle(*args)
    end
    def menuStyle() # :nodoc:
      getMenuStyle()
    end
  end
  class FXMenuCascade
    def menu=(*args) # :nodoc:
      setMenu(*args)
    end
    def menu(*args) # :nodoc:
      getMenu(*args)
    end
  end
  class FXMenuCheck
    def checkState # :nodoc:
      getCheckState
    end
    def setCheckState(*args) # :nodoc:
      setCheck(*args)
    end
    def checkState=(*args) # :nodoc:
      setCheck(*args)
    end
    def check # :nodoc:
      getCheck
    end			# deprecated
    def check=(*args) # :nodoc:
      setCheck(*args)
    end	# deprecated
    def boxColor # :nodoc:
      getBoxColor()
    end
    def boxColor=(*args) # :nodoc:
      setBoxColor(*args)
    end
  end
  class FXMenuCommand
    alias accelText=	setAccelText
    alias accelText		getAccelText
  end
  class FXMenuRadio
    def checkState # :nodoc:
      getCheckState
    end
    def setCheckState(*args) # :nodoc:
      setCheck(*args)
    end
    def checkState=(*args) # :nodoc:
      setCheck(*args)
    end
    def check # :nodoc:
      getCheck
    end			# deprecated
    def check=(*args) # :nodoc:
      setCheck(*args)
    end	# deprecated
    def radioColor # :nodoc:
      getRadioColor()
    end
    def radioColor=(*args) # :nodoc:
      setRadioColor(*args)
    end
  end
  class FXMenuSeparator
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
  end
  class FXMenuTitle
    def menu=(*args) # :nodoc:
      setMenu(*args)
    end
    def menu(*args) # :nodoc:
      getMenu(*args)
    end
  end
  class FXOptionMenu
    alias numOptions getNumOptions
    def current=(*args) # :nodoc:
      setCurrent(*args)
    end
    def current(*args) # :nodoc:
      getCurrent(*args)
    end
    def currentNo=(*args) # :nodoc:
      setCurrentNo(*args)
    end
    def currentNo(*args) # :nodoc:
      getCurrentNo(*args)
    end
    def menu=(*args) # :nodoc:
      setMenu(*args)
    end
    def menu(*args) # :nodoc:
      getMenu(*args)
    end
    def popped?(*args) # :nodoc:
      isPopped(*args)
    end
  end
  class FXPacker
    def frameStyle=(*args) # :nodoc:
      setFrameStyle(*args)
    end
    def frameStyle(*args) # :nodoc:
      getFrameStyle(*args)
    end
    def packingHints=(*args) # :nodoc:
      setPackingHints(*args)
    end
    def packingHints(*args) # :nodoc:
      getPackingHints(*args)
    end
    def borderWidth(*args) # :nodoc:
      getBorderWidth(*args)
    end
    def padTop=(*args) # :nodoc:
      setPadTop(*args)
    end
    def padTop(*args) # :nodoc:
      getPadTop(*args)
    end
    def padBottom=(*args) # :nodoc:
      setPadBottom(*args)
    end
    def padBottom(*args) # :nodoc:
      getPadBottom(*args)
    end
    def padLeft=(*args) # :nodoc:
      setPadLeft(*args)
    end
    def padLeft(*args) # :nodoc:
      getPadLeft(*args)
    end
    def padRight=(*args) # :nodoc:
      setPadRight(*args)
    end
    def padRight(*args) # :nodoc:
      getPadRight(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def borderColor=(*args) # :nodoc:
      setBorderColor(*args)
    end
    def borderColor(*args) # :nodoc:
      getBorderColor(*args)
    end
    def baseColor=(*args) # :nodoc:
      setBaseColor(*args)
    end
    def baseColor(*args) # :nodoc:
      getBaseColor(*args)
    end
    def hSpacing=(*args) # :nodoc:
      setHSpacing(*args)
    end
    def hSpacing(*args) # :nodoc:
      getHSpacing(*args)
    end
    def vSpacing=(*args) # :nodoc:
      setVSpacing(*args)
    end
    def vSpacing(*args) # :nodoc:
      getVSpacing(*args)
    end
  end
  class FXPopup
    def frameStyle=(*args) # :nodoc:
      setFrameStyle(*args)
    end
    def frameStyle(*args) # :nodoc:
      getFrameStyle(*args)
    end
    def borderWidth(*args) # :nodoc:
      getBorderWidth(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def borderColor=(*args) # :nodoc:
      setBorderColor(*args)
    end
    def borderColor(*args) # :nodoc:
      getBorderColor(*args)
    end
    def baseColor=(*args) # :nodoc:
      setBaseColor(*args)
    end
    def baseColor(*args) # :nodoc:
      getBaseColor(*args)
    end
    def grabOwner(*args) # :nodoc:
      getGrabOwner(*args)
    end
    def orientation(*args) # :nodoc:
      getOrientation(*args)
    end
    def orientation=(*args) # :nodoc:
      setOrientation(*args)
    end
    def shrinkWrap(*args) # :nodoc:
      getShrinkWrap(*args)
    end
    def shrinkWrap=(*args) # :nodoc:
      setShrinkWrap(*args)
    end
  end
  class FXPrintDialog
    def printer=(*args) # :nodoc:
      setPrinter(*args)
    end
    def printer(*args) # :nodoc:
      getPrinter(*args)
    end
  end
  class FXProgressBar
    def progress=(*args) # :nodoc:
      setProgress(*args)
    end
    def progress(*args) # :nodoc:
      getProgress(*args)
    end
    def total=(*args) # :nodoc:
      setTotal(*args)
    end
    def total(*args) # :nodoc:
      getTotal(*args)
    end
    def barSize=(*args) # :nodoc:
      setBarSize(*args)
    end
    def barSize(*args) # :nodoc:
      getBarSize(*args)
    end
    def barBGColor=(*args) # :nodoc:
      setBarBGColor(*args)
    end
    def barBGColor(*args) # :nodoc:
      getBarBGColor(*args)
    end
    def barColor=(*args) # :nodoc:
      setBarColor(*args)
    end
    def barColor(*args) # :nodoc:
      getBarColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textAltColor=(*args) # :nodoc:
      setTextAltColor(*args)
    end
    def textAltColor(*args) # :nodoc:
      getTextAltColor(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def barStyle=(*args) # :nodoc:
      setBarStyle(*args)
    end
    def barStyle(*args) # :nodoc:
      getBarStyle(*args)
    end
  end
  class FXProgressDialog
    def barStyle=(*args) # :nodoc:
      setBarStyle(*args)
    end
    def barStyle # :nodoc:
      getBarStyle()
    end
    def message=(*args) # :nodoc:
      setMessage(*args)
    end
    def message(*args) # :nodoc:
      getMessage(*args)
    end
    def progress=(*args) # :nodoc:
      setProgress(*args)
    end
    def progress(*args) # :nodoc:
      getProgress(*args)
    end
    def total=(*args) # :nodoc:
      setTotal(*args)
    end
    def total(*args) # :nodoc:
      getTotal(*args)
    end
    def cancelled?(*args) # :nodoc:
      isCancelled(*args)
    end
  end
  class FXQuatd
    def FXQuatd.arc(*args)
      FXQuatd.new.arc!(*args)
    end
    def FXQuatd.lerp(*args)
      FXQuatd.new.lerp!(*args)
    end
  end
  class FXQuatf
    def FXQuatf.arc(*args)
      FXQuatf.new.arc!(*args)
    end
    def FXQuatf.lerp(*args)
      FXQuatf.new.lerp!(*args)
    end
  end
  class FXRadioButton
    def checkState # :nodoc:
      getCheckState
    end
    def setCheckState(*args) # :nodoc:
      setCheck(*args)
    end
    def checkState=(*args) # :nodoc:
      setCheck(*args)
    end
    def check # :nodoc:
      getCheck
    end			# deprecated
    def check=(*args) # :nodoc:
      setCheck(*args)
    end	# deprecated
    def radioButtonStyle=(*args) # :nodoc:
      setRadioButtonStyle(*args)
    end
    def radioButtonStyle(*args) # :nodoc:
      getRadioButtonStyle(*args)
    end
    def radioColor(*args) # :nodoc:
      getRadioColor(*args)
    end
    def radioColor=(*args) # :nodoc:
      setRadioColor(*args)
    end
    def diskColor # :nodoc:
      getDiskColor()
    end
    def diskColor=(clr) # :nodoc:
      setDiskColor(clr)
    end
  end

  class FXRealSlider
    def value=(*args) # :nodoc:
      setValue(*args)
    end
    def value(*args) # :nodoc:
      getValue(*args)
    end
    def sliderStyle(*args) # :nodoc:
      getSliderStyle(*args)
    end
    def sliderStyle=(*args) # :nodoc:
      setSliderStyle(*args)
    end
    def headSize(*args) # :nodoc:
      getHeadSize(*args)
    end
    def headSize=(*args) # :nodoc:
      setHeadSize(*args)
    end
    def slotSize(*args) # :nodoc:
      getSlotSize(*args)
    end
    def slotSize=(*args) # :nodoc:
      setSlotSize(*args)
    end
    def increment(*args) # :nodoc:
      getIncrement(*args)
    end
    def increment=(*args) # :nodoc:
      setIncrement(*args)
    end
    def tickDelta(*args) # :nodoc:
      getTickDelta(*args)
    end
    def tickDelta=(*args) # :nodoc:
      setTickDelta(*args)
    end
    def slotColor=(*args) # :nodoc:
      setSlotColor(*args)
    end
    def slotColor(*args) # :nodoc:
      getSlotColor(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
  end

  class FXRealSpinner
    def cyclic?(*args) # :nodoc:
      isCyclic(*args)
    end
    def cyclic=(*args) # :nodoc:
      setCyclic(*args)
    end
    def textVisible?(*args) # :nodoc:
      isTextVisible(*args)
    end
    def textVisible=(*args) # :nodoc:
      setTextVisible(*args)
    end
    def value=(*args) # :nodoc:
      setValue(*args)
    end
    def value(*args) # :nodoc:
      getValue(*args)
    end
    def range=(*args) # :nodoc:
      setRange(*args)
    end
    def range(*args) # :nodoc:
      getRange(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def spinnerStyle=(*args) # :nodoc:
      setSpinnerStyle(*args)
    end
    def spinnerStyle(*args) # :nodoc:
      getSpinnerStyle(*args)
    end
    def editable=(*args) # :nodoc:
      setEditable(*args)
    end
    def editable?(*args) # :nodoc:
      isEditable(*args)
    end
    def upArrowColor() # :nodoc:
      getUpArrowColor()
    end
    def upArrowColor=(clr) # :nodoc:
      setUpArrowColor(clr)
    end
    def downArrowColor() # :nodoc:
      getDownArrowColor()
    end
    def downArrowColor=(clr) # :nodoc:
      setDownArrowColor(clr)
    end
    def textColor() # :nodoc:
      getTextColor()
    end
    def textColor=(clr) # :nodoc:
      setTextColor(clr)
    end
    def selBackColor() # :nodoc:
      getSelBackColor()
    end
    def selBackColor=(clr) # :nodoc:
      setSelBackColor(clr)
    end
    def selTextColor() # :nodoc:
      getSelTextColor()
    end
    def selTextColor=(clr) # :nodoc:
      setSelTextColor(clr)
    end
    def cursorColor() # :nodoc:
      getCursorColor()
    end
    def cursorColor=(clr) # :nodoc:
      setCursorColor(clr)
    end
    def numColumns # :nodoc:
      getNumColumns()
    end
    def numColumns=(nc) # :nodoc:
      setNumColumns(nc)
    end
  end

  class FXRecentFiles
    def maxFiles=(*args) # :nodoc:
      setMaxFiles(*args)
    end
    def maxFiles(*args) # :nodoc:
      getMaxFiles(*args)
    end
    def groupName=(*args) # :nodoc:
      setGroupName(*args)
    end
    def groupName(*args) # :nodoc:
      getGroupName(*args)
    end
    def target=(*args) # :nodoc:
      setTarget(*args)
    end
    def target(*args) # :nodoc:
      getTarget(*args)
    end
    def selector=(*args) # :nodoc:
      setSelector(*args)
    end
    def selector(*args) # :nodoc:
      getSelector(*args)
    end
  end
  class FXRegistry
    def appKey # :nodoc:
      getAppKey
    end
    def vendorKey # :nodoc:
      getVendorKey
    end
    def asciiMode=(*args) # :nodoc:
      setAsciiMode(*args)
    end
    def asciiMode? # :nodoc:
      getAsciiMode
    end
  end
  class FXReplaceDialog
    def searchText=(*args) # :nodoc:
      setSearchText(*args)
    end
    def searchText(*args) # :nodoc:
      getSearchText(*args)
    end
    def replaceText=(*args) # :nodoc:
      setReplaceText(*args)
    end
    def replaceText(*args) # :nodoc:
      getReplaceText(*args)
    end
    def searchMode=(*args) # :nodoc:
      setSearchMode(*args)
    end
    def searchMode(*args) # :nodoc:
      getSearchMode(*args)
    end
  end

  class FXRuler
    def position=(*args) # :nodoc:
      setPosition(*args)
    end
    undef_method(:position) if defined?(:position)
    def position # :nodoc:
      getPosition
    end
    def contentSize=(*args) # :nodoc:
      setContentSize(*args)
    end
    def contentSize # :nodoc:
      getContentSize
    end
    def documentSize=(*args) # :nodoc:
      setDocumentSize(*args)
    end
    def documentSize # :nodoc:
      getDocumentSize()
    end
    def edgeSpacing=(*args) # :nodoc:
      setEdgeSpacing(*args)
    end
    def edgeSpacing # :nodoc:
      getEdgeSpacing()
    end
    def marginLower=(*args) # :nodoc:
      setMarginLower(*args)
    end
    def marginLower # :nodoc:
      getMarginLower()
    end
    def marginUpper=(*args) # :nodoc:
      setMarginUpper(*args)
    end
    def marginUpper # :nodoc:
      getMarginUpper()
    end
    def indentFirst=(*args) # :nodoc:
      setIndentFirst(*args)
    end
    def indentFirst # :nodoc:
      getIndentFirst()
    end
    def indentLower=(*args) # :nodoc:
      setIndentLower(*args)
    end
    def indentLower # :nodoc:
      getIndentLower()
    end
    def indentUpper=(*args) # :nodoc:
      setIndentUpper(*args)
    end
    def indentUpper # :nodoc:
      getIndentUpper()
    end
    def numberTicks=(*args) # :nodoc:
      setNumberTicks(*args)
    end
    def numberTicks # :nodoc:
      getNumberTicks()
    end
    def majorTicks=(*args) # :nodoc:
      setMajorTicks(*args)
    end
    def majorTicks # :nodoc:
      getMajorTicks()
    end
    def minorTicks=(*args) # :nodoc:
      setMinorTicks(*args)
    end
    def minorTicks # :nodoc:
      getMinorTicks()
    end
    def tinyTicks=(*args) # :nodoc:
      setTinyTicks(*args)
    end
    def tinyTicks # :nodoc:
      getTinyTicks()
    end
    def pixelsPerTick=(*args) # :nodoc:
      setPixelPerTick(*args)
    end
    def pixelsPerTick # :nodoc:
      getPixelPerTick()
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font # :nodoc:
      getFont
    end
    def value=(*args) # :nodoc:
      setValue(*args)
    end
    def value # :nodoc:
      getValue
    end
    def rulerStyle=(*args) # :nodoc:
      setRulerStyle(*args)
    end
    def rulerStyle # :nodoc:
      getRulerStyle
    end
    def rulerAlignment=(*args) # :nodoc:
      setRulerAlignment(*args)
    end
    def rulerAlignment # :nodoc:
      getRulerAlignment
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def textColor # :nodoc:
      getTextColor
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText # :nodoc:
      getHelpText
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText # :nodoc:
      getTipText
    end
  end

  class FXRulerView
    alias documentX		getDocumentX
    alias documentY		getDocumentY
    alias documentColor		getDocumentColor
    alias documentColor=	setDocumentColor
    alias documentWidth		getDocumentWidth
    alias documentWidth=	setDocumentWidth
    alias documentHeight	getDocumentHeight
    alias documentHeight=	setDocumentHeight
    alias hEdgeSpacing 		getHEdgeSpacing
    alias hEdgeSpacing=		setHEdgeSpacing
    alias vEdgeSpacing		getVEdgeSpacing
    alias vEdgeSpacing=		setVEdgeSpacing
    alias hMarginLower		getHMarginLower
    alias hMarginLower=		setHMarginLower
    alias hMarginUpper		getHMarginUpper
    alias hMarginUpper=		setHMarginUpper
    alias vMarginLower 		getVMarginLower
    alias vMarginLower=		setVMarginLower
    alias vMarginUpper 		getVMarginUpper
    alias vMarginUpper=		setVMarginUpper
    alias hAlignment		getHAlignment
    alias hAlignment=		setHAlignment
    alias vAlignment		getVAlignment
    alias vAlignment=		setVAlignment
    alias hRulerFont		getHRulerFont
    alias hRulerFont=		setHRulerFont
    alias vRulerFont 		getVRulerFont
    alias vRulerFont=		setVRulerFont
    alias hNumberTicks 		getHNumberTicks
    alias hNumberTicks=		setHNumberTicks
    alias vNumberTicks 		getVNumberTicks
    alias vNumberTicks=		setVNumberTicks
    alias hMajorTicks 		getHMajorTicks
    alias hMajorTicks=		setHMajorTicks
    alias vMajorTicks 		getVMajorTicks
    alias vMajorTicks=		setVMajorTicks
    alias hMediumTicks 		getHMediumTicks
    alias hMediumTicks=		setHMediumTicks
    alias vMediumTicks 		getVMediumTicks
    alias vMediumTicks=		setVMediumTicks
    alias hTinyTicks 		getHTinyTicks
    alias hTinyTicks=		setHTinyTicks
    alias vTinyTicks 		getVTinyTicks
    alias vTinyTicks=		setVTinyTicks
    alias hPixelsPerTick 	getHPixelsPerTick
    alias hPixelsPerTick=	setHPixelsPerTick
    alias vPixelsPerTick 	getVPixelsPerTick
    alias vPixelsPerTick=	setVPixelsPerTick
    alias arrowPosX		getArrowPosX
    alias arrowPosX=		setArrowPosX
    alias arrowPosY		getArrowPosY
    alias arrowPosY=		setArrowPosY
    alias hRulerStyle		getHRulerStyle
    alias hRulerStyle=		setHRulerStyle
    alias vRulerStyle		getVRulerStyle
    alias vRulerStyle=		setVRulerStyle
    alias helpText		getHelpText
    alias helpText=		setHelpText
    alias tipText		getTipText
    alias tipText=		setTipText
  end

  class FXScrollArea
    def viewportWidth(*args) # :nodoc:
      getViewportWidth(*args)
    end
    def viewportHeight(*args) # :nodoc:
      getViewportHeight(*args)
    end
    def contentWidth(*args) # :nodoc:
      getContentWidth(*args)
    end
    def contentHeight(*args) # :nodoc:
      getContentHeight(*args)
    end
    def scrollStyle=(*args) # :nodoc:
      setScrollStyle(*args)
    end
    def scrollStyle(*args) # :nodoc:
      getScrollStyle(*args)
    end
    def horizontalScrollable?(*args) # :nodoc:
      isHorizontalScrollable(*args)
    end
    def verticalScrollable?(*args) # :nodoc:
      isVerticalScrollable(*args)
    end
    def xPosition(*args) # :nodoc:
      getXPosition(*args)
    end
    def yPosition(*args) # :nodoc:
      getYPosition(*args)
    end
  end
  class FXScrollBar
    def range=(*args) # :nodoc:
      setRange(*args)
    end
    def range(*args) # :nodoc:
      getRange(*args)
    end
    def page=(*args) # :nodoc:
      setPage(*args)
    end
    def page(*args) # :nodoc:
      getPage(*args)
    end
    def line=(*args) # :nodoc:
      setLine(*args)
    end
    def line(*args) # :nodoc:
      getLine(*args)
    end
    def position=(*args) # :nodoc:
      setPosition(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def borderColor(*args) # :nodoc:
      getBorderColor(*args)
    end
    def borderColor=(*args) # :nodoc:
      setBorderColor(*args)
    end
    def scrollbarStyle(*args) # :nodoc:
      getScrollbarStyle(*args)
    end
    def scrollbarStyle=(*args) # :nodoc:
      setScrollbarStyle(*args)
    end
  end
  class FXScrollPane
    def topItem=(t) # :nodoc:
      setTopItem(t)
    end
    def topItem # :nodoc:
      getTopItem()
    end
  end
  class FXSearchDialog
    def searchText=(*args) # :nodoc:
      setSearchText(*args)
    end
    def searchText(*args) # :nodoc:
      getSearchText(*args)
    end
    def searchMode=(*args) # :nodoc:
      setSearchMode(*args)
    end
    def searchMode(*args) # :nodoc:
      getSearchMode(*args)
    end
  end
  class FXSeparator
    def separatorStyle=(style) # :nodoc:
      setSeparatorStyle(style)
    end
    def separatorStyle() # :nodoc:
      getSeparatorStyle()
    end
  end
  class FXSettings
    def existingEntry?(*args)
      existingEntry(*args)
    end
    def existingSection?(*args)
      existingSection(*args)
    end
    def modified=(*args)
      setModified(*args)
    end
    def modified?
      isModified
    end
  end
  class FXShutterItem
    def button(*args) # :nodoc:
      getButton(*args)
    end
    def content(*args) # :nodoc:
      getContent(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
  end
  class FXShutter
    def current=(*args) # :nodoc:
      setCurrent(*args)
    end
    def current(*args) # :nodoc:
      getCurrent(*args)
    end
  end

  class FXSlider
    def value=(*args) # :nodoc:
      setValue(*args)
    end
    def value(*args) # :nodoc:
      getValue(*args)
    end
    def range=(*args) # :nodoc:
      setRange(*args)
    end
    def range(*args) # :nodoc:
      getRange(*args)
    end
    def sliderStyle(*args) # :nodoc:
      getSliderStyle(*args)
    end
    def sliderStyle=(*args) # :nodoc:
      setSliderStyle(*args)
    end
    def headSize(*args) # :nodoc:
      getHeadSize(*args)
    end
    def headSize=(*args) # :nodoc:
      setHeadSize(*args)
    end
    def slotSize(*args) # :nodoc:
      getSlotSize(*args)
    end
    def slotSize=(*args) # :nodoc:
      setSlotSize(*args)
    end
    def increment(*args) # :nodoc:
      getIncrement(*args)
    end
    def increment=(*args) # :nodoc:
      setIncrement(*args)
    end
    def tickDelta(*args) # :nodoc:
      getTickDelta(*args)
    end
    def tickDelta=(*args) # :nodoc:
      setTickDelta(*args)
    end
    def slotColor=(*args) # :nodoc:
      setSlotColor(*args)
    end
    def slotColor(*args) # :nodoc:
      getSlotColor(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
  end

  class FXSpinner
    def cyclic?(*args) # :nodoc:
      isCyclic(*args)
    end
    def cyclic=(*args) # :nodoc:
      setCyclic(*args)
    end
    def textVisible?(*args) # :nodoc:
      isTextVisible(*args)
    end
    def textVisible=(*args) # :nodoc:
      setTextVisible(*args)
    end
    def value=(*args) # :nodoc:
      setValue(*args)
    end
    def value(*args) # :nodoc:
      getValue(*args)
    end
    def range=(*args) # :nodoc:
      setRange(*args)
    end
    def range(*args) # :nodoc:
      getRange(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def spinnerStyle=(*args) # :nodoc:
      setSpinnerStyle(*args)
    end
    def spinnerStyle(*args) # :nodoc:
      getSpinnerStyle(*args)
    end
    def editable=(*args) # :nodoc:
      setEditable(*args)
    end
    def editable?(*args) # :nodoc:
      isEditable(*args)
    end
    def upArrowColor() # :nodoc:
      getUpArrowColor()
    end
    def upArrowColor=(clr) # :nodoc:
      setUpArrowColor(clr)
    end
    def downArrowColor() # :nodoc:
      getDownArrowColor()
    end
    def downArrowColor=(clr) # :nodoc:
      setDownArrowColor(clr)
    end
    def textColor() # :nodoc:
      getTextColor()
    end
    def textColor=(clr) # :nodoc:
      setTextColor(clr)
    end
    def selBackColor() # :nodoc:
      getSelBackColor()
    end
    def selBackColor=(clr) # :nodoc:
      setSelBackColor(clr)
    end
    def selTextColor() # :nodoc:
      getSelTextColor()
    end
    def selTextColor=(clr) # :nodoc:
      setSelTextColor(clr)
    end
    def cursorColor() # :nodoc:
      getCursorColor()
    end
    def cursorColor=(clr) # :nodoc:
      setCursorColor(clr)
    end
    def numColumns # :nodoc:
      getNumColumns()
    end
    def numColumns=(nc) # :nodoc:
      setNumColumns(nc)
    end
  end
  class FXSplitter
    def splitterStyle=(*args) # :nodoc:
      setSplitterStyle(*args)
    end
    def splitterStyle(*args) # :nodoc:
      getSplitterStyle(*args)
    end
    def barSize=(*args) # :nodoc:
      setBarSize(*args)
    end
    def barSize(*args) # :nodoc:
      getBarSize(*args)
    end
  end
  class FXSpring
    def relativeWidth=(*args) # :nodoc:
      setRelativeWidth(*args)
    end
    def relativeWidth # :nodoc:
      getRelativeWidth()
    end
    def relativeHeight=(*args) # :nodoc:
      setRelativeHeight(*args)
    end
    def relativeHeight() # :nodoc:
      getRelativeHeight()
    end
  end
  class FXStatusBar
    def cornerStyle=(*args) # :nodoc:
      setCornerStyle(*args)
    end
    def cornerStyle(*args) # :nodoc:
      getCornerStyle(*args)
    end
    def statusLine(*args) # :nodoc:
      getStatusLine(*args)
    end
    def dragCorner(*args) # :nodoc:
      getDragCorner(*args)
    end
  end
  class FXStatusLine
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def normalText=(*args) # :nodoc:
      setNormalText(*args)
    end
    def normalText(*args) # :nodoc:
      getNormalText(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def textHighlightColor(*args) # :nodoc:
      getTextHighlightColor(*args)
    end
    def textHighlightColor=(*args) # :nodoc:
      setTextHighlightColor(*args)
    end
  end
  class FXStream
    def bigEndian=(*args) # :nodoc:
      setBigEndian(*args)
    end
    def bigEndian? # :nodoc:
      isBigEndian()
    end
    def error=(*args) # :nodoc:
      setError(*args)
    end
    def space # :nodoc:
      getSpace()
    end
    def space=(sp) # :nodoc:
      setSpace(sp)
    end
    def position=(*args) # :nodoc:
      setPosition(*args)
    end
    def position(*args) # :nodoc:
      getPosition(*args)
    end
    def bytesSwapped=(*args) # :nodoc:
      setSwapBytes(*args)
    end
    def bytesSwapped? # :nodoc:
      getSwapBytes()
    end
  end
  class FXSwitcher
    def current=(*args) # :nodoc:
      setCurrent(*args)
    end
    def current(*args) # :nodoc:
      getCurrent(*args)
    end
    def switcherStyle=(*args) # :nodoc:
      setSwitcherStyle(*args)
    end
    def switcherStyle(*args) # :nodoc:
      getSwitcherStyle(*args)
    end
  end
  class FXTabItem
    def tabOrientation(*args) # :nodoc:
      getTabOrientation(*args)
    end
    def tabOrientation=(*args) # :nodoc:
      setTabOrientation(*args)
    end
  end
  class FXTabBar
    def current=(*args) # :nodoc:
      setCurrent(*args)
    end
    def current(*args) # :nodoc:
      getCurrent(*args)
    end
    def tabStyle(*args) # :nodoc:
      getTabStyle(*args)
    end
    def tabStyle=(*args) # :nodoc:
      setTabStyle(*args)
    end
  end
  class FXTableItem
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def icon=(*args) # :nodoc:
      setIcon(*args)
    end
    def icon(*args) # :nodoc:
      getIcon(*args)
    end
    def data=(*args) # :nodoc:
      setData(*args)
    end
    def data(*args) # :nodoc:
      getData(*args)
    end
    def hasFocus?(*args) # :nodoc:
      hasFocus(*args)
    end
    def selected=(*args) # :nodoc:
      setSelected(*args)
    end
    def selected?(*args) # :nodoc:
      isSelected(*args)
    end
    def enabled=(*args) # :nodoc:
      setEnabled(*args)
    end
    def enabled?(*args) # :nodoc:
      isEnabled(*args)
    end
    def draggable=(*args) # :nodoc:
      setDraggable(*args)
    end
    def draggable?(*args) # :nodoc:
      isDraggable(*args)
    end
    def justify=(*args) # :nodoc:
      setJustify(*args)
    end
    def justify(*args) # :nodoc:
      getJustify(*args)
    end
    def iconPosition=(*args) # :nodoc:
      setIconPosition(*args)
    end
    def iconPosition(*args) # :nodoc:
      getIconPosition(*args)
    end
    def borders=(*args) # :nodoc:
      setBorders(*args)
    end
    def borders(*args) # :nodoc:
      getBorders(*args)
    end
    def stipple=(*args) # :nodoc:
      setStipple(*args)
    end
    def stipple(*args) # :nodoc:
      getStipple(*args)
    end
    def iconOwned=(*args) # :nodoc:
      setIconOwned(*args)
    end
    def iconOwned?(*args) # :nodoc:
      isIconOwned(*args)
    end
  end
  class FXTable
    def itemVisible?(*args) # :nodoc:
      isItemVisible(*args)
    end
    def itemSpanning?(*args) # :nodoc:
      isItemSpanning(*args)
    end
    def itemEnabled?(*args) # :nodoc:
      isItemEnabled(*args)
    end
    def itemCurrent?(*args) # :nodoc:
      isItemCurrent(*args)
    end
    def itemSelected?(*args) # :nodoc:
      isItemSelected(*args)
    end
    def horizontalGridShown=(vis) # :nodoc:
      showHorzGrid(vis)
    end
    def horizontalGridShown? # :nodoc:
      isHorzGridShown
    end
    def verticalGridShown=(vis) # :nodoc:
      showVertGrid(vis)
    end
    def verticalGridShown? # :nodoc:
      isVertGridShown
    end
    def columnHeader # :nodoc:
      getColumnHeader()
    end
    def rowHeader # :nodoc:
      getRowHeader()
    end
    def visibleRows=(*args) # :nodoc:
      setVisibleRows(*args)
    end
    def visibleRows(*args) # :nodoc:
      getVisibleRows(*args)
    end
    def visibleColumns=(*args) # :nodoc:
      setVisibleColumns(*args)
    end
    def visibleColumns(*args) # :nodoc:
      getVisibleColumns(*args)
    end
    def numRows(*args) # :nodoc:
      getNumRows(*args)
    end
    def numColumns(*args) # :nodoc:
      getNumColumns(*args)
    end
    def marginTop=(*args) # :nodoc:
      setMarginTop(*args)
    end
    def marginTop(*args) # :nodoc:
      getMarginTop(*args)
    end
    def marginBottom=(*args) # :nodoc:
      setMarginBottom(*args)
    end
    def marginBottom(*args) # :nodoc:
      getMarginBottom(*args)
    end
    def marginLeft=(*args) # :nodoc:
      setMarginLeft(*args)
    end
    def marginLeft(*args) # :nodoc:
      getMarginLeft(*args)
    end
    def marginRight=(*args) # :nodoc:
      setMarginRight(*args)
    end
    def marginRight(*args) # :nodoc:
      getMarginRight(*args)
    end
    def tableStyle(*args) # :nodoc:
      getTableStyle(*args)
    end
    def tableStyle=(*args) # :nodoc:
      setTableStyle(*args)
    end
    def columnHeaderMode # :nodoc:
      getColumnHeaderMode()
    end
    def columnHeaderMode=(hint) # :nodoc:
      setColumnHeaderMode(hint)
    end
    def rowHeaderMode # :nodoc:
      getRowHeaderMode()
    end
    def rowHeaderMode=(hint) # :nodoc:
      setRowHeaderMode(hint)
    end
    def columnHeaderFont # :nodoc:
      getColumnHeaderFont
    end
    def columnHeaderFont=(f) # :nodoc:
      setColumnHeaderFont(f)
    end
    def rowHeaderFont # :nodoc:
      getRowHeaderFont
    end
    def rowHeaderFont=(f) # :nodoc:
      setRowHeaderFont(f)
    end
    def columnHeaderHeight # :nodoc:
      getColumnHeaderHeight()
    end
    def columnHeaderHeight=(h) # :nodoc:
      setColumnHeaderHeight(h)
    end
    def rowHeaderWidth # :nodoc:
      getRowHeaderWidth()
    end
    def rowHeaderWidth=(w) # :nodoc:
      setRowHeaderWidth(w)
    end
    def defColumnWidth=(*args) # :nodoc:
      setDefColumnWidth(*args)
    end
    def defColumnWidth(*args) # :nodoc:
      getDefColumnWidth(*args)
    end
    def defRowHeight=(*args) # :nodoc:
      setDefRowHeight(*args)
    end
    def defRowHeight(*args) # :nodoc:
      getDefRowHeight(*args)
    end
    def minRowHeight(r) # :nodoc:
      getMinRowHeight(r)
    end
    def minColumnWidth(c) # :nodoc:
      getMinColumnWidth(c)
    end
    def currentRow(*args) # :nodoc:
      getCurrentRow(*args)
    end
    def currentColumn(*args) # :nodoc:
      getCurrentColumn(*args)
    end
    def anchorRow(*args) # :nodoc:
      getAnchorRow(*args)
    end
    def anchorColumn(*args) # :nodoc:
      getAnchorColumn(*args)
    end
    def selStartRow # :nodoc:
      getSelStartRow()
    end
    def selStartColumn # :nodoc:
      getSelStartColumn()
    end
    def selEndRow # :nodoc:
      getSelEndRow()
    end
    def selEndColumn # :nodoc:
      getSelEndColumn()
    end
    def rowSelected?(r) # :nodoc:
      isRowSelected(r)
    end
    def columnSelected?(c) # :nodoc:
      isColumnSelected(c)
    end
    def anythingSelected? # :nodoc:
      isAnythingSelected
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def baseColor=(*args) # :nodoc:
      setBaseColor(*args)
    end
    def baseColor(*args) # :nodoc:
      getBaseColor(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def borderColor=(*args) # :nodoc:
      setBorderColor(*args)
    end
    def borderColor(*args) # :nodoc:
      getBorderColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def gridColor=(*args) # :nodoc:
      setGridColor(*args)
    end
    def gridColor(*args) # :nodoc:
      getGridColor(*args)
    end
    def stippleColor=(*args) # :nodoc:
      setStippleColor(*args)
    end
    def stippleColor(*args) # :nodoc:
      getStippleColor(*args)
    end
    def cellBorderColor=(*args) # :nodoc:
      setCellBorderColor(*args)
    end
    def cellBorderColor(*args) # :nodoc:
      getCellBorderColor(*args)
    end
    def cellBorderWidth=(*args) # :nodoc:
      setCellBorderWidth(*args)
    end
    def cellBorderWidth(*args) # :nodoc:
      getCellBorderWidth(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
  end
  class FXText
    def positionSelected?(*args)
      isPosSelected(*args)
    end
    def positionVisible?(*args)
      isPosVisible(*args)
    end
    def positionAt(*args)
      getPosAt(*args)
    end
    def marginTop=(*args) # :nodoc:
      setMarginTop(*args)
    end
    def marginTop(*args) # :nodoc:
      getMarginTop(*args)
    end
    def marginBottom=(*args) # :nodoc:
      setMarginBottom(*args)
    end
    def marginBottom(*args) # :nodoc:
      getMarginBottom(*args)
    end
    def marginLeft=(*args) # :nodoc:
      setMarginLeft(*args)
    end
    def marginLeft(*args) # :nodoc:
      getMarginLeft(*args)
    end
    def marginRight=(*args) # :nodoc:
      setMarginRight(*args)
    end
    def marginRight(*args) # :nodoc:
      getMarginRight(*args)
    end
    def wrapColumns(*args) # :nodoc:
      getWrapColumns(*args)
    end
    def wrapColumns=(*args) # :nodoc:
      setWrapColumns(*args)
    end
    def tabColumns(*args) # :nodoc:
      getTabColumns(*args)
    end
    def tabColumns=(*args) # :nodoc:
      setTabColumns(*args)
    end
    def barColumns(*args) # :nodoc:
      getBarColumns(*args)
    end
    def barColumns=(*args) # :nodoc:
      setBarColumns(*args)
    end
    def modified?(*args) # :nodoc:
      isModified(*args)
    end
    def modified=(*args) # :nodoc:
      setModified(*args)
    end
    def editable?(*args) # :nodoc:
      isEditable(*args)
    end
    def editable=(*args) # :nodoc:
      setEditable(*args)
    end
    def styled=(*args) # :nodoc:
      setStyled(*args)
    end
    def styled?(*args) # :nodoc:
      isStyled(*args)
    end
    def delimiters=(*args) # :nodoc:
      setDelimiters(*args)
    end
    def delimiters(*args) # :nodoc:
      getDelimiters(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def hiliteTextColor=(*args) # :nodoc:
      setHiliteTextColor(*args)
    end
    def hiliteTextColor(*args) # :nodoc:
      getHiliteTextColor(*args)
    end
    def hiliteBackColor=(*args) # :nodoc:
      setHiliteBackColor(*args)
    end
    def hiliteBackColor(*args) # :nodoc:
      getHiliteBackColor(*args)
    end
    def activeBackColor=(*args) # :nodoc:
      setActiveBackColor(*args)
    end
    def activeBackColor(*args) # :nodoc:
      getActiveBackColor(*args)
    end
    def cursorColor=(*args) # :nodoc:
      setCursorColor(*args)
    end
    def cursorColor(*args) # :nodoc:
      getCursorColor(*args)
    end
    def numberColor=(*args) # :nodoc:
      setNumberColor(*args)
    end
    def numberColor(*args) # :nodoc:
      getNumberColor(*args)
    end
    def barColor=(*args) # :nodoc:
      setBarColor(*args)
    end
    def barColor(*args) # :nodoc:
      getBarColor(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def length(*args) # :nodoc:
      getLength(*args)
    end
    def topLine=(*args) # :nodoc:
      setTopLine(*args)
    end
    def topLine(*args) # :nodoc:
      getTopLine(*args)
    end
    def bottomLine=(*args) # :nodoc:
      setBottomLine(*args)
    end
    def bottomLine(*args) # :nodoc:
      getBottomLine(*args)
    end
    def centerLine=(*args) # :nodoc:
      setCenterLine(*args)
    end
    def anchorPos=(*args) # :nodoc:
      setAnchorPos(*args)
    end
    def anchorPos(*args) # :nodoc:
      getAnchorPos(*args)
    end
    def cursorPos=(*args) # :nodoc:
      setCursorPos(*args)
    end
    def cursorRow=(*args) # :nodoc:
      setCursorRow(*args)
    end
    def cursorRow(*args) # :nodoc:
      getCursorRow(*args)
    end
    def cursorCol=(*args) # :nodoc:
      setCursorCol(*args)
    end
    def cursorCol(*args) # :nodoc:
      getCursorCol(*args)
    end
    def cursorPos(*args) # :nodoc:
      getCursorPos(*args)
    end
    def selStartPos(*args) # :nodoc:
      getSelStartPos(*args)
    end
    def selEndPos(*args) # :nodoc:
      getSelEndPos(*args)
    end
    def textStyle=(*args) # :nodoc:
      setTextStyle(*args)
    end
    def textStyle(*args) # :nodoc:
      getTextStyle(*args)
    end
    def visibleRows=(*args) # :nodoc:
      setVisibleRows(*args)
    end
    def visibleRows(*args) # :nodoc:
      getVisibleRows(*args)
    end
    def visibleColumns=(*args) # :nodoc:
      setVisibleColumns(*args)
    end
    def visibleColumns(*args) # :nodoc:
      getVisibleColumns(*args)
    end
    def hiliteMatchTime=(*args) # :nodoc:
      setHiliteMatchTime(*args)
    end
    def hiliteMatchTime(*args) # :nodoc:
      getHiliteMatchTime(*args)
    end
    def hiliteStyles=(*args) # :nodoc:
      setHiliteStyles(*args)
    end
    def hiliteStyles(*args) # :nodoc:
      getHiliteStyles(*args)
    end
  end
  class FXTextField
    def editable?(*args) # :nodoc:
      isEditable(*args)
    end
    alias :editable :editable?
    def editable=(*args) # :nodoc:
      setEditable(*args)
    end
    def cursorPos=(*args) # :nodoc:
      setCursorPos(*args)
    end
    def cursorPos(*args) # :nodoc:
      getCursorPos(*args)
    end
    def cursorColor=(clr) # :nodoc:
      setCursorColor(clr)
    end
    def cursorColor() # :nodoc:
      getCursorColor()
    end
    def anchorPos=(*args) # :nodoc:
      setAnchorPos(*args)
    end
    def anchorPos(*args) # :nodoc:
      getAnchorPos(*args)
    end
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def numColumns=(*args) # :nodoc:
      setNumColumns(*args)
    end
    def numColumns(*args) # :nodoc:
      getNumColumns(*args)
    end
    def justify=(*args) # :nodoc:
      setJustify(*args)
    end
    def justify(*args) # :nodoc:
      getJustify(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
    def textStyle=(*args) # :nodoc:
      setTextStyle(*args)
    end
    def textStyle(*args) # :nodoc:
      getTextStyle(*args)
    end
    def posSelected?(*args) # :nodoc:
      isPosSelected(*args)
    end
    def posVisible?(*args) # :nodoc:
      isPosVisible(*args)
    end
  end
  class FXTIFIcon
    def codec=(*args) # :nodoc:
      setCodec(*args)
    end
    def codec(*args) # :nodoc:
      getCodec(*args)
    end
  end
  class FXTIFImage
    def codec=(*args) # :nodoc:
      setCodec(*args)
    end
    def codec(*args) # :nodoc:
      getCodec(*args)
    end
  end
  class FXToggleButton
    def altText=(*args) # :nodoc:
      setAltText(*args)
    end
    def altText(*args) # :nodoc:
      getAltText(*args)
    end
    def altIcon=(*args) # :nodoc:
      setAltIcon(*args)
    end
    def altIcon(*args) # :nodoc:
      getAltIcon(*args)
    end
    def state=(*args) # :nodoc:
      setState(*args)
    end
    def state(*args) # :nodoc:
      getState(*args)
    end
    def altHelpText=(*args) # :nodoc:
      setAltHelpText(*args)
    end
    def altHelpText(*args) # :nodoc:
      getAltHelpText(*args)
    end
    def altTipText=(*args) # :nodoc:
      setAltTipText(*args)
    end
    def altTipText(*args) # :nodoc:
      getAltTipText(*args)
    end
    def toggleStyle=(*args) # :nodoc:
      setToggleStyle(*args)
    end
    def toggleStyle(*args) # :nodoc:
      getToggleStyle(*args)
    end
  end
  class FXToolBarGrip
    def doubleBar? # :nodoc:
      isDoubleBar
    end
    def doubleBar=(*args) # :nodoc:
      setDoubleBar(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def activeColor=(*args) # :nodoc:
      setActiveColor(*args)
    end
    def activeColor(*args) # :nodoc:
      getActiveColor(*args)
    end
  end
  class FXToolBarShell
    def frameStyle=(*args) # :nodoc:
      setFrameStyle(*args)
    end
    def frameStyle(*args) # :nodoc:
      getFrameStyle(*args)
    end
    def borderWidth(*args) # :nodoc:
      getBorderWidth(*args)
    end
    def hiliteColor=(*args) # :nodoc:
      setHiliteColor(*args)
    end
    def hiliteColor(*args) # :nodoc:
      getHiliteColor(*args)
    end
    def shadowColor=(*args) # :nodoc:
      setShadowColor(*args)
    end
    def shadowColor(*args) # :nodoc:
      getShadowColor(*args)
    end
    def borderColor=(*args) # :nodoc:
      setBorderColor(*args)
    end
    def borderColor(*args) # :nodoc:
      getBorderColor(*args)
    end
    def baseColor=(*args) # :nodoc:
      setBaseColor(*args)
    end
    def baseColor(*args) # :nodoc:
      getBaseColor(*args)
    end
  end
  class FXToolBarTab
    def collapsed?(*args) # :nodoc:
      isCollapsed(*args)
    end
    def tabStyle=(*args) # :nodoc:
      setTabStyle(*args)
    end
    def tabStyle(*args) # :nodoc:
      getTabStyle(*args)
    end
    def activeColor(*args) # :nodoc:
      getActiveColor(*args)
    end
    def activeColor=(*args) # :nodoc:
      setActiveColor(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText # :nodoc:
      getTipText()
    end
  end
  class FXToolTip
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
  end
  class FXTopWindow
    def minimized? # # :nodoc:
      isMinimized()
    end
    def maximized? # :nodoc:
      isMaximized()
    end
    def title=(*args) # :nodoc:
      setTitle(*args)
    end
    def title(*args) # :nodoc:
      getTitle(*args)
    end
    def padTop=(*args) # :nodoc:
      setPadTop(*args)
    end
    def padTop(*args) # :nodoc:
      getPadTop(*args)
    end
    def padBottom=(*args) # :nodoc:
      setPadBottom(*args)
    end
    def padBottom(*args) # :nodoc:
      getPadBottom(*args)
    end
    def padLeft=(*args) # :nodoc:
      setPadLeft(*args)
    end
    def padLeft # :nodoc:
      getPadLeft()
    end
    def padRight=(*args) # :nodoc:
      setPadRight(*args)
    end
    def padRight # :nodoc:
      getPadRight()
    end
    def hSpacing(*args) # :nodoc:
      getHSpacing(*args)
    end
    def vSpacing(*args) # :nodoc:
      getVSpacing(*args)
    end
    def hSpacing=(*args) # :nodoc:
      setHSpacing(*args)
    end
    def vSpacing=(*args) # :nodoc:
      setVSpacing(*args)
    end
    def packingHints=(*args) # :nodoc:
      setPackingHints(*args)
    end
    def packingHints(*args) # :nodoc:
      getPackingHints(*args)
    end
    def decorations=(*args) # :nodoc:
      setDecorations(*args)
    end
    def decorations(*args) # :nodoc:
      getDecorations(*args)
    end
    def icon(*args) # :nodoc:
      getIcon(*args)
    end
    def icon=(*args) # :nodoc:
      setIcon(*args)
    end
    def miniIcon(*args) # :nodoc:
      getMiniIcon(*args)
    end
    def miniIcon=(*args) # :nodoc:
      setMiniIcon(*args)
    end
  end
  class FXTreeItem
    def parent(*args) # :nodoc:
      getParent(*args)
    end
    def next(*args) # :nodoc:
      getNext(*args)
    end
    def prev(*args) # :nodoc:
      getPrev(*args)
    end
    def first(*args) # :nodoc:
      getFirst(*args)
    end
    def last(*args) # :nodoc:
      getLast(*args)
    end
    def below(*args) # :nodoc:
      getBelow(*args)
    end
    def above(*args) # :nodoc:
      getAbove(*args)
    end
    def numChildren(*args) # :nodoc:
      getNumChildren(*args)
    end
    def text=(*args) # :nodoc:
      setText(*args)
    end
    def text(*args) # :nodoc:
      getText(*args)
    end
    def to_s(*args) # :nodoc:
      getText(*args)
    end
    def openIcon=(*args) # :nodoc:
      setOpenIcon(*args)
    end
    def openIcon(*args) # :nodoc:
      getOpenIcon(*args)
    end
    def closedIcon=(*args) # :nodoc:
      setClosedIcon(*args)
    end
    def closedIcon(*args) # :nodoc:
      getClosedIcon(*args)
    end
    def data=(*args) # :nodoc:
      setData(*args)
    end
    def data(*args) # :nodoc:
      getData(*args)
    end
    def selected=(*args) # :nodoc:
      setSelected(*args)
    end
    def selected?(*args) # :nodoc:
      isSelected(*args)
    end
    def opened=(*args) # :nodoc:
      setOpened(*args)
    end
    def opened?(*args) # :nodoc:
      isOpened(*args)
    end
    def expanded=(*args) # :nodoc:
      setExpanded(*args)
    end
    def expanded?(*args) # :nodoc:
      isExpanded(*args)
    end
    def enabled=(*args) # :nodoc:
      setEnabled(*args)
    end
    def enabled?(*args) # :nodoc:
      isEnabled(*args)
    end
    def draggable=(*args) # :nodoc:
      setDraggable(*args)
    end
    def draggable?(*args) # :nodoc:
      isDraggable(*args)
    end
    def childOf?(*args) # :nodoc:
      isChildOf(*args)
    end
    def parentOf?(*args) # :nodoc:
      isParentOf(*args)
    end
  end
  class FXTreeList
    def itemSelected?(*args) # :nodoc:
      isItemSelected(*args)
    end
    def itemCurrent?(*args) # :nodoc:
      isItemCurrent(*args)
    end
    def itemVisible?(*args) # :nodoc:
      isItemVisible(*args)
    end
    def itemOpened?(*args) # :nodoc:
      isItemOpened(*args)
    end
    def itemExpanded?(*args) # :nodoc:
      isItemExpanded(*args)
    end
    def itemLeaf?(*args) # :nodoc:
      isItemLeaf(*args)
    end
    def itemEnabled?(*args) # :nodoc:
      isItemEnabled(*args)
    end
    def numItems(*args) # :nodoc:
      getNumItems(*args)
    end
    def numVisible(*args) # :nodoc:
      getNumVisible(*args)
    end
    def numVisible=(*args) # :nodoc:
      setNumVisible(*args)
    end
    def firstItem(*args) # :nodoc:
      getFirstItem(*args)
    end
    def lastItem(*args) # :nodoc:
      getLastItem(*args)
    end
    def currentItem=(*args) # :nodoc:
      setCurrentItem(*args)
    end
    def currentItem(*args) # :nodoc:
      getCurrentItem(*args)
    end
    def anchorItem=(*args) # :nodoc:
      setAnchorItem(*args)
    end
    def anchorItem(*args) # :nodoc:
      getAnchorItem(*args)
    end
    def cursorItem(*args) # :nodoc:
      getCursorItem(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def indent=(*args) # :nodoc:
      setIndent(*args)
    end
    def indent(*args) # :nodoc:
      getIndent(*args)
    end
    def textColor(*args) # :nodoc:
      getTextColor(*args)
    end
    def textColor=(*args) # :nodoc:
      setTextColor(*args)
    end
    def selBackColor(*args) # :nodoc:
      getSelBackColor(*args)
    end
    def selBackColor=(*args) # :nodoc:
      setSelBackColor(*args)
    end
    def selTextColor(*args) # :nodoc:
      getSelTextColor(*args)
    end
    def selTextColor=(*args) # :nodoc:
      setSelTextColor(*args)
    end
    def lineColor(*args) # :nodoc:
      getLineColor(*args)
    end
    def lineColor=(*args) # :nodoc:
      setLineColor(*args)
    end
    def listStyle(*args) # :nodoc:
      getListStyle(*args)
    end
    def listStyle=(*args) # :nodoc:
      setListStyle(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
  end
  class FXTreeListBox
    def numItems(*args) # :nodoc:
      getNumItems(*args)
    end
    def numVisible(*args) # :nodoc:
      getNumVisible(*args)
    end
    def numVisible=(*args) # :nodoc:
      setNumVisible(*args)
    end
    def firstItem(*args) # :nodoc:
      getFirstItem(*args)
    end
    def lastItem(*args) # :nodoc:
      getLastItem(*args)
    end
    def currentItem=(*args) # :nodoc:
      setCurrentItem(*args)
    end
    def currentItem(*args) # :nodoc:
      getCurrentItem(*args)
    end
    def paneShown?(*args) # :nodoc:
      isPaneShown(*args)
    end
    def font=(*args) # :nodoc:
      setFont(*args)
    end
    def font(*args) # :nodoc:
      getFont(*args)
    end
    def listStyle(*args) # :nodoc:
      getListStyle(*args)
    end
    def listStyle=(*args) # :nodoc:
      setListStyle(*args)
    end
    def helpText=(*args) # :nodoc:
      setHelpText(*args)
    end
    def helpText(*args) # :nodoc:
      getHelpText(*args)
    end
    def tipText=(*args) # :nodoc:
      setTipText(*args)
    end
    def tipText(*args) # :nodoc:
      getTipText(*args)
    end
  end
  class FXVec2d
    alias len  length
    alias len2 length2
  end
  class FXVec2f
    alias len  length
    alias len2 length2
  end
  class FXVec3d
    alias len  length
    alias len2 length2
  end
  class FXVec3f
    alias len  length
    alias len2 length2
  end
  class FXVec4d
    alias len  length
    alias len2 length2
  end
  class FXVec4f
    alias len  length
    alias len2 length2
  end
  class FXVisual
    def flags(*args) # :nodoc:
      getFlags(*args)
    end
    def depth(*args) # :nodoc:
      getDepth(*args)
    end
    def numColors(*args) # :nodoc:
      getNumColors(*args)
    end
    def numRed(*args) # :nodoc:
      getNumRed(*args)
    end
    def numGreen(*args) # :nodoc:
      getNumGreen(*args)
    end
    def numBlue(*args) # :nodoc:
      getNumBlue(*args)
    end
    def maxColors=(*args) # :nodoc:
      setMaxColors(*args)
    end
    def maxColors(*args) # :nodoc:
      getMaxColors(*args)
    end
  end
  class FXWindow
    def parent(*args) # :nodoc:
      getParent(*args)
    end
    def owner(*args) # :nodoc:
      getOwner(*args)
    end
    def shell(*args) # :nodoc:
      getShell(*args)
    end
    def root(*args) # :nodoc:
      getRoot(*args)
    end
    def next(*args) # :nodoc:
      getNext(*args)
    end
    def prev(*args) # :nodoc:
      getPrev(*args)
    end
    def first(*args) # :nodoc:
      getFirst(*args)
    end
    def last(*args) # :nodoc:
      getLast(*args)
    end
    def focus(*args) # :nodoc:
      getFocus(*args)
    end
    def key=(*args) # :nodoc:
      setKey(*args)
    end
    def key(*args) # :nodoc:
      getKey(*args)
    end
    def target=(*args) # :nodoc:
      setTarget(*args)
    end
    def target(*args) # :nodoc:
      getTarget(*args)
    end
    def selector=(*args) # :nodoc:
      setSelector(*args)
    end
    def message=(*args) # :nodoc:
      setSelector(*args)
    end
    def selector(*args) # :nodoc:
      getSelector(*args)
    end
    def message(*args) # :nodoc:
      getSelector(*args)
    end
    def x(*args) # :nodoc:
      getX(*args)
    end
    def y(*args) # :nodoc:
      getY(*args)
    end
    def defaultWidth(*args) # :nodoc:
      getDefaultWidth(*args)
    end
    def defaultHeight(*args) # :nodoc:
      getDefaultHeight(*args)
    end
    def x=(*args) # :nodoc:
      setX(*args)
    end
    def y=(*args) # :nodoc:
      setY(*args)
    end
    def width=(*args) # :nodoc:
      setWidth(*args)
    end
    def height=(*args) # :nodoc:
      setHeight(*args)
    end
    def layoutHints=(*args) # :nodoc:
      setLayoutHints(*args)
    end
    def layoutHints(*args) # :nodoc:
      getLayoutHints(*args)
    end
    def accelTable(*args) # :nodoc:
      getAccelTable(*args)
    end
    def accelTable=(*args) # :nodoc:
      setAccelTable(*args)
    end
    def shell?(*args) # :nodoc:
      isShell(*args)
    end
    def childOf?(*args) # :nodoc:
      isChildOf(*args)
    end
    def containsChild?(*args) # :nodoc:
      containsChild(*args)
    end
    def defaultCursor=(*args) # :nodoc:
      setDefaultCursor(*args)
    end
    def defaultCursor(*args) # :nodoc:
      getDefaultCursor(*args)
    end
    def dragCursor=(*args) # :nodoc:
      setDragCursor(*args)
    end
    def dragCursor(*args) # :nodoc:
      getDragCursor(*args)
    end
    def cursorPosition(*args) # :nodoc:
      getCursorPosition(*args)
    end
    def setEnabled(x)
      x ? enable() : disable()
    end
    def enabled=(*args) # :nodoc:
      setEnabled(*args)
    end
    def enabled?(*args) # :nodoc:
      isEnabled(*args)
    end
    alias enabled enabled?
    def active?(*args) # :nodoc:
      isActive(*args)
    end
    def canFocus?(*args) # :nodoc:
      canFocus(*args)
    end
    def hasFocus?(*args) # :nodoc:
      hasFocus(*args)
    end
    def default?(*args) # :nodoc:
      isDefault(*args)
    end
    def initial?(*args) # :nodoc:
      isInitial(*args)
    end
    def grabbed?(*args) # :nodoc:
      grabbed(*args)
    end
    def grabbedKeyboard? # :nodoc:
      grabbedKeyboard
    end
    def setShown(s)
      s ? show() : hide()
    end
    def shown=(*args) # :nodoc:
      setShown(*args)
    end
    def shown?(*args) # :nodoc:
      shown(*args)
    end
    def composite?(*args) # :nodoc:
      isComposite(*args)
    end
    def underCursor?(*args) # :nodoc:
      underCursor(*args)
    end
    def hasSelection?(*args) # :nodoc:
      hasSelection(*args)
    end
    def hasClipboard?(*args) # :nodoc:
      hasClipboard(*args)
    end
    def dropEnabled?(*args) # :nodoc:
      isDropEnabled(*args)
    end
    def dragging?(*args) # :nodoc:
      isDragging(*args)
    end
    def dropTarget?(*args) # :nodoc:
      isDropTarget(*args)
    end
    def contains?(*args) # :nodoc:
      contains(*args)
    end
    def backColor=(*args) # :nodoc:
      setBackColor(*args)
    end
    def backColor(*args) # :nodoc:
      getBackColor(*args)
    end
    def doesSaveUnder?(*args) # :nodoc:
      doesSaveUnder(*args)
    end
    def offeredDNDType?(*args) # :nodoc:
      offeredDNDType(*args)
    end
  end

  class FXWizard
    def container # :nodoc:
      getContainer
    end
    def image=(img) # :nodoc:
      setImage(img)
    end
    def image # :nodoc:
      getImage
    end
    def numPanels # :nodoc:
      getNumPanels
    end
    def currentPanel=(p) # :nodoc:
      setCurrentPanel(p)
    end
    def currentPanel # :nodoc:
      getCurrentPanel
    end
  end

end
