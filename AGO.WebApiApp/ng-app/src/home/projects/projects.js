angular.module('home')
    .config(['$routeProvider', '$locationProvider', '$stateProvider', '$urlRouterProvider', 'sysConfig',
        function($routeProvider, $locationProvider, $stateProvider, $urlRouterProvider, sysConfig) {

            var projectsList = {
                name: 'page2C.projectList',
                url: '/projects/listview',
                views: {
                    'sidebar': {
                        templateUrl: sysConfig.src('home/projects/listview/projectsListFilter.tpl.html')
                    },
                    'content': {
                        templateUrl: sysConfig.src('home/projects/listview/projectsListGrid.tpl.html')
                    }
                }
            };

            $stateProvider
                .state(projectsList);

        }
    ])
    .service("projectsService", ['$q', '$http', 'sysConfig',
        function($q, $http, sysConfig) {
            this.getProjects = function(opt) {
                var deferred = $q.defer();
                $http.post("/api/v1", {
                    action: "get",
                    model: "projects",
                    filter: opt.filter
                },{tracker:'projects'}).success(function(data, status, headers, config) {
                    deferred.resolve(data);
                }).error(function(data, status, headers, config) {
                    // TODO
                });
                return deferred.promise;
            };
        }
    ])
    .controller('projectsListGridCtrl', ['$scope', 'projectsService', 'pageConfig', 'sysConfig', 'promiseTracker', '$http',
        function($scope, $projectsService, $pageConfig, sysConfig, promiseTracker, $http) {
            $pageConfig.setConfig({
                breadcrumbs: [{
                    name: 'Projects',
                    url: '/#!/projects/listview'
                }]
            });
            $scope.projects = [];

            $scope.loading = promiseTracker('projects');

            $projectsService.getProjects({filter:{}}).then(function (res) {
                $scope.projects = res.projects;
             });

            $scope.moment=new Date();

            $scope.templatesConfig = function(projectId) {
                if (projectId && projectId.indexOf('play') >= 0) {
                    return sysConfig.src('home/projects/listview/details/playProjectDetails.tpl.html');
                } else {
                    return sysConfig.src('home/projects/listview/details/otherProjectDetails.tpl.html');
                }
            };
            $scope.projectDetailsTemplate = '';

            $scope.showDetails = function(projectId) {
                $scope.selectedProjectId = projectId;
                $scope.projectDetailsTemplate = $scope.templatesConfig(projectId);
            };
        }
    ]);