angular.module('core')
    .directive('ngRelinclude', ['$http', '$templateCache', '$anchorScroll', '$compile', 'sysConfig',
        function($http, $templateCache, $anchorScroll, $compile, sysConfig) {
            return {
                restrict: 'ECA',
                terminal: true,
                compile: function(element, attr) {
                    var srcExp = (attr.ngRelinclude || attr.src),
                        onloadExp = attr.onload || '',
                        autoScrollExp = attr.autoscroll;

                    if (srcExp.charAt(0) === "'") {
                        srcExp = "'" + sysConfig.src(srcExp.substr(1));
                    }

                    return function(scope, element) {
                        var changeCounter = 0,
                            childScope;

                        var clearContent = function() {
                            if (childScope) {
                                childScope.$destroy();
                                childScope = null;
                            }

                            element.html('');
                        };

                        scope.$watch(srcExp, function ngIncludeWatchAction(src) {
                            var thisChangeId = ++changeCounter;

                            if (src) {
                                $http.get(src, {
                                    cache: $templateCache
                                }).success(function(response) {
                                    if (thisChangeId !== changeCounter) {
                                        return;
                                    }

                                    if (childScope) {
                                        childScope.$destroy();
                                    }
                                    childScope = scope.$new();

                                    element.html(response);
                                    $compile(element.contents())(childScope);

                                    if (angular.isDefined(autoScrollExp) && (!autoScrollExp || scope.$eval(autoScrollExp))) {
                                        $anchorScroll();
                                    }

                                    childScope.$emit('$includeContentLoaded');
                                    scope.$eval(onloadExp);
                                }).error(function() {
                                    if (thisChangeId === changeCounter) {
                                        clearContent();
                                    }
                                });
                            } else {
                                clearContent();
                            }
                        });
                    };
                }
            };
        }
    ]);