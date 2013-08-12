angular.module('core')
    .service("pageConfig", ['$rootScope',
        function($rootScope) {
            this.current = {};
            this.setConfig = function(newConfig) {
                this.current = newConfig;
                $rootScope.$broadcast('page:configChanged');
            };
        }
    ]);