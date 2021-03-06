﻿(function (app) {
    'use strict';

    app.controller('addPostCtrl', addPostCtrl);

    addPostCtrl.$inject = ['$scope', 'apiService', 'notificationService', '$location', 'commonService'];

    function addPostCtrl($scope, apiService, notificationService, $location, commonService) {
        $scope.post = {
            CreatedDate: new Date(),
            Status: "Published"
        }
        // setup editor options
        $scope.editorOptions = {
            language: 'vi',
            height: '200px'
        };
        $scope.CreateAlias = createAlias;
        $scope.ChooseImage = chooseImage;
        $scope.categories = [];

        function loadListParents() {
            apiService.get('/api/category/getlistparent', null, function (result) {
                $scope.categories = result.data;
            });
        }

        $scope.AddPost = addPost;

        function createAlias() {
            $scope.post.Alias = commonService.makeSeoTitle($scope.post.Name);
        }
        function chooseImage() {
            var finder = new CKFinder();
            finder.selectActionFunction = function (fileUrl) {
                $scope.$apply(function() {
                    $scope.post.Image = fileUrl;
                });

            };
            finder.popup();
        }
        function addPost() {
            apiService.post('/api/post/add', $scope.post, addSuccessed, addFailed);
        }

        function addSuccessed(response) {
            notificationService.displaySuccess($scope.post.Name + ' đã được thêm mới.');
            $location.url('posts');
        }
        function addFailed(response) {
            notificationService.displayError(response.statusText);
        }

        loadListParents();
    }
})(angular.module('TEDU.posts'));