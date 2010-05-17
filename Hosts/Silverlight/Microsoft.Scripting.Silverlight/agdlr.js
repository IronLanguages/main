SilverlightDlrWindow = function() { 
}
SilverlightDlrWindow.prototype = {
  forEachPanel: function(check, action) {
    element = this.getElement('silverlightDlrWindow')
    for(i = 0; i < element.childNodes.length; i++) {
      e = element.childNodes[i]
      if(check(e, this)) {
        action(e, this)
      }
    }
  },

  isDiv: function(e) {
    return e.tagName == "DIV" && e.id != "silverlightDlrWindowMenu"
  },

  isDivAndNot: function(id, e) {
    return e.tagName == "DIV" && (e.id != id && e.id != "silverlightDlrWindowMenu")
  },

  hideElement: function(e) {
    e.style.display = "none"
  },

  showElement: function(e) {
    e.style.display = "block"
  },

  hasClass: function(ele, cls) {
    return ele.className.match(new RegExp('(\\s|^)'+cls+'(\\s|$)'));
  },
  addClass: function(ele, cls) {
    if (!this.hasClass(ele, cls)) 
      ele.className += " " + cls;
  },
  removeClass: function(ele, cls) {
    if (this.hasClass(ele, cls)) {
      var reg = new RegExp('(\\s|^)'+cls+'(\\s|$)');
      ele.className = ele.className.replace(reg,' ');
    }
  },

  getElement: function(id) {
    return document.getElementById(id)
  },

  hideAllOtherPanels: function(id) {
    this.forEachPanel(function(e, obj) { return obj.isDivAndNot(id, e) }, function(e, obj) {
      obj.hideElement(e)
    })
    return false
  },

  showPanel: function(id) {
    e = this.getElement(id)
    if(e) {
      this.showElement(e)
      this.setLinkActive(this.getLinkId(e))
      this.hideAllOtherPanels(id)
    }
    return false
  },

  hideAllPanels: function(link) {
    this.forEachPanel(function(e, obj) { return obj.isDiv(e) }, function(e, obj) {
      obj.hideElement(e)
    })
    this.setLinkActive(link.id);

    return false
  },
  
  getLinkId: function(element) {
    return element.id + "Link"
  },

  setLinkActive: function(id) {
    e = this.getElement(id)
    if(!this.hasClass(e, 'active')) {
      this.addClass(e, 'active')
      this.setLinkInactive(activeLink)
      activeLink = e
    }
  },

  setLinkInactive: function(e) {
    if(e != null && this.hasClass(e, 'active')) {
      this.removeClass(e, 'active')
      activeLink = null
    }
  },
  
  initialize: function() {
    activeFound = false
    sdlrw.forEachPanel(function(e, obj) { return obj.isDiv(e) }, function(e, obj) {
      if(!activeFound && e.style.display != "none") {
        activeFound = true
        obj.showPanel(e.id)
      }
    })
    if(!activeFound) {
      sdlrw.hideAllPanels()
    }
  }
}
var sdlrw = new SilverlightDlrWindow
var activeLink = null
