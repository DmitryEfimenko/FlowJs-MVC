'use strict';

angular.module('app.controllers').controller('PicUploadController', [
    '$scope', 'miscService', function ($scope, miscService) {

        $scope.files = [];

        $scope.fileUploadSuccess = function (message) {
            $scope.files.push(JSON.parse(message));
        };

        $scope.fileUploadProgress = function (progress) {
            $scope.fileProgress = Math.round(progress * 100);
        };

        $scope.uploadError = function (message) {
            var jsonResponse = JSON.parse(message);
            var modelState = jsonResponse.modelState;
            $scope.modelState = modelState;
        };

        $scope.validateFile = function ($file) {
            $scope.modelState = undefined;
            $scope.formUpload.$serverErrors = undefined;
            var allowedExtensions = ['jpeg', 'jpg', 'bmp', 'png'];
            var isValidType = allowedExtensions.indexOf($file.getExtension()) >= 0;
            if (!isValidType) $scope.modelState = { file: ['type'] };
            return isValidType;
        };

        $scope.getSecurityHeaders = function () {
            return miscService.getSecurityHeaders();
        };
    }
]);
