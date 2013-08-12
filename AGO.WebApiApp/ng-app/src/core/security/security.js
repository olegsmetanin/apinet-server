// Based loosely around work by Witold Szczerba - https://github.com/witoldsz/angular-http-auth
angular.module('security.service', [
  'security.retryQueue',    // Keeps track of failed requests that need to be retried once the user logs in
  'security.login',         // Contains the login form template and controller
  'ui.bootstrap.dialog'     // Used to display the login form as a modal dialog.
])

.factory('security', ['$http', '$q', '$location', 'securityRetryQueue', '$dialog', 'currentProject', 'userGroups', 'sysConfig',
  function($http, $q, $location, queue, $dialog, currentProject, userGroups, sysConfig) {

  // Redirect to the given url (defaults to '/')
  function redirect(url) {
    url = url || '/';
    $location.path(url);
    window.location.href = '/';
  }

  // Login form dialog stuff
  var loginDialog = null;
  function openLoginDialog() {
    if ( loginDialog ) {
      throw new Error('Trying to open a dialog that is already open!');
    }
    loginDialog = $dialog.dialog();
    loginDialog.open(sysConfig.src('core/security/login/form.tpl.html'), 'LoginFormController').then(onLoginDialogClose);
  }
  function closeLoginDialog(success) {
    if (loginDialog) {
      loginDialog.close(success);
    }
  }
  function onLoginDialogClose(success) {
    loginDialog = null;
    if ( success ) {
      queue.retryAll();
    } else {
      queue.cancelAll();
      redirect();
    }
  }

  // Register a handler for when an item is added to the retry queue
  queue.onItemAddedCallbacks.push(function(retryItem) {
    if ( queue.hasMore() ) {
      service.showLogin();
    }
  });

  // The public API of the service
  var service = {

    // Get the first reason for needing a login
    getLoginReason: function() {
      return queue.retryReason();
    },

    // Show the modal login dialog
    showLogin: function() {
      openLoginDialog();
    },

    // Attempt to authenticate a user by the given email and password
    login: function(email, password) {
      var request = $http.post('/login', {email: email, password: password});
      return request.then(function(response) {
        service.currentUser = response.data.user;
        if ( service.isAuthenticated() ) {
          closeLoginDialog(true);
        }
        service.currentUserGroups = null;
      });
    },

    // Give up trying to login and clear the retry queue
    cancelLogin: function() {
      closeLoginDialog(false);
      redirect();
    },

    // Logout the current user and redirect
    logout: function(redirectTo) {
      $http.post('/logout').then(function() {
        service.currentUser = null;
        redirect(redirectTo);
      });
    },

    // Ask the backend to see if a user is already authenticated - this may be from a previous session.
    requestCurrentUser: function() {
      if ( service.isAuthenticated() ) {
        return $q.when(service.currentUser);
      } else {
        return $http.post('/current-user').then(function(response) {
          service.currentUser = response.data.user;
          return service.currentUser;
        });
      }
    },

    // Information about the current user
    currentUser: null,
    currentUserGroups: userGroups,

    // Is the current user authenticated?
    isAuthenticated: function(){
      return !!service.currentUser;
    },

    // Is the current user an adminstrator?
    isAdmin: function() {
      return !!(service.currentUser && service.currentUser.admin);
    },

    //Ask the backend about user groups in provided project
    requestUserGroups: function(proj) {
      if ( !service.isAuthenticated() ) {
        return service.requestCurrentUser().then(function() {
          return $http.post('/user-groups', {project: proj}).then(function(response) {
            service.currentUserGroups = response.data.groups;
            return service.currentUserGroups;
          });
        });
      }
      else if (service.currentUserGroups !== null) {
        return $q.when(userGroups);
      }
      else {
        return $http.post('/user-groups', {project: proj}).then(function(response) {
          service.currentUserGroups = response.data.groups;
          return service.currentUserGroups;
        });
      }
    }
  };

  return service;
}]);
