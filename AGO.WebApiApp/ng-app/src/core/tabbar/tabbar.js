angular.module('core')
    .directive("tabbar", function() {
        return function($scope, element, attrs) {
            var tabs = $scope.tabs,
                currentIndex = attrs.tabbar*1,
                html = '' + '<div class="navbar">' + '<div class="navbar-inner">' + '<ul class="nav">';
            for (var i = 0; i < tabs.length; i++) {
                html += '<li ' + (currentIndex === i ? 'class="active"' : '') + '><a href="' + tabs[i].url + '">' + tabs[i].name + '</a></li>';
            }
            html = html + '</ul>' + '</div>' + '</div>';
            element.html(html);
        };
    });