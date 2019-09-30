function Session(data) {
	var Self = this;
	Self.Id = ko.observable();
	Self.Title = ko.observable();
	Self.RoomId = ko.observable();
	Self.TimeSlotId = ko.observable();

	if (data) {
		Self.Id(data.Id);
		Self.RoomId(data.RoomId);
		Self.TimeSlotId(data.TimeSlotId);
		Self.Title(data.Title);
	}
}

function ViewModel() {
	var Self = this;
	Self.Sessions = ko.observableArray([]);
	Self.GetTitle = function (room, timeslot) {
		for (var lcv = 0; lcv < Self.Sessions().length; ++lcv) {
			var ses = Self.Sessions()[lcv];
			if ( (ses.RoomId() === room) && (ses.TimeSlotId() === timeslot) )
				return ses.Title().substring(0, ses.Title().lastIndexOf( ' ', 60 ) );
		}
		return 'Session';
	};

	var SessionsRequest = new XMLHttpRequest();
	SessionsRequest.open('GET', '/api/v1/session', true);
	SessionsRequest.send();

	SessionsRequest.onreadystatechange = function () {
		if (SessionsRequest.readyState == SessionsRequest.DONE) {
			switch (SessionsRequest.status) {
				case 200:
					var SessionList = JSON.parse(SessionsRequest.responseText);
					if (SessionList.length)
						for (var index = 0; index < SessionList.length; ++index)
							Self.Sessions.push(new Session(SessionList[index]));
					else
						Self.Sessions.push(new Session(SessionList));
					break;

				case 401:
					// Login failed

				default:
					break;
			}
		}
	};
}

ko.applyBindings(new ViewModel(), document.getElementById('Content'));
