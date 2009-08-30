///////////////////////////////////////////////////////////////////////////////
// When the page is loaded, place a single hidden Silverlight control on the 
// page, which loads the DLR assemblies, and then pulls DLR code out of the
// HTML page.
///////////////////////////////////////////////////////////////////////////////

if(!window.DLR)
  window.DLR = {}

if(!DLR.loaded) {

  Object.merge = function(dest, src) {
    for(var prop in src) {
      dest[prop] = src[prop]
    }
    return dest
  }

  DLR.parseSettings = function(defaults, settings) {
    // the the full settings dictionary
    var raw_settings = Object.merge(defaults, settings);

    // pull out the DLR-specific configuration options
    var dlr_keys = ['autoAdd', 'debug', 'console', 'start', 'exceptionDetail', 'reportErrors'];
    var dlr_options = {};
    for(d in dlr_keys) {
      key = dlr_keys[d]
      if(raw_settings[key]) {
        dlr_options[key] = raw_settings[key];
        delete raw_settings[key];
      }
    }

    // autoAdd is a special DLR-specific settings; put it on the DLR object
    DLR.autoAdd = dlr_options['autoAdd'];
    delete dlr_options['autoAdd'];
    
    // generate settings.initParams from the rest of the DLR-specific options
    var initParams = "";
    for(opt in dlr_options) {
      initParams += (opt + "=" + dlr_options[opt] + ",")
    }
    initParams = initParams.substring(0, initParams.length - 1);
    raw_settings['initParams'] = initParams;

    return raw_settings;
  }

  DLR.startup = function() {
    DLR.loaded = true;
    if(DLR.autoAdd && Silverlight.isInstalled(null)) {
      var spantag = document.createElement("span");
      document.body.appendChild(spantag);
      slHtml = Silverlight.buildHTML(DLR.settings);
      spantag.innerHTML = slHtml;
    }
  }

  DLR.defaultSettings = {
    autoAdd: true,
    width: 1,
    height: 1,
    onerror: 'Silverlight.default_error_handler',
    reportErrors: 'errorLocation',
    source: 'dlr.xap',
  }

  if(!DLR.settings) {
    DLR.settings = DLR.defaultSettings;
  } else {
    var defaults = DLR.defaultSettings;
    DLR.settings = DLR.parseSettings(defaults, DLR.settings);
  }

  if(window.addEventListener) {
    window.addEventListener('load', DLR.startup, false);
  } else {
    window.attachEvent('onload', DLR.startup);
  }
};
