function Profile(data) {
	var Self = this;
	Self.Id = ko.observable();
	Self.DisplayName = ko.observable();
	Self.Link = ko.observable();
	if (!data) { return; }
	Self.Id(data.Id);
	Self.DisplayName(data.DisplayName);

	Self.Link('/speakers.html?id=' + data.Id);
}

function TimeSlot(data) {
	var Self = this;
	Self.StartTime = ko.observable();
	Self.EndTime = ko.observable();
	Self.DisplayDateTime = ko.observable();

	if (data) {
		Self.StartTime(new Date(data.StartTime));
		Self.EndTime(new Date(data.EndTime));
		Self.DisplayDateTime(data.DisplayDateTime);
	}
}

function Session(data) {
	var Self = this;
	Self.Id = ko.observable();
	Self.Speaker = ko.observable();
	Self.Title = ko.observable();
	Self.Abstract = ko.observable();
	Self.Room = ko.observable();
	Self.TimeSlot = ko.observable();
	Self.Level = ko.observable();
	Self.Tags = ko.observableArray([]);

	Self.TagList = ko.pureComputed(function () {
		var TagList = '';
		for (var index = 0; index < this.Tags().length; ++index)
			TagList += this.Tags()[index].Text() + '; ';
		return TagList;
	}, Self);

	if (!data) { return; }
	Self.Id(data.Id);
	Self.Speaker(new Profile(data.Speaker));
	Self.Title(data.Title);
	Self.Abstract('<p>' + data.Abstract.trim().replace(/\r\n/g, '\n').replace(/\n\n/g, '\n').replace(/\n/g, '</p><p>') + '</p>');
	Self.Room(data.Room);
	Self.TimeSlot(new TimeSlot(data.TimeSlot));
	Self.Level(new Tag(data.Level));

	if (!data.Tags) { return; }
	for (var index = 0; index < data.Tags.length; ++index) {
		if (ko.isObservable(data.Tags[index])) {
			Self.Tags.push(data.Tags[index]);
		} else {
			Self.Tags.push(new Tag(data.Tags[index]));
		}
	}
}

function Tag(data) {
	var Self = this;
	Self.Id = ko.observable(data.Id);
	Self.Text = ko.observable(data.Text);
	Self.Link = ko.observable('sessions.html?tagId=' + data.Id);
}

function ViewModel() {
	var Self = this;
	Self.Sessions = ko.observableArray([]);

	var qd = null;
	if (location.search) {
		qd = {};
		location.search.substr(1).split("&").forEach(function (item) { var s = item.split("="), k = s[0], v = s[1] && decodeURIComponent(s[1]); (k in qd) ? qd[k].push(v) : qd[k] = [v] });
	}

	var SessionsRequest = new XMLHttpRequest();
	if (!qd) {
		SessionsRequest.open('GET', `${currentEnvironment}/session`, true);
		SessionsRequest.send();
		return;
	}

	if (qd.id) {
		SessionsRequest.open('GET', `${currentEnvironment}/session/` + qd.id, true);
	} else if (qd.tagId) {
		SessionsRequest.open('GET', `${currentEnvironment}/session/tag/` + qd.tagId, true);
	} else {
		SessionsRequest.open('GET', `${currentEnvironment}/session`, true);
	}
	SessionsRequest.send();

	SessionsRequest.onreadystatechange = function () {
		if (SessionsRequest.readyState !== SessionsRequest.DONE) { return; }
		switch (SessionsRequest.status) {
			case 200:
				var SessionList = JSON.parse(SessionsRequest.responseText);
				if (SessionList.length) {
					for (var index = 0; index < SessionList.length; ++index) {
						Self.Sessions.push(new Session(SessionList[index]));
					}
				} else {
					Self.Sessions.push(new Session(SessionList));
				}
				break;
			case 401:
			// Login failed
			default:
				break;
		}
	};
}


ko.applyBindings(new ViewModel(), document.getElementById('Content'));
