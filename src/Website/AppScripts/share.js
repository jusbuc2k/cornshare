/// <reference path="../scripts/dropzone.js" />
/// <reference path="../scripts/knockout.validation.js" />
/// 
; (function (window, $, ko, Dropzone) {

    window.corn = window.corn || {};
    var ns = window.corn;

    function ajaxPost(url, data) {
        return $.ajax({
            url: url,
            type: "POST",
            dataType: 'json',
            data: data
        });
    }

    function FileModel(data) {
        var self = this;
        data = data || {};

        self.fileID = data.fileID;
        self.fileName = data.fileName;
        self.length = data.length;
        self.thumbUrl = data.thumbUrl;
    }
    
    function DownloadableFileModel(data) {
        FileModel.call(this, data);
        var self = this;
        self.downloadUrl = data.downloadUrl;        
    }

    DownloadableFileModel.prototype = new FileModel();

    function EmailModel() {
        var self = this;

        self.emailFrom = ko.observable().extend({ required: true, email: true });

        self.emailTo = ko.observable().extend({ required: true, email: true });

        self.emailBody = ko.observable().extend({ required: true });

        ko.validation.group(self);
    }

    function ShareModel(options) {
        var self = this;

        function createLinks() {
            $.ajax({
                url: options.createLinksUrl,
                type: "POST",
                dataType: 'json',
                data: {
                    "fileSetID": options.fileSetID,
                    "name": self.name(),
                    "requirePassword": self.requirePassword(),
                    "password": self.password(),
                    "expirationDate": self.expirationDate(),
                    "allowUpload": self.allowUpload()
                }
            }).done(function (result) {
                if (result.success) {
                    self.generatedLinks.fileSet(result.setLink);
                    self.generatedLinks.files.removeAll();
                    self.viewIndex(1);

                    if (result.fileLinks) {
                        for (var i = 0; i < result.fileLinks.length; i++) {
                            self.generatedLinks.files.push(result.fileLinks[i]);
                        }
                    }
                } else {
                    alert(result.errorMessage);
                }
                self.controlsEnabled(true);
            }).error(function (xhr, textStatus, status) {
                alert(textStatus);
                self.controlsEnabled(true);
            });
        }

        function sendEmail(shareToken) {
            ajaxPost(options.sendEmail,
                {
                    "fileSetID": options.fileSetID,
                    "expirationDate": self.expirationDate(),
                    "allowUpload": self.allowUpload(),
                    "requirePassword": self.requirePassword(),
                    "from": self.emailMessage.emailFrom(),
                    "to": self.emailMessage.emailTo(),
                    "body": self.emailMessage.emailBody(),
                    "link": self.emailLink()
                }
            ).done(function (result) {
                if (result.success) {
                    alert('Email Sent!');
                } else {
                    alert('Email failed...');
                }
                self.emailVisible(false);
            });
        }

        // FILESET PROPERTIES 

        self.name = ko.observable(options.name).extend({
            required: true
        });

        self.requirePassword = ko.observable(false);

        self.password = ko.observable().extend({
            required: { onlyIf: self.requirePassword }
        });

        self.allowUpload = ko.observable(false);

        self.changeExpirationDate = ko.observable(false);

        self.expirationDate = ko.observable(new Date(new Date().getTime() + (86400000 * 30)).toLocaleDateString()).extend({
            required: true,
            date: true
        });

        // UI STATE

        self.viewIndex = ko.observable(0);

        self.controlsEnabled = ko.observable(false);

        self.fallback = ko.observable(false);

        self.uploadedFileCount = ko.observable(options.fileCount);

        self.isShareEnabled = ko.computed(function () {
            return self.controlsEnabled() && (self.uploadedFileCount() || self.allowUpload());
        });

        // Object References

        self.uploaderProxy = null;

        self.generatedLinks = {
            fileSet: ko.observable(''),
            files: ko.observableArray()
        };

        self.emailVisible = ko.observable(false);

        self.emailMessage = new EmailModel();

        self.emailLink = ko.observable();

        // event handlers

        self.onShareClicked = function () {
            if (!self.isValid()) {
                self.errors.showAllMessages();
                return;
            }

            if (this.uploaderProxy.hasProcessingFiles()) {
                return;
            }
            self.controlsEnabled(false);
            createLinks();
        };

        self.onClearClicked = function () {
            self.uploaderProxy.clear();
        };

        self.onChangeExpirationDateClicked = function () {
            self.changeExpirationDate(true);
        };

        self.onCreateEmailClicked = function (obj) {
            var body = '';

            if (obj.generatedLinks) {
                if (self.generatedLinks.files().length > 0) {
                    body += 'Hello,\n\nI created a link for you to download the following files:\n\n';
                    for (var i = 0; i < self.generatedLinks.files().length; i++) {
                        body += (' - ' + self.generatedLinks.files()[i].fileName + '\n');
                    }
                    if (self.allowUpload()) {
                        body += '\nYou can also upload your own files to the share.\n';
                    }
                } else {
                    body += 'Hello,\n\nI created a link where you can upload files.\n';
                }

                self.emailLink(obj.generatedLinks.fileSet());
            } else if (obj.url) {
                body += 'Hello,\n\nI created a link for you to download the file \'' + obj.fileName + '\'.\n';
                self.emailLink(obj.url);
            }

            body += '\nThanks!';
            self.emailMessage.emailBody(body);
            self.emailVisible(true);
        };

        self.onSendEmailClicked = function () {
            if (self.emailMessage.isValid()) {
                sendEmail();
            } else {
                self.emailMessage.errors.showAllMessages();
            }
        };

        self.cancelEmailClicked = function () {
            self.emailVisible(false);
        };

        // INIT 

        ko.validation.group(self);

        return self;
    }

    function LinkModel(options, data) {
        var self = this;

        function createThumbUrl(fileID) {
            return options.baseUrl + '/Thumb/' + fileID;
        }

        function createDownloadUrl(fileID) {
            return options.baseUrl + '/Download/' + fileID;
        }

        function loadFiles(data) {
            return ko.utils.arrayMap(data, function (file) {
                return new DownloadableFileModel({
                    fileID: file.FileID,
                    fileName: file.FileName,
                    length: file.Length,
                    downloadUrl: createDownloadUrl(file.FileID),
                    thumbUrl: createThumbUrl(file.FileID)
                });
            });
        }

        function validatePassword() {
            return ajaxPost(
                options.baseUrl,
                {
                    shareToken: self.shareToken(),
                    password: self.password()
                }).done(function (result) {
                    self.passwordIsValid(result.PasswordIsValid);
                    self.allowUpload(result.AllowUpload);
                    self.fileSetName(result.FileSetName);
                    if (result.Files) {
                        self.files(loadFiles(result.Files));
                    }
                });
        }

        self.uploaderProxy = null;

        self.controlsEnabled = ko.observable(false);

        self.shareToken = ko.observable(data.ShareToken);

        self.fileSetName = ko.observable(data.FileSetName);

        self.files = ko.observable(loadFiles(data.Files));

        self.fallback = ko.observable(false);

        self.password = ko.observable();

        self.passwordRequired = ko.observable(data.PasswordRequired);

        self.passwordIsValid = ko.observable(data.PasswordIsValid);

        self.allowUpload = ko.observable(data.AllowUpload);

        self.passwordInputVisible = ko.computed(function () {
            return self.passwordRequired() && !self.passwordIsValid();
        });

        self.downloadVisible = ko.computed(function () {
            return !self.passwordRequired() || self.passwordIsValid();
        });

        self.uploadVisible = ko.computed(function () {
            return self.allowUpload() && self.downloadVisible();
        });

        // functions

        self.download = function (file) {
            window.location.href = file.downloadUrl;
        };

        self.refresh = function () {
            validatePassword();
        };

        // event handlers

        self.passwordIsValid.subscribe(function (value) {
            if (value) {
                self.uploaderProxy.enable();
            } else {
                self.uploaderProxy.disable();
            }
        });

        self.onValidatePasswordClicked = function () {
            validatePassword();
        };

        self.onUploadClicked = function () {
            self.controlsEnabled(false);
            self.uploaderProxy.upload(function () {
                self.controlsEnabled(true);
            });
        };

        self.onClearClicked = function () {
            self.uploaderProxy.clear();
        };
    }

    ns.ShareModel = ShareModel;
    ns.LinkModel = LinkModel;

    ns.FileModel = FileModel;
    ns.DownloadableFileModel = DownloadableFileModel;

})(window, jQuery, ko, window.Dropzone);