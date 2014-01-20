var app = require('http').createServer(handler)
  , io = require('socket.io').listen(app)
  , redis = require("redis")
  , client = redis.createClient()
  , authClient = redis.createClient()
  , login2socket = {}

io.configure(function() {
	io.set('log level', 2);
	io.set('authorization', function (handshakeData, callback) {
		if (handshakeData.login) {
			callback(null, true);
			return;
		}

		if (!handshakeData.query || !handshakeData.query.token) {
			callback("No auth token", false);
			return;
		}

		var token = handshakeData.query.token;
		authClient.get("token:" + token, function(err, reply) {
    		if (reply == null) {
    			callback("Invalid token", false);
    			return;
    		}

    		handshakeData.login = reply;
    		callback(null, true);
		});
	});
})

app.listen(7777);

client.on("ready", function () {
	client.subscribe('reports_changed');
});

io.sockets.on('connection', function (s) {
	var login = s.handshake.login;
	login2socket[login] = login2socket[login] || { login: login, sockets: [] };
	login2socket[login].sockets.push(s);

	s.on('disconnect', function() {
		if (!login2socket[login]) {
			//already no data for this login
			return;
		}
		var socketIndex = login2socket[login].sockets.indexOf(s);
		if (socketIndex >= 0) {
			login2socket[login].sockets.splice(socketIndex, 1);
		}
		if (login2socket[login].sockets.length <= 0) {
			delete login2socket[login];
		}
	});
});

client.on("message", function(channel, message) {
	//assume, that channel is reports_changed and message is ReportTask object
	console.log(message);
	var task = JSON.parse(message);
	if (!login2socket[task.Login]) {
		console.log('User ' + task.Login + ' not connected, remove message');
		console.log(login2socket);
		return;
	}

	var socks = login2socket[task.Login].sockets;
	for(var i = 0; i < socks.length; i++) {
		socks[i].emit('reports', message);
	}
});

function handler (req, res) {
  res.writeHead(200);
  res.end("Ok, i am notification host");
}
