angular.module('core', ['ui.state', 'ui.bootstrap', 'core.security', 'core.templates', 'ajoslin.promise-tracker','angularMoment']);

angular.module('core')
    .config(['$routeProvider', '$locationProvider', '$stateProvider', '$urlRouterProvider', 'sysConfig',
        function($routeProvider, $locationProvider, $stateProvider, $urlRouterProvider, sysConfig) {
            $locationProvider.hashPrefix('!');

            var page1C = {
                name: "page1C",
                abstract: true,
                templateUrl: sysConfig.src('core/masterpages/page1C.tpl.html')
            },
                page2C = {
                    name: "page2C",
                    abstract: true,
                    templateUrl: sysConfig.src('core/masterpages/page2C.tpl.html')
                },
                tabPage1C = {
                    name: "tabPage1C",
                    abstract: true,
                    templateUrl: sysConfig.src('core/masterpages/tabPage1C.tpl.html')
                },
                tabPage2C = {
                    name: "tabPage2C",
                    abstract: true,
                    templateUrl: sysConfig.src('core/masterpages/tabPage2C.tpl.html')
                };

            $stateProvider
                .state(page1C)
                .state(page2C)
                .state(tabPage1C)
                .state(tabPage2C);
        }
    ])
    .constant('I18N.MESSAGES', {
      'errors.route.changeError':'Route change error',
      'crud.user.save.success':"A user with id '{{id}}' was saved successfully.",
      'crud.user.remove.success':"A user with id '{{id}}' was removed successfully.",
      'crud.user.remove.error':"Something went wrong when removing user with id '{{id}}'.",
      'crud.user.save.error':"Something went wrong when saving a user...",
      'crud.project.save.success':"A project with id '{{id}}' was saved successfully.",
      'crud.project.remove.success':"A project with id '{{id}}' was removed successfully.",
      'crud.project.save.error':"Something went wrong when saving a project...",
      'login.reason.notAuthorized':"You do not have the necessary access permissions.  Do you want to login as someone else?",
      'login.reason.notAuthenticated':"You must be logged in to access this part of the application.",
      'login.error.invalidCredentials': "Login failed.  Please check your credentials and try again.",
      'login.error.serverError': "There was a problem with authenticating: {{exception}}."
    })
    .controller('HeaderCtrl', ['$scope', 'security', function($scope, security) {
        $scope.isAuthenticated = security.isAuthenticated;
    }])
    .run(['security', function(security) {
      // Get the current user when the application starts
      // (in case they are still logged in from a previous session or on project change)
      security.requestCurrentUser();
    }]);