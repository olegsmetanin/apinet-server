angular.module('core')
    .directive('filterComplex', ['$timeout',
        function($timeout) {
            return {
                scope: {
                    filterNgModel: "=",
                    meta: "="
                },
                compile: function(element, attrs, transclude) {

                    return function($scope, element, attrs, filterNgModelCtrl) {
                        var path = attrs.path.replace(/'/g, '');

                        element.bind('filterChange', function() {
                            if (!$scope.$$phase) {

                                var oldVal = $scope.filterNgModel,
                                    newVal = {
                                        state: {
                                            path: path
                                        },
                                        val: element.structuredFilter('data')
                                    };

                                $scope.$apply(function() {
                                    $scope.filterNgModel = newVal;
                                });
                            }
                        });

                        $scope.$parent.$watch(attrs.filterNgModel, function(newVal, oldVal, scope) {
                            if ((newVal) && (newVal !== oldVal)) {

                                $timeout(function() {
                                    element.structuredFilter('data', newVal.val);
                                });

                            }
                        });


                        $timeout(function() {
                            if (jQuery().structuredFilter) {
                                element.structuredFilter({
                                    meta: $scope.meta,
                                    path: path
                                });
                            }
                        });
                    };
                },

                controller: ['$scope', '$element', '$attrs', '$transclude',
                    function($scope, $element, $attrs, $transclude) {}
                ]
            };
        }
    ])
    .directive('filterUser', ['$timeout',
        function($timeout) {
            return {
                scope: {
                    filterNgModel: "=",
                    meta: "="
                },
                compile: function(element, attrs, transclude) {

                    return function($scope, element, attrs, filterNgModelCtrl) {
                        var path = attrs.path.replace(/'/g, '');

                        element.bind('filterChange', function() {
                            if (!$scope.$$phase) {

                                var oldVal = $scope.filterNgModel,
                                    newVal = {
                                        state: {
                                            path: path
                                        },
                                        val: element.structuredFilter('data')
                                    };

                                $scope.$apply(function() {
                                    $scope.filterNgModel = newVal;
                                });
                            }
                        });

                        $scope.$parent.$watch(attrs.filterNgModel, function(newVal, oldVal, scope) {
                            if ((newVal) && (newVal !== oldVal)) {

                                $timeout(function() {
                                    element.structuredFilter('data', newVal.val);
                                });

                            }
                        });


                        $timeout(function() {
                            if (jQuery().structuredFilter) {
                                element.structuredFilter({
                                    meta: $scope.meta,
                                    path: path
                                });
                            }
                        });
                    };
                },

                controller: ['$scope', '$element', '$attrs', '$transclude',
                    function($scope, $element, $attrs, $transclude) {}
                ]
            };
        }
    ]);