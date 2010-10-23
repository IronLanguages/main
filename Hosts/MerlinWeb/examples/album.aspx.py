#  Copyright (c) Microsoft Corporation. 
#
#  This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
#  copy of the license can be found in the License.html file at the root of this distribution. If 
#  you cannot locate the  Apache License, Version 2.0, please send an email to 
#  dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
#  by the terms of the Apache License, Version 2.0.
#
#  You must not remove this notice, or any other, from this software.

from System import DateTime, TimeSpan
from System.Drawing import Color, Bitmap, Graphics, Point, Rectangle, RectangleF, Pens, Brushes, Font
from System.Drawing.Drawing2D import InterpolationMode, LinearGradientBrush
from System.Drawing.Imaging import ImageFormat
from System.IO import Directory, File, Path, MemoryStream
from System.Web import HttpContext, HttpRuntime, HttpUtility
from System.Web.Caching import CacheDependency
from System.Web.Hosting import HostingEnvironment

# Customization

ThumbnailSize = 120
PreviewSize = 700
ChildFolderColor = Color.LightGray
ParentFolderColor = Color.LightSalmon

# Helpers

def BitmapToBytes(bitmap):
    ms = MemoryStream()
    bitmap.Save(ms, ImageFormat.Jpeg)
    ms.Close()
    return ms.ToArray()

def GetCachedItem(itemType, path):
    cacheKey = 'py:albumitem:' + path
    # lookup in cache
    item = HttpRuntime.Cache.Get(cacheKey)
    if item is not None: return item
    # create new
    item = itemType(path)
    # cache it
    HttpRuntime.Cache.Insert(cacheKey, item, None, DateTime.MaxValue, TimeSpan.FromMinutes(4))
    return item

# PICTURE respesents info about one picture

class PICTURE:
    def __init__(self, filename):
        self.filename = filename
        self.name = Path.GetFileName(self.filename)
        self.__thumbnail = None
        self.__dims = None


    def __getDimensions(self):
        if self.__dims is None:
            origImage = Bitmap.FromFile(self.filename, False)
            self.__dims = (origImage.Width, origImage.Height)
            origImage.Dispose()
        return self.__dims

    def CalcResizedDims(self, size):
        width, height = self.__getDimensions()
        if width >= height and width > size:
            return size, int(float(size*height)/width)
        elif height >= width and height > size:
            return int(float(size*width)/height), size
        else:
            return width, height

    def DrawResizedImage(self, size):
        # load original image
        origImage = Bitmap.FromFile(self.filename, False)
        # calculate
        if self.__dims is None: self.__dims = (origImage.Width, origImage.Height)
        width, height = self.CalcResizedDims(size)
        # draw new image
        newImage = Bitmap(width, height)
        g = Graphics.FromImage(newImage)
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        g.DrawImage(origImage, 0, 0, width, height)
        imageBytes = BitmapToBytes(newImage)
        # cleanup
        g.Dispose()
        newImage.Dispose()
        origImage.Dispose()
        return imageBytes

    def __drawPictureThumbnail(self):
        # load original image
        origImage = Bitmap.FromFile(self.filename, False)
        # calculate
        size = ThumbnailSize
        if self.__dims is None: self.__dims = (origImage.Width, origImage.Height)
        width, height = self.CalcResizedDims(size)
        drawWidth, drawHeight = width, height
        width, height = max(width, size), max(height, size)
        drawXOffset, drawYOffset = (width-drawWidth)/2, (height-drawHeight)/2
        # draw new image
        newImage = Bitmap(width, height)
        g = Graphics.FromImage(newImage)
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        g.FillRectangle(Brushes.GhostWhite, 0, 0, width, height)
        g.DrawRectangle(Pens.LightGray, 0, 0, width-1, height-1)
        g.DrawImage(origImage, drawXOffset, drawYOffset, drawWidth, drawHeight)
        imageBytes = BitmapToBytes(newImage)
        # cleanup
        g.Dispose()
        newImage.Dispose()
        origImage.Dispose()
        return imageBytes

    def thumbnail(self):
        if self.__thumbnail is None: self.__thumbnail = self.__drawPictureThumbnail()
        return self.__thumbnail

def GetPicture(filename):
    return GetCachedItem(PICTURE, filename)

# FOLDER respesents info about one folder

