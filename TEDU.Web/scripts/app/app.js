﻿(function () {
    'use strict'
    angular.module('TEDU', ['ui.router'])
    .config(config);

    config.$inject = ['$stateProvider', '$urlRouterProvider'];

    function config($stateProvider, $urlRouterProvider) {

        $urlRouterProvider.otherwise("/admin");

        $stateProvider
            .state('home', {
                url: "/admin",
                templateUrl: "/scripts/app/home/index.html",
                controller: "homeCtrl"
            })
             .state('categories', {
                 url: '/categories',
                 templateUrl: "/scripts/app/categories/listCategories.html",
                 controller: "categoryCtrl"
             })
             .state('posts', {
                 url: '/posts',
                 templateUrl: "/scripts/app/posts/listPosts.html",
                 controller: "postCtrl"
             });
    }
})();