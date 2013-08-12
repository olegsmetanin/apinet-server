angular.module('core')
    .controller('breadcrumbsCtrl', ['$scope', 'pageConfig',
        function($scope, $pageConfig) {
            $scope.breadcrumbs = $pageConfig.current.breadcrumbs;
            $scope.$on('page:configChanged', function() {
                $scope.breadcrumbs = $pageConfig.current.breadcrumbs;
            });

            $scope.isHref = function(index) {
                var res = true;
                if ((index === ($scope.breadcrumbs.length - 1)) || ($scope.breadcrumbs[index].url === '')) {
                    res = false;
                }
                return res;
            };
        }
    ]);