class FOLDER:
    def __init__(self, path):
        self.path = path
        self.name = Path.GetFileName(self.path)
        self.__thumbnail = None
        self.__parentThumbnail = None

    def __getFolderItems(self, picturesOnly, count):
        if picturesOnly:
            list = []
        else:
            def IsSpecialFolder(dir):
                d = Path.GetFileName(dir).lower()
                return d.startswith('_vti_') or  d.startswith('app_') or d.startswith('bin') or d.startswith('aspnet_')
            list = [GetFolder(d) for d in Directory.GetDirectories(self.path)[:count] if not IsSpecialFolder(d)]
            count -= len(list)
        if count > 0:
            list += [GetPicture(p) for p in Directory.GetFiles(self.path, '*.jpg')[:count]]
        return list


    def GetFirstFolderItems(self, count): return self.__getFolderItems(False, count)

    def GetFolderItems(self): return self.__getFolderItems(False, 10000)

    def GetFolderPictures(self): return self.__getFolderItems(True, 10000)

    def __drawFolderThumbnail(self, parent):
        size = ThumbnailSize
        # create new image
        newImage = Bitmap(size, size)
        g = Graphics.FromImage(newImage)
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        # draw background
        if parent: bc = ParentFolderColor
        else:      bc = ChildFolderColor
        b = LinearGradientBrush(Point(0, 0), Point(size, size), bc, Color.GhostWhite)
        g.FillRectangle(b, 0, 0, size, size)
        b.Dispose()
        g.DrawRectangle(Pens.LightGray, 0, 0, size-1, size-1)
        # draw up to 4 subitems
        folderItems = self.GetFirstFolderItems(4)
        delta = 10
        side = (size-3*delta)/2-1
        rects = (
            Rectangle(delta + 3                , delta + 12                , side, side),
            Rectangle(size / 2 + delta / 2 - 3 , delta + 12                , side, side),
            Rectangle(delta + 3                , size / 2 + delta / 2 + 6  , side, side),
            Rectangle(size / 2 + delta / 2 - 3 , size / 2 + delta / 2 + 6  , side, side))
        for rect, item in zip(rects, folderItems):
            subImage = Bitmap.FromStream(MemoryStream(item.thumbnail()), False)
            g.DrawImage(subImage, rect)
            subImage.Dispose()
        for rect in rects:
            g.DrawRectangle(Pens.LightGray, rect)
        # draw folder name
        if parent: name = '[..]'
        else:      name = Path.GetFileName(self.path)
        f = Font('Arial', 10)
        g.DrawString(name, f, Brushes.Black, RectangleF(2, 2, size-2, size-2))
        f.Dispose()
        # get the bytes of the image
        imageBytes = BitmapToBytes(newImage)
        # cleanup
        g.Dispose()
        newImage.Dispose()
        return imageBytes

    def thumbnail(self):
        if self.__thumbnail is None: self.__thumbnail = self.__drawFolderThumbnail(False)
        return self.__thumbnail

    def parentThumbnail(self):
        if self.__parentThumbnail is None: self.__parentThumbnail = self.__drawFolderThumbnail(True)
        return self.__parentThumbnail

def GetFolder(path):
    return GetCachedItem(FOLDER, path)

# Virtual path helper

class VPATH:
    def __init__(self, str):
        s = str.lower()
        if s != '/' and s.endswith('/'): s = s.rstrip('/')
        if not s.startswith('/') or s.find('/.') >= 0 or s.find('\\') >= 0:
            raise Exception('invalid virtual path %s' % (str))
        self.str = s

    def encode(self):
        return HttpUtility.UrlEncodeUnicode(self.str).Replace('\'', '%27').Replace('%2f', '/').Replace('+', '%20')

    def physicalPath(self): return HostingEnvironment.MapPath(self.str)

    def parent(self):
        s = self.str
        if s == '/': return None
        s = s[0:s.rfind('/')]
        if s == '': s = '/'
        return VPATH(s)

    def child(self, name):
        if self.str == '/': return VPATH('/'+name)
        else:               return VPATH(self.str+'/'+name)

    def name(self):
        s = self.str
        if s == '/': return None
        return s[s.rfind('/')+1:]

    def isRoot(self): return self.str == VPATH.root().str


    def underRoot(self):
        r = VPATH.root().str
        s = self.str
        return r == '/' or r == s or s.startswith(r + '/')

    __rootVPath = None

    @staticmethod
    def root():
        if VPATH.__rootVPath is None: VPATH.__rootVPath = VPATH(Request.FilePath).parent()
        return VPATH.__rootVPath

