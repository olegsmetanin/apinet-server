angular.module('home', ['core', 'ui.state', 'home.templates']);

angular.module('home')
    .config(['$routeProvider', '$locationProvider', '$stateProvider', '$urlRouterProvider', 'sysConfig',
        function($routeProvider, $locationProvider, $stateProvider, $urlRouterProvider, sysConfig) {

            $urlRouterProvider.otherwise('/projects/listview');

            home = {
                name: 'page1C.home',
                url: '/',
                views: {
                    'content': {
                        templateUrl: sysConfig.src('src/home/home.tpl.html')
                    }
                }
            };

            $stateProvider
                .state(home);
        }
    ])
    .controller('homeCtrl', ['$scope', '$stateParams', 'pageConfig',
        function($scope, $stateParams, $pageConfig) {

            $pageConfig.setConfig({
                breadcrumbs: [{
                        name: 'Home',
                        url: '/'
                    }

                ]
            });
        }
    ]);