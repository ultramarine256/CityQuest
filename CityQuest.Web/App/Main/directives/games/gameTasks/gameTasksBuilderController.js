﻿(function () {
    var app = angular.module('app');
    app.directive('gameTasksBuilder', ['clientCityQuestConstService', 'clientPermissionService', 'abp.services.cityQuest.gameTask',
        function (constSvc, permissionSvc, gameTaskSvc) {
        return {
            restrict: 'E',
            templateUrl: '/App/Main/directives/games/gameTasks/gameTasksBuilderTemplate.cshtml',
            scope: {},
            bindToController: {
                templateMode: '=',
                gameId: '=',
                gameTasks: '=',
                gameTaskTypes: '=',
                conditionTypes: '=',
            },
            controllerAs: 'gameTasksBuilder',
            controller: function ($scope) {
                var vm = this;

                vm.localize = constSvc.localize;

                //---------------------------------------------------------------------------------------------------------
                //-----------------------------------------Load promise----------------------------------------------------
                /// Is used to store current load promise
                vm.loadPromise = null;
                //---------------------------------------------------------------------------------------------------------
                //----------------------------------------Template's modes-------------------------------------------------
                /// Is used to get bool result for conmaring template's mode with standart ones
                vm.templateModeState = {
                    templateMode: vm.templateMode,
                    isInfo: function () {
                        return vm.templateModeState.templateMode &&
                            (vm.templateModeState.templateMode == constSvc.formModes.info);
                    },
                    isCreate: function () {
                        return vm.templateModeState.templateMode &&
                            (vm.templateModeState.templateMode == constSvc.formModes.create);
                    },
                    isUpdate: function () {
                        return vm.templateModeState.templateMode &&
                            (vm.templateModeState.templateMode == constSvc.formModes.update);
                    }
                };
                //---------------------------------------------------------------------------------------------------------
                //----------------------------------Template's actions service---------------------------------------------
                /// Is used to allow actions for gameTaskBuilder
                vm.gameTaskPermissionsOnActions = {
                    canReloadGameTasks: function () {
                        return vm.templateModeState.isUpdate() && vm.gameId && vm.gameId > 0;;
                    },
                    canAddGameTaskOnTop: function () {
                        return vm.templateModeState.isCreate() || vm.templateModeState.isUpdate();
                    },
                    canAddGameTaskOnBottom: function () {
                        return vm.templateModeState.isCreate() || vm.templateModeState.isUpdate();
                    },
                    canDeleteGameTask: function (entity) {
                        return vm.templateModeState.isCreate() || vm.templateModeState.isUpdate();
                    },
                    canDeleteAllGameTasks: function () {
                        return vm.templateModeState.isCreate() || vm.templateModeState.isUpdate();
                    },
                    canMoveGameTaskToTop: function (entity) {
                        return vm.templateModeState.isCreate() || vm.templateModeState.isUpdate();
                    },
                    canMoveGameTaskToBottom: function (entity) {
                        return vm.templateModeState.isCreate() || vm.templateModeState.isUpdate();
                    },
                    canMoveGameTaskUp: function (entity) {
                        return vm.templateModeState.isCreate() || vm.templateModeState.isUpdate();
                    },
                    canMoveGameTaskDown: function (entity) {
                        return vm.templateModeState.isCreate() || vm.templateModeState.isUpdate();
                    },
                    canMinimize: function (entity) {
                        return entity && !entity.isMinimized;
                    },
                    canMaximize: function (entity) {
                        return entity && entity.isMinimized;
                    },
                    canActivateGameTask: function (entity) {
                        return entity && (vm.templateModeState.isCreate() || vm.templateModeState.isUpdate()) && !entity.isActive;
                    },
                    canDeactivateGameTask: function (entity) {
                        return entity && (vm.templateModeState.isCreate() || vm.templateModeState.isUpdate()) && entity.isActive;
                    },
                };
                //---------------------------------------------------------------------------------------------------------
                //---------------------------------------Game task actions-------------------------------------------------
                /// Is used to store actions can be allowed in gameTaskBuilder
                vm.gameTaskActions = {
                    reloadGameTasks: function () {
                        vm.loadPromise = gameTaskSvc.retrieveGameTasksForGame({
                            GameId: vm.gameId
                        }).success(function (data) {
                            vm.gameTasks = data.gameTasks;
                        });
                        return vm.loadPromise;
                    },
                    setGameTasksOrders: function () {
                        angular.forEach(vm.gameTasks, function (value, key) {
                            value.order = key + 1;
                        });
                        return vm.gameTasks;
                    },
                    getNewGameTask: function (currentGameId) {
                        var emptyGameTask = {
                            gameId: currentGameId,
                            gameTaskTypeId: null,
                            tips: [],
                            conditions: [],
                            name: null,
                            description: null,
                            taskText: null,
                            isActive: true,
                            order: null,
                        };
                        return emptyGameTask;
                    },
                    addGameTaskOnTop: function () {
                        var newTask = vm.gameTaskActions.getNewGameTask(vm.gameId);
                        vm.gameTasks.unshift(newTask);
                        vm.gameTaskActions.setGameTasksOrders();
                        return vm.gameTasks;
                    },
                    addGameTaskOnBottom: function () {
                        var newTask = vm.gameTaskActions.getNewGameTask(vm.gameId);
                        newTask.order = vm.gameTasks.length + 1;
                        vm.gameTasks.push(newTask);
                        return vm.gameTasks;
                    },
                    deleteGameTask: function (order) {
                        var index = order - 1;
                        if (!(index > -1 && index < vm.gameTasks.length))
                            return false;

                        vm.gameTasks.splice(index, 1);
                        vm.gameTaskActions.setGameTasksOrders();
                        return vm.gameTasks;
                    },
                    deleteAllGameTasks: function () {
                        vm.gameTasks = [];
                    },
                    moveGameTaskToTop: function (order) {
                        var index = order - 1;
                        var movingGameTask = vm.gameTasks[index];
                        vm.gameTasks.splice(index, 1);
                        vm.gameTasks.unshift(movingGameTask);
                        vm.gameTaskActions.setGameTasksOrders();
                        return vm.gameTasks;
                    },
                    moveGameTaskToBottom: function (order) {
                        var index = order - 1;
                        var movingGameTask = vm.gameTasks[index];
                        vm.gameTasks.splice(index, 1);
                        vm.gameTasks.push(movingGameTask);
                        vm.gameTaskActions.setGameTasksOrders();
                        return vm.gameTasks;
                    },
                    swapGameTasks: function (order1, order2) {
                        var index1 = order1 - 1;
                        var index2 = order2 - 1;

                        if (!(index1 != index2 &&
                            index1 > -1 && index1 < vm.gameTasks.length && 
                            index2 > -1 && index2 < vm.gameTasks.length))
                            return false;

                        var store = vm.gameTasks[index1];
                        vm.gameTasks[index1] = vm.gameTasks[index2];
                        vm.gameTasks[index2] = store;

                        vm.gameTaskActions.setGameTasksOrders();

                        return vm.gameTasks;
                    },
                    moveGameTaskUp: function (order) {
                        var index = order - 1;

                        if (!(index > 0 && index < vm.gameTasks.length))
                            return false;

                        vm.gameTaskActions.swapGameTasks(index, order);
                        return vm.gameTasks;
                    },
                    moveGameTaskDown: function (order) {
                        var index = order - 1;

                        if (!(index > -1 && index < (vm.gameTasks.length - 1)))
                            return false;

                        vm.gameTaskActions.swapGameTasks(order, (order + 1));
                        return vm.gameTasks;
                    },
                    minimize: function (order) {
                        var index = order - 1;

                        if (!(index > -1 && index < vm.gameTasks.length))
                            return false;

                        (vm.gameTasks[index]).isMinimized = true;
                        return vm.gameTasks;
                    },
                    maximize: function (order) {
                        var index = order - 1;

                        if (!(index > -1 && index < vm.gameTasks.length))
                            return false;

                        (vm.gameTasks[index]).isMinimized = false;
                        return vm.gameTasks;
                    },
                    activateGameTask: function (order) {
                        var index = order - 1;

                        if (!(index > -1 && index < vm.gameTasks.length))
                            return false;

                        (vm.gameTasks[index]).isActive = true;
                        return vm.gameTasks;
                    },
                    deactivateGameTask: function (order) {
                        var index = order - 1;

                        if (!(index > -1 && index < vm.gameTasks.length))
                            return false;

                        (vm.gameTasks[index]).isActive = false;
                        return vm.gameTasks;
                    },
                };
                //---------------------------------------------------------------------------------------------------------
            }
        }
    }]);
})();