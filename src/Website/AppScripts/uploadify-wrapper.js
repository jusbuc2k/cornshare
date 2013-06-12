/// <reference path="../scripts/dropzone.js" />
; (function (window, $, ko) {

    window.corn = window.corn || {};
    var ns = window.corn;

    function UploadifyWrapper(uploadify, options) {
        var self = this;

        var up = uploadify;
        var successCallbacks = [];
        var options = {
            removeOnSuccess: (options && typeof(options.removeOnSuccess)==='boolean') ? options.removeOnSuccess : true
        }
        
        // private functions

        // public methods

        self.hasPendingFiles = function () {
            return false;
        };

        self.hasProcessingFiles = function () {
            return false;
        };
        
        self.upload = function (callback) {
            //successCallbacks.push(callback);
            //processQueue(dz);
        };

        self.clear = function () {
            //clearQueue(dz);
        };

        self.enable = function () {
            //dz.enable();
        };

        self.disable = function () {
           //dz.disable();
        };

        // event handlers
    }
    
    ns.UploadifyWrapper = UploadifyWrapper;

})(window, jQuery, ko);