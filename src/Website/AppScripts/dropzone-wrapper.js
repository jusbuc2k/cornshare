/// <reference path="../scripts/dropzone.js" />
; (function (window, $, ko, Dropzone) {

    window.corn = window.corn || {};
    var ns = window.corn;

    function DropZoneWrapper(dropZone, options) {
        var self = this;

        var dz = dropZone;
        var successCallbacks = [];
        var options = {
            removeOnSuccess: (options && typeof(options.removeOnSuccess)==='boolean') ? options.removeOnSuccess : true
        }
        
        // private functions

        function processQueue(dz) {
            for (var i = 0; i < dz.files.length; i++) {
                dz.filesQueue.push(dz.files[i]);
            }
            dz.processQueue();
        }

        function clearQueue(dz) {
            for (var i = dz.files.length - 1; i >= 0 ; i--) {
                dz.removeFile(dz.files[i]);
            }
        }

        // public methods

        self.hasPendingFiles = function () {
            return dz.files.length;
        };

        self.hasProcessingFiles = function () {
            return dz.filesProcessing.length;
        };
        
        self.upload = function (callback) {
            successCallbacks.push(callback);
            processQueue(dz);
        };

        self.clear = function () {
            clearQueue(dz);
        };

        self.enable = function () {
            dz.enable();
        };

        self.disable = function () {
            dz.disable();
        };

        // event handlers

        dz.on('success', function (file) {
            var fn;
            if (options.removeOnSuccess) {
                this.removeFile(file);
            }
            if (this.filesQueue.length === 0 && this.filesProcessing.length === 0) {
                while (true) {
                    fn = successCallbacks.pop();
                    if (fn)
                        fn();
                    else
                        break;
                }
            }
        });
    }
    
    ns.DropZoneWrapper = DropZoneWrapper;

})(window, jQuery, ko, window.Dropzone);