# Request modes

PreviewImageMode = 1
ThumbnailImageMode = 2
ParentThumbnailImageMode = 3

# IMAGETAG represents data needed to produce an HTML image

class IMAGETAG:
    def __init__(self, mode, vpath):
        p = vpath.encode()
        self.Src = '?mode=%d&path=%s' % (mode, p)
        if mode in (ThumbnailImageMode, ParentThumbnailImageMode):
            if vpath.isRoot(): self.Link = '?'
            else:              self.Link = '?path='+p
            self.Width, self.Height = ThumbnailSize, ThumbnailSize
            self.Alt = 'thumbnail for '+p
        elif mode == PreviewImageMode:
            self.Link = p
            self.Width, self.Height = GetPicture(vpath.physicalPath()).CalcResizedDims(PreviewSize)
            self.Alt = p

# Request Processing

# 'declare' events we could be handling
def Page_Load(sender, e): pass
def Page_Unload(sender, e): pass

# parse the mode from the query string
if Request.QueryString.mode is not None:
    mode = int(Request.QueryString.mode)
else:
    mode = 0

# parse the path from the query string
if Request.QueryString.path is not None:
    vpath = VPATH(Request.QueryString.path)
    if not vpath.underRoot():
        raise HttpException('invalid path - not in the handler scope')
else:
    vpath = VPATH.root()

ppath = vpath.physicalPath()
isFolder = Directory.Exists(ppath)
if not isFolder and not File.Exists(ppath):
    raise HttpException('invalid path - not found')

# perform the action depending on the mode
if mode > 0:
    # response is an image
    if mode == PreviewImageMode:
        imageResponse = GetPicture(ppath).DrawResizedImage(PreviewSize)
    elif mode == ThumbnailImageMode:
        if isFolder: imageResponse = GetFolder(ppath).thumbnail()
        else:        imageResponse = GetPicture(ppath).thumbnail()
    elif mode == ParentThumbnailImageMode:
        imageResponse = GetFolder(ppath).parentThumbnail()
    
    # output the image in Page_Unload event        
    def Page_Unload(sender, e):
        response = HttpContext.Current.Response
        response.Clear()
        response.ContentType = 'image/jpeg'
        response.OutputStream.Write(imageResponse, 0, imageResponse.Length)
        
else:
    pvpath = vpath.parent()

    # response is HTML
    if isFolder:
        # folder view
        def Page_Load(sender, e):
            global ParentLink

            Header.Title = vpath.str
            AlbumMultiView.ActiveViewIndex = 0

            if pvpath is not None and pvpath.underRoot():
                ParentLink = IMAGETAG(ParentThumbnailImageMode, pvpath)
                FolderViewParentLinkSpan.Visible = True

            ThumbnailList.DataSource = [IMAGETAG(ThumbnailImageMode, vpath.child(i.name))
                for i in GetFolder(ppath).GetFolderItems()]
            ThumbnailList.DataBind()
    else:
        def Page_Load(sender, e):
            global ParentLink, PreviousLink, PictureLink, NextLink
            # single picture details view
            AlbumMultiView.ActiveViewIndex = 1

            ParentLink = IMAGETAG(ParentThumbnailImageMode, pvpath)

            prevvpath = None
            nextvpath = None
            found = False

            for pict in GetFolder(pvpath.physicalPath()).GetFolderPictures():
                p = pvpath.child(pict.name)
                if p.str == vpath.str:
                    found = True
                elif found:
                    nextvpath = p
                    break
                else:
                    prevvpath = p

            if not found: prevvpath = None

            if prevvpath is not None:
                PreviousLink = IMAGETAG(ThumbnailImageMode, prevvpath)
                PreviousLinkSpan.Visible = True
            else:
                NoPreviousLinkSpan.Visible = True

            if found:
                PictureLink = IMAGETAG(PreviewImageMode, vpath)
                PictureLinkSpan.Visible = True

            if nextvpath is not None:
                NextLink = IMAGETAG(ThumbnailImageMode, nextvpath)
                NextLinkSpan.Visible = True
            else:
                NoNextLinkSpan.Visible = True
