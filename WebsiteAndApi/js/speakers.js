function Session(data) {
	var Self = this;
	Self.Id = ko.observable();
	Self.Title = ko.observable();
	Self.Link = ko.observable();

	if (data) {
		Self.Id(data.Id);
		Self.Title(data.Title);

		Self.Link('sessions.html?id=' + data.Id);
	}
}

function Profile(data) {
	var Self = this;
	Self.Id = ko.observable();
	Self.DisplayName = ko.observable();
	Self.Password = ko.observable();
	Self.Bio = ko.observable();
	Self.Twitter = ko.observable();
	Self.Website = ko.observable();
	Self.Blog = ko.observable();
	Self.ProfilePicture = ko.observable();
	Self.Sessions = ko.observableArray([]);

	if (data) {
		Self.Id(data.Id);
		Self.DisplayName(data.DisplayName);

		if (data.Bio) {
		    var displayBio = data.Bio.replace(/(?:\r\n|\r|\n)/g, '<br />');
		    Self.Bio(displayBio);
		}

		if (data.Twitter)
			if (data.Twitter[0] != '@')
				data.Twitter = '@' + data.Twitter;

		Self.Twitter(data.Twitter);

		if (data.Website)
			if (data.Website.search('http') == -1)
				data.Website = 'http://' + data.Website;

		Self.Website(data.Website);

		if (data.Blog)
			if (data.Blog.search('http') == -1)
				data.Blog = 'http://' + data.Blog;

		Self.Blog(data.Blog);

		Self.ProfilePicture(data.ProfilePicture);

		for (var index = 0; index < data.Sessions.length; ++index)
			Self.Sessions.push(new Session(data.Sessions[index]));
	}

	Self.TwitterLink = ko.pureComputed(function () {
		var Raw = Self.Twitter();

		if (Raw)
			return 'https://twitter.com/' + Raw.substring(1);
		else
			return null;
	});
}

function ViewModel() {
	var Self = this;
	Self.Profiles = ko.observableArray([]);

	var qd = null;
	if (location.search) {
		qd = {};
		location.search.substr(1).split("&").forEach(function (item) { var s = item.split("="), k = s[0], v = s[1] && decodeURIComponent(s[1]); (k in qd) ? qd[k].push(v) : qd[k] = [v] });
	}

	var ProfileRequest = new XMLHttpRequest();
	if (qd && qd.id)
		ProfileRequest.open('GET', '/api/v1/user/' + qd.id, true);
	else
		ProfileRequest.open('GET', '/api/v1/user', true);
	ProfileRequest.send();

	ProfileRequest.onreadystatechange = function () {
		if (ProfileRequest.readyState == ProfileRequest.DONE) {
			switch (ProfileRequest.status) {
				case 200:
					var ProfileList = JSON.parse(ProfileRequest.responseText);
					if (ProfileList.length)
						for (var index = 0; index < ProfileList.length; ++index)
							Self.Profiles.push(new Profile(ProfileList[index]));
					else
						Self.Profiles.push(new Profile(ProfileList));
					break;

				case 401:
					// Login failed

				default:
					break;
			}
		}
	};
}

ko.applyBindings(new ViewModel(), document.getElementById( 'Content' ));
