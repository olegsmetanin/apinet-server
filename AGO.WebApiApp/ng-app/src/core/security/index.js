//from angular-app
// Based loosely around work by Witold Szczerba - https://github.com/witoldsz/angular-http-auth
angular.module('core.security', [
  'security.service',
  'security.interceptor',
  'security.login',
  'security.authorization']);
