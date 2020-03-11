function ShowMessage(msg) {
	var ErrorMessage = document.getElementById('ErrorMessage');
	ErrorMessage.innerText = msg;
	ErrorMessage.style.display = '';

	setTimeout('ErrorMessage.style.display = "none";', 5000);
}

function Profile(data) {
	var Self = this;
	Self.Id = ko.observable();
	Self.DisplayName = ko.observable();
	Self.EmailAddress = ko.observable();
	Self.PasswordHash = ko.observable();
	Self.Bio = ko.observable();
	Self.Twitter = ko.observable();
	Self.Website = ko.observable();

	if (data) {
		Self.Id(data.Id);
		Self.DisplayName(data.DisplayName);
		Self.EmailAddress(data.EmailAddress);
		Self.Bio(data.Bio);
		Self.Twitter(data.Twitter);
		Self.Website(data.Website);
	}
}

function Session(data) {
	var Self = this;
	Self.Id = ko.observable();
	Self.UserId = ko.observable();
	Self.Title = ko.observable();
	Self.Abstract = ko.observable();
	Self.Notes = ko.observable();
    Self.SessionLength = ko.observable();
    Self.Level = ko.observable();
	Self.Tags = ko.observableArray([]);
    Self.EventId = ko.observable();

	if (data) {
		Self.Id(data.Id);
		Self.UserId(data.UserId);
		Self.Title(data.Title);
		Self.Abstract(data.Abstract);
		Self.Notes(data.Notes);
        Self.SessionLength(data.SessionLength);
        Self.EventId(data.EventId);

        if (data.Tags) {
            for (var index = 0; index < data.Tags.length; ++index) {
                if (ko.isObservable(data.Tags[index])) {
                    Self.Tags.push(data.Tags[index]);
                } else {
                    Self.Tags.push(new Tag(data.Tags[index]));
                }
            }
        }
	}
}

function Tag(data) {
	var Self = this;
	Self.Id = ko.observable(data.Id);
	Self.Text = ko.observable(data.Text);
}

