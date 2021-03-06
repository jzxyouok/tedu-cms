﻿(function (app) {
    'use strict';

    app.controller('trainerCtrl', trainerCtrl);

    trainerCtrl.$inject = ['$scope', 'apiService', 'notificationService', '$ngBootbox'];

    function trainerCtrl($scope, apiService, notificationService, $ngBootbox) {
        $scope.loading = true;
        $scope.data = [];

        $scope.search = search;

        $scope.clearSearch = clearSearch;

        $scope.deleteItem = deleteItem;

        $scope.getClassForRow = getClassForRow;

        function getClassForRow(id, parentId) {
            if (parentId == null)
                return 'treegrid-' + id;
            else {
                return 'treegrid-' + id + ' treegrid-parent-' + parentId;
            }
        }

        function deleteItem(id) {
            $ngBootbox.confirm('Bạn có chắc muốn xóa?')
                .then(function () {
                    var config = {
                        params: {
                            id: id
                        }
                    }
                    apiService.del('/api/trainer/delete', config, function () {
                        notificationService.displaySuccess('Đã xóa thành công.');
                        search();
                    },
                    function () {
                        notificationService.displayError('Xóa không thành công.');
                    });
                });
        }

        function search() {
            $scope.loading = true;
            apiService.get('/api/trainer/getlistparent', null, dataLoadCompleted, dataLoadFailed);
        }

        function dataLoadCompleted(result) {
            $scope.data = result.data;
            $scope.loading = false;
            if ($scope.filterExpression && $scope.filterExpression.length) {
                notificationService.displayInfo(result.data.length + ' items found');
            }
        }

        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        function clearSearch() {
            $scope.filterExpression = '';
            search();
        }

        $scope.search();

        //$('.tree').treegrid();
    }
}
)(angular.module('TEDU'));