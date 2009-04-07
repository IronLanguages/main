require 'fox16'

include Fox

class CustomTableItem < FXTableItem
	def drawContent(table, dc, x, y, w, h)
	  puts "in drawContent()"
		hg = table.horizontalGridShown?
		vg = table.verticalGridShown?
		ml = table.marginLeft + (vg ? 1 : 0)
		mt = table.marginTop + (hg ? 1 : 0)
		mr = table.marginRight
		mb = table.marginBottom
		font = dc.font
		lbl = text
		icn = icon

		# Text width and height
		beg, tw, th = 0, 0, 0
		begin
			_end = beg;
			_end += 1 while _end < lbl.length && lbl[_end].chr != '\n' 
			t = font.getTextWidth(lbl[beg..._end])
			tw = t if t > tw
			th += font.fontHeight
			beg = _end + 1
		end while _end < lbl.length

		# Icon size
		iw, ih = 0, 0
		unless icn.nil?
			iw = icn.width
			ih = icn.height
		end

		# Icon-text spacing
		s = 0
		s = 4 if (iw > 0 && tw > 0)

		# Fix x coordinate
		if justify & LEFT == 1
		  case iconPosition
			when BEFORE
			  ix = x + ml
        tx = ix + iw + s
			when AFTER
			  tx = x + ml
        ix = tx + tw + s
			else
			  ix = x + ml
			  tx = x + ml
			end
		elsif justify & RIGHT == 1
			case iconPosition
		  when BEFORE
				tx = x + w - mr - tw
				ix = tx - iw - s
			when AFTER
				ix = x + w - mr - iw
				tx = ix - tw - s
			else
			  ix = x + w - mr - iw
			  tx = x + w - mr - tw
			end
		else
		  case iconPosition
			when BEFORE
				ix = x + (ml + w - mr)/2 - (tw + iw + s)/2
        tx = ix + iw + s
			when AFTER
				tx = x + (ml + w - mr)/2 - (tw + iw + s)/2
        ix = tx + tw + s
			else
			  ix = x + (ml + w - mr)/2 - iw/2
        tx = x + (ml + w - mr)/2 -tw/2
      end
		end

		# Fix y coordinate
		if justify & TOP == 1
		  case iconPosition
			when ABOVE
			  iy = y + mt
        ty = iy + ih
			when BELOW
			  ty = y + mt
        iy = ty + th
			else
			  iy = y + mt
			  ty = y + mt
			end
		elsif justify & BOTTOM == 1
		  case iconPosition
			when ABOVE
			  ty = y + h - mb - th
        iy = ty - ih
			when BELOW
			  iy = y + h - mb - ih
        ty = iy - th
			else
			  iy = y + h - mb - ih
			  ty = y + h - mb - th
			end
		else
		  case iconPosition
			when ABOVE
				iy = y + (mt + h - mb)/2 - (th + ih)/2
				ty = iy + ih
			when BELOW
				ty = y + (mt + h - mb)/2 - (th + ih)/2
				iy = ty + th
			else
			  iy = y + (mt + h - mb)/2 - ih/2
        ty = y + (mt + h - mb)/2 - th/2
      end
		end

		# Paint icon
		dc.drawIcon(icn, ix, iy) unless icn.nil?
			
		# Text color
		if selected?
			dc.foreground = table.selTextColor
		else
			dc.foreground = table.textColor
		end
		puts "dc.foreground = (#{FXREDVAL(dc.foreground)}, #{FXGREENVAL(dc.foreground)}, #{FXBLUEVAL(dc.foreground)})"

		# Draw text
		yy = ty + font.fontAscent
		beg = 0
		begin
			_end = beg
			_end += 1 while _end < lbl.length && lbl[_end].chr != '\n' 
			if justify & LEFT == 1
			  xx = tx
			elsif justify & RIGHT == 1
        xx = tx + tw - font.getTextWidth(lbl[beg..._end])
			else
        xx = tx + (tw - font.getTextWidth(lbl[beg..._end]))/2
      end
			dc.drawText(xx, yy, lbl[beg..._end])
			yy += font.fontHeight
			beg = _end + 1
		end while _end < lbl.length
	end
end

class CustomTable < FXTable
	def createItem *parameters
		CustomTableItem.new *parameters
	end
end

app = FXApp.new
main = FXMainWindow.new app, 'Test'

table = CustomTable.new main
table.setTableSize 2, 2
table.visibleRows = 2
table.visibleColumns = 2

table.setItemText 0, 0, 'one'
table.setItemText 0, 1, 'two'
table.setItemText 1, 0, 'three'
table.setItemText 1, 1, 'four'

app.create
main.show PLACEMENT_SCREEN
app.run