function ViewModel() {
	var Self = this;
	Self.Profile = ko.observable(new Profile());
	Self.Sessions = ko.observableArray([]);
    Self.PastSessions = ko.observableArray([]);
    Self.Levels = ko.observableArray([]);
	Self.Tags = ko.observableArray([]);
	Self.SelectedSession = ko.observable(new Session());
	Self.Verify = ko.observable();

	var ProfileRequest = new XMLHttpRequest();
	ProfileRequest.withCredentials = true;
	ProfileRequest.open('GET', '/api/v1/user/' + sessionStorage.getItem('Id'), true);
	ProfileRequest.send();

	ProfileRequest.onreadystatechange = function () {
		if (ProfileRequest.readyState == ProfileRequest.DONE) {
			switch (ProfileRequest.status) {
				case 200:
					Self.Profile(new Profile(JSON.parse(ProfileRequest.responseText)));
					break;

				case 400:
					// Not logged in
					window.location.href = "/login.html";
					break;

				case 401:
					// Not your profile

				case 404:
					// Profile Not Found

				default:
					break;
			}
		}
	};

	var SessionsRequest = new XMLHttpRequest();
	SessionsRequest.withCredentials = true;
	SessionsRequest.open('GET', '/api/v1/session/user/' + sessionStorage.getItem('Id'), true);
	SessionsRequest.send();
	
	SessionsRequest.onreadystatechange = function () {
		if (SessionsRequest.readyState == SessionsRequest.DONE) {
			switch (SessionsRequest.status) {
				case 200:
					var SessionList = JSON.parse(SessionsRequest.responseText);
                    for (var index = 0; index < SessionList.length; ++index) {
                        // HACK: Hardcoded EventId
						if (SessionList[index].EventId == 2020)
							Self.Sessions.push(new Session(SessionList[index]));
						else
							Self.PastSessions.push(new Session(SessionList[index]));
					}
					break;
	
				case 401:
					// Login failed
	
				default:
					break;
			}
		}
	};
	
	var TagsRequest = new XMLHttpRequest();
	TagsRequest.withCredentials = true;
	TagsRequest.open('GET', '/api/v1/tag', true);
	TagsRequest.send();
	
	TagsRequest.onreadystatechange = function () {
		if (TagsRequest.readyState == TagsRequest.DONE) {
			switch (TagsRequest.status) {
				case 200:
					var TagList = JSON.parse(TagsRequest.responseText);
                    for (var index = 0; index < TagList.length; ++index) {
                        if ( TagList[index].Id < 4 ) {
                            Self.Levels.push(new Tag(TagList[index]));
                        } else {
                            Self.Tags.push(new Tag(TagList[index]));
                        }
                    }
					break;
	
				case 401:
					// Login failed
	
				default:
					break;
			}
		}
	};

	Self.ShowProfile = function () {
		document.getElementById('Profile').style.display = 'block';
		document.getElementById('Session').style.display = 'none';
		document.getElementById('Credentials').style.display = 'none';
		document.getElementById('ErrorMessage').style.display = 'none';
	}

	Self.ShowCredentials = function () {
		document.getElementById('Profile').style.display = 'none';
		document.getElementById('Session').style.display = 'none';
		document.getElementById('Credentials').style.display = 'block';
		document.getElementById('ErrorMessage').style.display = 'none';
	}
	
	Self.ShowSession = function (data) {
		document.getElementById('Profile').style.display = 'none';
		document.getElementById('Session').style.display = 'block';
		document.getElementById('Credentials').style.display = 'none';
		document.getElementById('ErrorMessage').style.display = 'none';

		if (data) {
			Self.SelectedSession(data);
		} else {
			Self.SelectedSession(new Session());
			Self.SelectedSession().Id(-1);
			Self.SelectedSession().UserId(Self.Profile().Id);
		}
	}
	
	Self.SaveProfile = function () {
		var Request = new XMLHttpRequest();
		Request.withCredentials = true;
		Request.open('POST', '/api/v1/user', true);
		Request.setRequestHeader('Content-Type', 'application/json');
		Request.send(ko.toJSON(Self.Profile));

		Request.onreadystatechange = function () {
			if (Request.readyState == Request.DONE) {
				switch (Request.status) {
					case 200:
						ShowMessage('Profile Saved');
						break;

					case 400:
						// Not logged in
						window.location.href = "/login.html";
						break;

					case 401:
						// Not your profile

					case 404:
						// Profile Not Found

					default:
						break;
				}
			}
		};
	}

	Self.SaveCredentials = function () {
		if (Self.Verify() != Self.Profile().PasswordHash()) {
			ShowMessage('Password and Verify did not match');
			return;
		}
	
		var Request = new XMLHttpRequest();
		Request.withCredentials = true;
		Request.open('POST', '/api/v1/user/' + sessionStorage.getItem('Id'), true);
		Request.setRequestHeader('Content-Type', 'application/json');
		Request.send(ko.toJSON(Self.Profile));
	
		Request.onreadystatechange = function () {
			if (Request.readyState == Request.DONE) {
				switch (Request.status) {
					case 200:
						Self.Verify('');
						Self.Profile().PasswordHash('');
						Self.ShowProfile();
						break;
	
					case 400:
						// Login failed
	
					default:
						break;
				}
			}
		};
	}
	
	Self.SaveSession = function () {
		var Request = new XMLHttpRequest();
		Request.withCredentials = true;
		Request.open('POST', '/api/v1/session', true);
		Request.setRequestHeader('Accept', 'application/json');
		Request.setRequestHeader('Content-Type', 'application/json');
		Request.send(ko.toJSON(Self.SelectedSession()));
	
		Request.onreadystatechange = function () {
			if (Request.readyState == Request.DONE) {
				switch (Request.status) {
					case 201:
						Self.Sessions.push(new Session(JSON.parse(Request.responseText)));
	
					case 204:
						Self.ShowProfile();
						break;
	
					default:
						break;
				}
			}
		};
	}
	
	Self.ResubmitSession = function (data) {
		// Hacky way to create a deep clone
		var json = ko.toJSON(data);
		var copy = new Session(JSON.parse(json));
		copy.Id(-1);

		var Request = new XMLHttpRequest();
		Request.withCredentials = true;
		Request.open('POST', '/api/v1/session', true);
		Request.setRequestHeader('Accept', 'application/json');
		Request.setRequestHeader('Content-Type', 'application/json');
		Request.send(ko.toJSON(copy));

		Request.onreadystatechange = function () {
			if (Request.readyState == Request.DONE) {
				switch (Request.status) {
					case 201:
						Self.Sessions.push(new Session(JSON.parse(Request.responseText)));

					case 204:
						Self.ShowProfile();
						break;

					default:
						break;
				}
			}
		};
	}

	Self.DeleteSession = function (data) {
		var Request = new XMLHttpRequest();
		Request.withCredentials = true;
		Request.open('DELETE', '/api/v1/session/' + data.Id(), true);
		Request.send();
	
		Request.onreadystatechange = function () {
			if (Request.readyState == Request.DONE) {
				switch (Request.status) {
					case 204: // It's gone because we deleted it
					case 404: // Never found it in the first place
						Self.Sessions.remove(data);
						Self.Sessions.valueHasMutated();
						break;
	
					case 401: // Not your session to delete
					case 500: // Something happened
					default:
						break;
				}
			}
		};
	}
	
	Self.AddOrRemoveTagToSession = function (data) {
		for (var index = 0; index < Self.SelectedSession().Tags().length; ++index) {
			if (data.Text().toUpperCase() == Self.SelectedSession().Tags()[index].Text().toUpperCase()) {
				Self.SelectedSession().Tags.remove(Self.SelectedSession().Tags()[index]);
				return;
			}
		}
	
		Self.SelectedSession().Tags.push(data);
	}
	
	Self.SaveTag = function () {
		var TagText = document.getElementById('NewTagText').value.trim();
		if (!TagText) return;
	
		for (var index = 0; index < Self.Tags().length; ++index) {
			if (TagText.toUpperCase() == Self.Tags()[index].Text().toUpperCase()) {
				Self.AddOrRemoveTagToSession(Self.Tags()[index]);
				document.getElementById('NewTagText').value = '';
				return;
			}
		}
	
		var RequestJson = {
			Id: -1,
			Text: TagText
		};

		var TagRequest = new XMLHttpRequest();
		TagRequest.withCredentials = true;
		TagRequest.open('POST', '/api/v1/tag', true);
		TagRequest.setRequestHeader('Content-Type', 'application/json');
		TagRequest.send(JSON.stringify(RequestJson));
	
		TagRequest.onreadystatechange = function () {
			if (TagRequest.readyState == TagRequest.DONE) {
				switch (TagRequest.status) {
					case 201:
						var NewTag = new Tag(JSON.parse(TagRequest.responseText));
						Self.Tags.push(NewTag);
						Self.AddOrRemoveTagToSession(NewTag);
						document.getElementById('NewTagText').value = '';
						break;
	
					case 401:
						// Login failed
	
					default:
						break;
				}
			}
		}
	}
}

ko.applyBindings(new ViewModel(), document.getElementById('Content'));